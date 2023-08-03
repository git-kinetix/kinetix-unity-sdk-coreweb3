// // ----------------------------------------------------------------------------
// // <copyright file="KinetixCharacterComponentLocal.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using UnityEngine;
using Kinetix.Internal;
using System.Collections.Generic;
using Kinetix.Internal.Utils;
using Kinetix.Internal.Retargeting;

namespace Kinetix
{
	/// <summary>
	/// A local character. It can play animation directly on the avatar.<br/>
	/// It's also provide the <see cref="KinetixNetworkedPose"/> to be sent to the remote peers.
	/// </summary>
	public class KinetixCharacterComponentLocal : KinetixCharacterComponent
	{
		const float OUTER_BLEND_DURATION = 0.35f;

		private SimulationSampler sampler;
		private KinetixFrame currentFrame;

		/// <inheritdoc/>
		public override void Init(KinetixAvatar kinetixAvatar, RootMotionConfig _RootMotionConfig)
		{
			base.Init(kinetixAvatar, _RootMotionConfig);
			sampler = new SimulationSampler();
			sampler.OnQueueStart              += Sampler_OnQueueStart             ;
			sampler.OnQueueStop               += Sampler_OnQueueStop              ;
			sampler.OnAnimationStart          += Sampler_OnAnimationStart         ;
			sampler.OnAnimationStop           += Sampler_OnAnimationStop          ;
			sampler.OnPlayedFrame             += Sampler_OnPlayedFrame            ;
			sampler.RequestAdaptToInterpreter += Sampler_RequestAdaptToInterpreter;
			sampler.RequestAvatarPos          += Sampler_RequestAvatarPos         ;
			sampler.RequestAvatar             += Sampler_RequestAvatar            ;

			sampler.Effect.RegisterEffect(new ClipToClipBlendEffect());
			sampler.Effect.RegisterEffect(new OuterBlendEffect(OUTER_BLEND_DURATION));
			sampler.Effect.RegisterEffect(new BlendCancelBetweenClipEffect());
			sampler.Effect.RegisterEffect(new RootMotionEffect(_RootMotionConfig));

			//sampler.bones = characterBones;

			currentFrame = null;
		}

		/// <summary>
		/// Get the current frame animation if it exists.<br/>
		/// See also <seealso cref="IsPoseAvailable"/> to know if a pose is available
		/// </summary>
		/// <returns>Returns the pose in a sampleable format</returns>
		public KinetixFrame GetRawPose() => currentFrame;
		
		/// <summary>
		/// Get the raw pose in a format suitable for the network
		/// </summary>
		/// <returns>Returns the pose in a network format</returns>
		public byte[] GetSerializedPose()
		{
			return GetNetworkedPose()?.ToByteArray();
		}

		/// <summary>
		/// Get the raw pose in a format suitable for the network
		/// </summary>
		/// <returns>Returns the pose in a network format</returns>
		protected KinetixNetworkedPose GetNetworkedPose()
		{
			if (!IsPoseAvailable()) 
				return null;

			return networkSampler.GetPose(currentFrame);
		}

		/// <inheritdoc/>
		public override bool IsPoseAvailable() => currentFrame != null && networkSampler.IsPlaying;
	
		/// <summary>
		/// Play an animation on the current player
		/// </summary>
		/// <param name="_AnimationIds">IDs of the animation</param>
		public async void PlayAnimation(AnimationIds _AnimationIds)
		{
			if (_AnimationIds?.UUID == null)
			{
				KinetixDebug.LogWarning("Animation ID cannot be null when Play Animation");
				return;
			}

			KinetixEmote emote = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_AnimationIds);
			KinetixClip clip = await KinetixCoreBehaviour.ServiceLocator.Get<RetargetingService>().GetRetargetedClipByAvatar<KinetixClip, KinetixClipExporter>(emote, kinetixAvatar, SequencerPriority.VeryHigh, true);

			if (clip == null)
			{
				KinetixDebug.LogWarning("Can't get the animation " + _AnimationIds.UUID);
				return;
			}
			
			sampler.Play(new KinetixClipWrapper(clip, _AnimationIds));
		}

		public void StopAnimation()
		{
			sampler.SoftStop(OUTER_BLEND_DURATION);
		}

