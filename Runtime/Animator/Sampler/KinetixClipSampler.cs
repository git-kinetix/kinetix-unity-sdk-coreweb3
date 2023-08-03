// // ----------------------------------------------------------------------------
// // <copyright file="KinetixClipSampler.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;

namespace Kinetix.Internal
{
	public class KinetixClipSampler
	{
		public delegate KinetixFrame SampleFrame(KinetixClip clip, int frame);
		public SampleFrame SampleFrameHandler;

		public KinetixFrame previousFrame;

		private KinetixClipWrapper toSample;
		private int currentFrame = -1;

		public bool isMain = false;

		private bool ended = true;
		public bool Ended => ended;

		private double elapsedTime = 0;
		public double ElapsedTime => elapsedTime;

		public void Play(KinetixClipWrapper toSample)
		{
			this.toSample = toSample;
			ended = false;
			currentFrame = -1;
			elapsedTime = 0;
		}

		public void Play(KinetixClipWrapper toSample, int currentFrame)
		{
			this.toSample = toSample;
			ended = false;
			this.currentFrame = currentFrame;
			elapsedTime = currentFrame / toSample.clip.FrameRate;
		}

		public void Play(KinetixClipWrapper toSample, float time)
		{
			this.toSample = toSample;
			ended = false;
			this.currentFrame = (int)(time * toSample.clip.FrameRate);
			elapsedTime = time;
		}

		public KinetixClipWrapper Clip => toSample;

		public KinetixFrame Update(float deltaTime)
		{
			if (ended) return null;

			KinetixFrame toReturn = null;
			int frame = (int)Math.Floor(elapsedTime * (double)toSample.clip.FrameRate); //s * f/s = f
			if (frame >= 0 && frame < toSample.clip.KeyCount)
			{
				if (frame > currentFrame || previousFrame == null)
				{
					currentFrame = frame;
					previousFrame
						= toReturn
						= SampleFrameHandler(toSample, currentFrame);
				}
				else if (!isMain)
				{
					toReturn = previousFrame;
				}
				//If current frame isn't different and the sample is MAIN, the sample returns null
				//this is usefull to calibrate every sampler on MAIN's framerate (main will return null when it doesn't need 
			}
			else
			{
				ended = true;
			}

			elapsedTime += deltaTime;
			return toReturn;
		}
	}
}
