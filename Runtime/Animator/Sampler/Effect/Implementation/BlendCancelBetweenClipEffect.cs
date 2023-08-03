// // ----------------------------------------------------------------------------
// // <copyright file="BlendCancelBetweenClipEffect.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;

namespace Kinetix.Internal
{
	/// <summary>
	/// A blending for when the player instantly play a <see cref="KinetixClip"/> while a <see cref="KinetixClip"/> is playing
	/// </summary>
	public class BlendCancelBetweenClipEffect : ABlending, IFrameEffectModify, ISamplerAuthority
	{
		public float blendDuration;

		private KinetixClipSampler sampler;
		private KinetixClip currentClip;
		private float currentClipTimestamp;
		private float previousQueueEndTimestamp;
		private float blendCountdown;
		private KinetixFrame frame;
		private float updateTimestamp;

		const float THRESHOLD = 0.05f;

        public BlendCancelBetweenClipEffect(float blendDuration = 0.35f)
        {
            this.blendDuration = blendDuration;
        }

        public void OnAnimationStart(KinetixClip clip)
		{
			updateTimestamp = currentClipTimestamp = Time.time;
			currentClip = clip;
		}

		public void OnAnimationEnd()
		{
			frame = null;
		}

		public void OnPlayedFrame(ref KinetixFrame finalFrame, in KinetixFrame[] frames, int baseFrameIndex)
		{
			if (sampler == null)
			{
				frame = null;
				return;
			}

			float timestemp = Time.time;
			float deltaTime = timestemp - updateTimestamp;
			updateTimestamp = timestemp;

			frame = sampler.Update(deltaTime);

			if (sampler.Ended)
			{
				frame = null;
				sampler = null;
				return;
			}

			float lerp = blendCountdown / blendDuration;
            Blend(ref finalFrame, new KinetixFrame[] { frame }, lerp);

			blendCountdown -= deltaTime;

			if (blendCountdown < 0)
			{
				sampler = null;
			}
		}

		public void OnQueueEnd()
		{
			previousQueueEndTimestamp = Time.time;
            float clipTime = previousQueueEndTimestamp - currentClipTimestamp;

			if (clipTime >= currentClip.Duration)
			{
				sampler = null;
				currentClip = null;
				return;
			}

			sampler = Authority.CreateSampler();
			blendCountdown = blendDuration;
            sampler.Play(new KinetixClipWrapper(currentClip, null), clipTime);
			currentClip = null;
		}

		public void OnQueueStart()
		{
			float deltaTime = Time.time - previousQueueEndTimestamp;
			if (deltaTime > THRESHOLD)
			{
				sampler = null;
			}
		}

		public void Update()
		{
		}

		public void OnSoftStop(float blendTime)
		{

		}
	}
}
