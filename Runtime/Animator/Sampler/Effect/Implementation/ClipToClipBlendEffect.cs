// // ----------------------------------------------------------------------------
// // <copyright file="ClipToClipBlendEffect.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;

namespace Kinetix.Internal
{
	/// <summary>
	/// An effect that blends <see cref="KinetixClip"/> between them
	/// </summary>
	public class ClipToClipBlendEffect : ABlending, IFrameEffectModify, ISamplerAuthority
	{
		// How does it work :
		//
		//                blend        blend
		//          0    |......1     |......2
		// clip 1 : |-----------|    
		// clip 2 :      |-------------------|
		// clip 3 :                   |-----------|

		private bool isPlaying = false;

		/// <summary>
		/// Blend duration in seconds
		/// </summary>
		public float blendDuration;

		private KinetixClip current;
		private int frameToReach;
		private bool isBlendingCurrentClip;
		private float timestampStartBlend;

		/// <param name="blendDuration">Blend duration in seconds</param>
		public ClipToClipBlendEffect(float blendDuration = 1f)
		{
			this.blendDuration = blendDuration;
		}

		/// <inheritdoc/>
		public void OnAnimationEnd()
		{

		}

		/// <inheritdoc/>
		public void OnAnimationStart(KinetixClip clip)
		{
			current = clip;
			frameToReach = Mathf.FloorToInt((clip.Duration - blendDuration) * clip.FrameRate);
			isBlendingCurrentClip = false;
		}

		/// <inheritdoc/>
		public void OnPlayedFrame(ref KinetixFrame finalFrame, in KinetixFrame[] frames, int baseFrameIndex)
		{
			if (!isPlaying) return;

			int length = frames.Length;
			if (length > 1)
			{
				float elapsedTime = (Time.time - timestampStartBlend) / blendDuration;
				Blend(ref finalFrame, frames, elapsedTime, baseFrameIndex);
			}

			if (finalFrame.clip == current && !isBlendingCurrentClip && finalFrame.frame >= frameToReach)
			{
				isBlendingCurrentClip = true;
				timestampStartBlend = Time.time;
				Authority.StartNextClip(true);
			}
		}

		/// <inheritdoc/>
		public void OnQueueEnd()
		{
			isPlaying = false;
			current = null;
			frameToReach = -1;
			isBlendingCurrentClip = false;
		}

		/// <inheritdoc/>
		public void OnQueueStart()
		{
			isPlaying = true;
		}

		/// <inheritdoc/>
		public void Update() {}

        public void OnSoftStop(float blendTime)
        {
        }
    }
}
