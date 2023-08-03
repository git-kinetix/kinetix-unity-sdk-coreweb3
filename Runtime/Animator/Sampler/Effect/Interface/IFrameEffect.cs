// // ----------------------------------------------------------------------------
// // <copyright file="IFrameEffect.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

namespace Kinetix.Internal
{
    public interface IFrameEffect
	{
		/// <param name="clip">Clip of the animation that is going to start</param>
		public void OnAnimationStart(KinetixClip clip);
		public void OnAnimationEnd  ();
		/// <summary>
		/// Event sent when we're going from 0 clip to 1 clip playing
		/// </summary>
		public void OnQueueStart    ();
		/// <summary>
		/// Event sent when we're going from 1 clip to 0 clip playing
		/// </summary>
		public void OnQueueEnd      ();
		/// <summary>
		/// Update method of unity use it for any needed purpose
		/// </summary>
		public void Update          ();
		/// <summary>
		/// Event sent when "Stop" is called on the sampler
		/// </summary>
		/// <param name="blendTime">Time before which the animation must be stopped</param>
		public void OnSoftStop          (float blendTime);
	}
}
