// // ----------------------------------------------------------------------------
// // <copyright file="OuterBlendEffect.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;

namespace Kinetix.Internal
{
	/// <summary>
	/// Effect that can blending in-out between their animator and our clip
	/// </summary>
    public class OuterBlendEffect : ABlending, IFrameEffectModify, ISamplerAuthority
	{
		/// <summary>
		/// Status of the blending
		/// </summary>
		private enum BlendMode : byte
		{
			NONE = 0,
			IN,
			OUT
		}

		/// <summary>
		/// Blend duration in seconds
		/// </summary>
		public float blendDuration;
		
		private BlendMode blendMode;
		private float timestampStartBlend;
		private int blendOutFrameToReach;

		/// <param name="blendDuration">
		/// Blend duration in seconds
		/// </param>
		public OuterBlendEffect(float blendDuration = 0.35f)
		{
			this.blendDuration = blendDuration;
		}

		/// <inheritdoc/>
		public void OnAnimationEnd() {}

		/// <inheritdoc/>
		public void OnAnimationStart(KinetixClip clip)
		{
			blendOutFrameToReach = Mathf.FloorToInt((clip.Duration - blendDuration) * clip.FrameRate);
		}

		/// <inheritdoc/>
		public void OnPlayedFrame(ref KinetixFrame finalFrame, in KinetixFrame[] frames, int baseFrameIndex)
		{
			//Both Blend-IN and Blend-OUT use an elapsedTime from 0 to 1.
			float time = Time.time;
			float elapsedTime = (time - timestampStartBlend) / blendDuration;

			//Stop blending when we're done with blending
			//
			//Blend-out is the only one that can overflow
			if (elapsedTime < 0 || elapsedTime > 1 && blendMode != BlendMode.OUT) 
			{
				blendMode = BlendMode.NONE;
			}

			//Execute blending
			if (blendMode != BlendMode.NONE)
			{
				switch (blendMode)
				{
					case BlendMode.IN:
						Blend(ref finalFrame, new KinetixPose[1] { Authority.GetAvatarPos() }, 1- elapsedTime); //Since the Blend-IN goes from 0 to 1 we need to revert it for the lerp
						break;
					case BlendMode.OUT:
						Blend(ref finalFrame, new KinetixPose[1] { Authority.GetAvatarPos() }, elapsedTime);
						break;
				}
			}

			//If we're on the last item and there's no interclip blending
			if (Authority.GetQueue().Count == 0 && frames.Length == 1)
			{
				if (blendMode != BlendMode.OUT && finalFrame.frame >= blendOutFrameToReach)
				{
					SwitchBlend();
					blendMode = BlendMode.OUT;
				}
			}
			//Else we're not supposed to blend-out
			else if (elapsedTime > 0 && blendMode == BlendMode.OUT)
			{
				SwitchBlend();
			}
		}

		/// <summary>
		/// Switch between in and out blend.<br/>
		/// Switching from <see cref="BlendMode.NONE"/> will blend OUT
		/// </summary>
		private void SwitchBlend()
		{
			float time = Time.time;
			if (blendMode == BlendMode.NONE) //If no blend, go to out blend
			{
				blendMode = BlendMode.OUT;
				timestampStartBlend = time;
				return;
			}

			float endTime = timestampStartBlend + blendDuration;

			blendMode = blendMode == BlendMode.IN ? BlendMode.OUT : BlendMode.IN;

			// Math : There's a symetry between the newTimestamp, the current time (where the switch happens) and the end time
			// newTimestamp = switchTime - (endTime - switchTime)
			// newTimestamp = switchTime + switchTime - endTime 
			// newTimestamp = 2 * switchTime - endTime 
			//
			// 1 blendIn
			// |        \         /
			// |         \       /
			// |          \     /
			// |           \   /
			// |            \ /
			// |          switch
			// |            / \
			// |           /   \
			// |          /     \
			// |         /       \
			// 0-newTimestamp--endTime------			
			if (time > endTime)
				timestampStartBlend = time;
			else
				timestampStartBlend = 2 * time - endTime;

		}

		/// <inheritdoc/>
		public void OnQueueEnd()
		{
		}

		/// <inheritdoc/>
		public void OnQueueStart()
		{
			blendMode = BlendMode.IN;
			timestampStartBlend = Time.time;
		}

		/// <inheritdoc/>
		public void Update() {}

        public void OnSoftStop(float stopDelay)
        {
			if (blendMode == BlendMode.OUT) return;
			SwitchBlend();
			blendMode = BlendMode.OUT;
		}
    }
}
