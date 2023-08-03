// // ----------------------------------------------------------------------------
// // <copyright file="NetworkPoseSampler.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kinetix.Internal
{

	internal class NetworkPoseSampler
	{   
		// ====================================================================== //
		//  EVENTS                                                                //
		// ====================================================================== //
		public   event Action                       OnQueueStart    ;
		public   event Action                       OnQueueStop     ;
		public   event Action<KinetixFrame>         OnPlayedFrame   ;
		internal event Func<IEnumerator, Coroutine> RequestCoroutine;
		// ---------------------------------------------------------------------- //

		private const float LERP_SMOOTHNESS = 0.25f;

		private Queue<KinetixNetworkedPose> targetTimestampPoseQueue;
		private KinetixNetworkedPose currentPose;
		private KinetixFrame previousSentPos;

		private double startOffset = -1f;
		private double lastTimestamp = -1f;
		private double currentTimestamp = -1f;
		
		public bool IsPlaying => isPlaying;
		private bool isPlaying = false;

		private Guid lastAnimGuid = Guid.Empty;
		private Guid currentAnimGuid = Guid.Empty;

		private Coroutine interpolationRoutine = null;
		private BonePoseInfo[] bonePoseInfos;

		private KinetixNetworkConfiguration config;

		private HumanBodyBones[] bones;

		internal NetworkPoseSampler(HumanBodyBones[] bones)
        {
			targetTimestampPoseQueue = new Queue<KinetixNetworkedPose>();

			this.bones = bones;
			
			config = KinetixCoreBehaviour.ManagerLocator.Get<NetworkManager>().Configuration;
			bonePoseInfos = new BonePoseInfo[bones.Length];
		}

		#region LOCAL
		/// <summary>
		/// Notify the network pose sampler that 
		/// </summary>
		internal void StartPose()
		{
			isPlaying = true;
			currentAnimGuid = Guid.NewGuid();
		}

		internal void StopPose()
		{
			isPlaying = false;
			lastAnimGuid = currentAnimGuid;
			currentAnimGuid = Guid.Empty;
		}

		/// <summary>
		/// Translate a frame into a pose to export it in remote
		/// </summary>
		/// <param name="frame"></param>
		/// <returns></returns>
		public KinetixNetworkedPose GetPose(KinetixFrame frame)
		{
			if (!isPlaying)
			{
				return null;
			}

			if (bones == null || bones.Length == 0)
			{
				return null;
			}

			//Sort in the bone order of the NetworkPoseSampler
			TransformData[] transforms =
				frame.humanTransforms
					.Select((h, i) => new KeyValuePair<TransformData, int>(h, i))
					.OrderBy(OrderByHandler)
					.Select(h => h.Key)
					.ToArray();

			float[] blendshape = frame.blendshapes.ToArray();

			KinetixNetworkedPose toReturn = KinetixNetworkedPose.FromClip(bones, transforms, blendshape, frame.armature, frame.frame, config.SendLocalPosition, config.SendLocalScale);

			int OrderByHandler(KeyValuePair<TransformData, int> b)
				=> Array.IndexOf(bones, frame.bones[b.Value]);
            
			toReturn.guid = currentAnimGuid;
			return toReturn;
		}

		#endregion

		public void ApplyPose(KinetixNetworkedPose pose, double timestamp)
		{
			isPlaying = pose != null;
			
			// If suddenly we receive a null pose, that means the anim is finished
			if (!isPlaying) 
			{
				lastAnimGuid = currentAnimGuid;
				currentAnimGuid = Guid.Empty;
				return;
			}
			
			// If we receive frames of a killed anim, ignore and return
			if (pose.guid == lastAnimGuid) return;

			currentAnimGuid = pose.guid;
			pose.timestamp = timestamp;
	
			// Start the coroutine and reset params
			if (interpolationRoutine == null) 
			{
				startOffset = 0;
				lastTimestamp = timestamp;
				currentTimestamp = timestamp;
				currentPose = null;
				targetTimestampPoseQueue = new Queue<KinetixNetworkedPose>();

				interpolationRoutine = RequestCoroutine(InterpolateBonePosition());
			}

			// This is the queue for all anim frames and their timestamp
			targetTimestampPoseQueue.Enqueue(pose);
		}
		
		private IEnumerator InterpolateBonePosition()
		{
			float ellapsedSinceLast = 0;
	
			while (true)
			{
				// Increase timers
				float deltaTime = Time.deltaTime;
				currentTimestamp += deltaTime;
				ellapsedSinceLast += deltaTime;

				// Cumulate some upfront frames before triggering
				if (targetTimestampPoseQueue.Count < config.TargetFrameCacheCount && isPlaying) 
				{
					
					// If we waited the configured delay for some more frames and nothing is coming, kill the anim
					if (currentPose != null && ellapsedSinceLast > config.MaxWaitTimeBeforeEmoteExpiration) 
					{
						isPlaying = false;
						OnQueueStop?.Invoke();
						break;
					}

					if (currentPose == null) 
					{
						startOffset += deltaTime;

						yield return new WaitForEndOfFrame();
						continue;
					}
				}

				// Get the next frame
				if (currentPose == null || currentPose.timestamp + startOffset - lastTimestamp <= currentTimestamp - lastTimestamp) 
				{

					// If it was the last frame, ciao bye 
					if (targetTimestampPoseQueue.Count == 0) 
					{
						OnQueueStop?.Invoke();
						break;
					}

					// If we are not at the first frame of the anim, just save the previous timestamp and set the ellapsed time to 0
					if (currentPose != null) 
					{
						lastTimestamp = currentPose.timestamp;
						ellapsedSinceLast = 0;
					}

					bool isNull = currentPose == null;
					currentPose = targetTimestampPoseQueue.Dequeue();

					if (isNull)
						OnQueueStart?.Invoke();
				}

				// Calculate interpolation ratio
				float fRatio = Mathf.Abs(LERP_SMOOTHNESS);

				KinetixFrame toSend = new KinetixFrame(new TransformData[bones.Length], bones, currentPose.blendshapes); //Assume blendshape don't need interpolation
				TransformData previousPos;
				BonePoseInfo curentPos;

				//Assume armature don't need interpolation
				if (currentPose.hasArmature)
					toSend.armature = currentPose.armature;
				else
					toSend.armature = null;

				// Apply interpolation for each bone
				for (int boneIndex = 0; boneIndex < currentPose.bones.Length; boneIndex++)
				{
					if (currentPose.bones[boneIndex] != null && currentPose.bones[boneIndex].localPosition != null) 
					{
                        curentPos = currentPose.bones[boneIndex];
						if (previousSentPos == null)
						{
							toSend.humanTransforms[boneIndex] = new TransformData()
							{
								rotation = curentPos.localRotation,
								position = currentPose.posEnabled || boneIndex == 0
									? curentPos.localPosition
									: (Vector3?)null,
								scale = currentPose.scaleEnabled ?
									curentPos.localScale
									: (Vector3?)null,
							};
						}
						else
						{
							previousPos = previousSentPos.humanTransforms[boneIndex];
                        
							//Lerp between previous pos (or current if there is no previous pos) to current pos
							toSend.humanTransforms[boneIndex] = new TransformData()
							{
								rotation = Quaternion.Lerp(previousPos.rotation.GetValueOrDefault(curentPos.localRotation), curentPos.localRotation, fRatio),
								position = currentPose.posEnabled || boneIndex == 0
									? Vector3.Lerp(previousPos.position.GetValueOrDefault(curentPos.localPosition), curentPos.localPosition, fRatio)
									: (Vector3?)null,
								scale = currentPose.scaleEnabled
									? Vector3.Lerp(previousPos.scale.GetValueOrDefault(curentPos.localScale), curentPos.localScale, fRatio)
									: (Vector3?)null,
							};
						}
					}
				}

				OnPlayedFrame?.Invoke(toSend);
				previousSentPos = toSend;

				yield return new WaitForEndOfFrame();
			}
			

			//if (currentAnimGuid != Guid.Empty) 
			//{
			//	lastAnimGuid = currentAnimGuid;
			//	currentAnimGuid = Guid.Empty;
			//}
			
			// As clip blending is accounted for in the networked pose, we can just enable back the animator
			//animator.enabled = true;
			interpolationRoutine = null;
			targetTimestampPoseQueue = new Queue<KinetixNetworkedPose>();
		}
	}
}
