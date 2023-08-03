// // ----------------------------------------------------------------------------
// // <copyright file="IFrameEffectAdd.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;

namespace Kinetix.Internal
{
	/// <summary>
	/// Inherit this interface to create an effect that can generate frames outside of the sampling
	/// </summary>
	public interface IFrameEffectAdd : IFrameEffect
	{
		/// <summary>
		/// Send this event to add a frame outside of the sampling
		/// </summary>
		public event Action<KinetixFrame> OnAddFrame;
	}
}