		/// <summary>
		/// Play a sequence of animation on the current player
		/// </summary>
		/// <param name="_AnimationIdsArray">IDs of each animation in the play order</param>
		public async void PlayAnimationQueue(AnimationIds[] _AnimationIdsArray)
		{
			int loaded = 0;
			int animeCount = _AnimationIdsArray.Length;
			KinetixClipWrapper[] clips = new KinetixClipWrapper[animeCount];

			for (int i = 0; i < animeCount; i++)
			{
				var _AnimationIds = _AnimationIdsArray[i];

				KinetixEmote emote = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_AnimationIds);
				KinetixClip clip = await KinetixCoreBehaviour.ServiceLocator.Get<RetargetingService>().GetRetargetedClipByAvatar<KinetixClip, KinetixClipExporter>(emote, kinetixAvatar, SequencerPriority.VeryHigh, true);

				if (clip == null)
				{
					KinetixDebug.LogWarning("Can't get the animation " + _AnimationIds.UUID);
					continue;
				}

				clips[i] = new KinetixClipWrapper(clip, _AnimationIds);
				loaded++;
				
				if (loaded == animeCount)
				{
					sampler.PlayRange(clips);
				}
			}
		}

		/// <inheritdoc/>
		protected override void Update()
		{
			sampler?.Update();
		}

		#region Sampler Event
		private void Sampler_RequestAdaptToInterpreter(KinetixFrame obj)
		{
			obj.AdaptToInterpreter(poseInerpretor[0]);
		}

		private void Sampler_OnPlayedFrame(KinetixFrame obj)
		{
			currentFrame = obj;

			if (AutoPlay)
			{
				int count = poseInerpretor.Count;
				for (int i = 0; i < count; i++)
				{
					obj.Sample(poseInerpretor[i]);
				}
			}

			Call_OnPlayedFrame();
		}

		private void Sampler_OnAnimationStart(KinetixClipWrapper obj)
		{
			if (AutoPlay)
			{
				int count = poseInerpretor.Count;
				for (int i = 0; i < count; i++)
				{
					if (poseInerpretor[i] is IPoseInterpreterStartEnd startEnd) startEnd.AnimationStart(obj.clip);
				}
			}

			Call_OnAnimationStart(obj.animationIds);
		}

		private void Sampler_OnAnimationStop(KinetixClipWrapper obj)
		{
			currentFrame = null;
		
			if (AutoPlay)
			{
				int count = poseInerpretor.Count;
				for (int i = 0; i < count; i++)
				{
					if (poseInerpretor[i] is IPoseInterpreterStartEnd startEnd) startEnd.AnimationEnd(obj.clip);
				}
			}

			Call_OnAnimationEnd(obj.animationIds);
		}

		private void Sampler_OnQueueStart()
		{
			networkSampler.StartPose();

			if (AutoPlay)
			{
				int count = poseInerpretor.Count;
				for (int i = 0; i < count; i++)
				{
					if (poseInerpretor[i] is IPoseInterpreterStartEnd startEnd) startEnd.QueueStart();
				}
			}
		}

		private void Sampler_OnQueueStop()
		{
			networkSampler.StopPose();

			if (AutoPlay)
			{
				int count = poseInerpretor.Count;
				for (int i = 0; i < count; i++)
				{
					if (poseInerpretor[i] is IPoseInterpreterStartEnd startEnd) startEnd.QueueEnd();
				}
			}
		}

		private SkeletonPool.PoolItem Sampler_RequestAvatar()
			=> RetargetTableCache.GetTableSync(kinetixAvatar.Avatar).GetClone();

		private KinetixPose Sampler_RequestAvatarPos()
		{
			if (poseInerpretor.Count == 0)
			{
				return new KinetixPose(new TransformData[0], new HumanBodyBones[0], null, default, default);
			}

			return poseInerpretor[0].GetPose();
		}
		#endregion

		public override void Dispose()
		{
			base.Dispose();
			sampler.OnQueueStart     -= Sampler_OnQueueStart     ;
			sampler.OnQueueStop      -= Sampler_OnQueueStop      ;
			sampler.OnAnimationStart -= Sampler_OnAnimationStart ;
			sampler.OnAnimationStop  -= Sampler_OnAnimationStop  ;
			sampler.OnPlayedFrame    -= Sampler_OnPlayedFrame    ;
			sampler.RequestAvatarPos -= Sampler_RequestAvatarPos ;
			sampler.RequestAvatar    -= Sampler_RequestAvatar    ;

			sampler.Dispose();
		}
	}
}
