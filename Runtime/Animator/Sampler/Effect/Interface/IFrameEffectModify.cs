// // ----------------------------------------------------------------------------
// // <copyright file="IFrameEffectModify.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

namespace Kinetix.Internal
{
	/// <summary>
	/// Inherit this interface to create an effect that can modify frame before sending
	/// </summary>
	public interface IFrameEffectModify : IFrameEffect
	{
		/// <summary>
		/// Event sent when a frame has been played
		/// </summary>
		/// <param name="finalFrame">The cloned frame. You can modify informations in it</param>
		/// <param name="frames">Original frames. A null frame in the array means that the frame is the same as the previously sent</param>
		/// <param name="baseFrameIndex">Index on which <paramref name="finalFrame"/> is based</param>
		public void OnPlayedFrame(ref KinetixFrame finalFrame, in KinetixFrame[] frames, int baseFrameIndex);
	}
}
