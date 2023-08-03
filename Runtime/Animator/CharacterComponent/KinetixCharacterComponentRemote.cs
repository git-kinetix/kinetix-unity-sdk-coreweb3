// // ----------------------------------------------------------------------------
// // <copyright file="KinetixCharacterComponentRemote.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;
using Kinetix.Internal;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Kinetix
{
	/// <summary>
	/// A remote character. It recieves poses from the network and apply them to the character.
	/// </summary>
	public class KinetixCharacterComponentRemote : KinetixCharacterComponent
	{
		private KinetixNetworkedPose currentFrame;

		///<inheritdoc/>
		public override void Init(KinetixAvatar kinetixAvatar, RootMotionConfig rootMotionConfig)
		{
			base.Init(kinetixAvatar, rootMotionConfig);
			networkSampler.RequestCoroutine += NetworkSampler_RequestCoroutine;
			networkSampler.OnPlayedFrame += NetworkSampler_OnPlayedFrame;
			networkSampler.OnQueueStart += NetworkSampler_OnQueueStart;
			networkSampler.OnQueueStop += NetworkSampler_OnQueueStop;
		}

		///<inheritdoc/>
		public override bool IsPoseAvailable() => currentFrame != null;

		/// <summary>
		/// Get the raw pose in a format suitable for the network
		/// </summary>
		/// <returns>Returns the pose in a network format</returns>
		public KinetixNetworkedPose GetSerialisedPose()
		{
			if (!IsPoseAvailable())
				return null;

			return currentFrame;
		}

		/// <summary>
		/// Apply a pose from the network on the current avatar.<br/>
		/// If <see cref="KinetixCharacterComponent.AutoPlay"/> is false, it shall be handled via the events
		/// </summary>
		/// <param name="pose">The network pose of the animation in byte</param>
		/// <param name="timestamp">Timestamp at which the pose has been recieved</param>
		public void ApplySerializedPose(byte[] pose, double timestamp)
		{
			ApplySerializedPose(KinetixNetworkedPose.FromByte(pose), timestamp);
		}

		/// <summary>
		/// Apply a pose from the network on the current avatar.<br/>
		/// If <see cref="KinetixCharacterComponent.AutoPlay"/> is false, it shall be handled via the events
		/// </summary>
		/// <param name="pose">The network pose of the animation</param>
		/// <param name="timestamp">Timestamp at which the pose has been recieved</param>
		public void ApplySerializedPose(KinetixNetworkedPose pose, double timestamp)
		{
			currentFrame = pose;
			if (AutoPlay)
			{
				networkSampler.ApplyPose(pose, timestamp);
			}
			else
			{
				Call_OnPlayedFrame();
			}
		}

		#region Sampler events
		private Coroutine NetworkSampler_RequestCoroutine(System.Collections.IEnumerator arg)
			=> behaviour.StartCoroutine(arg);

		private void NetworkSampler_OnPlayedFrame(KinetixFrame obj)
		{
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
		private void NetworkSampler_OnQueueStart()
		{
			if (AutoPlay)
			{
				int count = poseInerpretor.Count;
				for (int i = 0; i < count; i++)
				{
					if (poseInerpretor[i] is IPoseInterpreterStartEnd startEnd)
					{
						startEnd.QueueStart();
						startEnd.AnimationStart(null);
					}
				}
			}

			Call_OnAnimationStart(null);
		}

		private void NetworkSampler_OnQueueStop()
		{
			networkSampler.StopPose();

			if (AutoPlay)
			{
				int count = poseInerpretor.Count;
				for (int i = 0; i < count; i++)
				{
					if (poseInerpretor[i] is IPoseInterpreterStartEnd startEnd)
					{
						startEnd.AnimationEnd(null);
						startEnd.QueueEnd();
					}
				}
			}

			Call_OnAnimationEnd(null);
		}
		#endregion

		public override void Dispose()
		{
			base.Dispose();

			networkSampler.RequestCoroutine += NetworkSampler_RequestCoroutine;
			networkSampler.OnPlayedFrame += NetworkSampler_OnPlayedFrame;
			networkSampler.OnQueueStart += NetworkSampler_OnQueueStart;
			networkSampler.OnQueueStop += NetworkSampler_OnQueueStop;
		}
	}
}
