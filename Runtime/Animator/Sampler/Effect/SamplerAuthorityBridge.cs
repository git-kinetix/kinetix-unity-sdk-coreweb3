// // ----------------------------------------------------------------------------
// // <copyright file="SamplerAuthorityBridge.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using Kinetix.Internal.Utils;
using System.Collections.Generic;

namespace Kinetix.Internal
{
	/// <summary>
	/// Authority on the samplers.<br/>
	/// You can call some methods like "StartNextClip" or "GetAvatarPos".
	/// </summary>     
	public class SamplerAuthorityBridge
	{
		/// <param name="additive">If true, don't stop the current animation</param>
		public   delegate void StartNextDelegate(bool additive);
		/// <returns>Returns the current pose of the avatar without the kinetix animation</returns>
		public   delegate KinetixPose GetAvatarPosDelegate();
		/// <returns>Returns the queue of remaining animations to play</returns>
		public   delegate Queue<KinetixClipWrapper> GetQueueDelegate();
		/// <param name="index">Sampler index</param>
		/// <returns>Returns the clip currently playing on the sampler n° <paramref name="index"/></returns>
		public   delegate KinetixClip GetClipDelegate(int index);
		/// <returns></returns>
		public   delegate KinetixClipSampler CreateSamplerDelegate();
		/// <returns>Returns a pool item with custom transforms of the avatar in TPose</returns>
		internal delegate SkeletonPool.PoolItem GetAvatarDelegate();

		/// <summary>
		/// Start a new clip on the sampler
		/// </summary>
		public   StartNextDelegate     StartNextClip;
		/// <summary>
		/// Get the current pose of the avatar without the kinetix animation
		/// </summary>
		public   GetAvatarPosDelegate  GetAvatarPos;
		/// <summary>
		/// Get the queue of remaining animations to play
		/// </summary>
		public   GetQueueDelegate      GetQueue;
		/// <summary>
		/// Get the clip currently playing on the sampler at index
		/// </summary>
		public   GetClipDelegate       GetClip;
		/// <summary>
		/// Create a <see cref="KinetixClipSampler"/> setup for external purposes
		/// </summary>
		public   CreateSamplerDelegate CreateSampler;
		/// <summary>
		/// Get the TPose transform hierarchy of the avatar
		/// </summary>
		/// <remarks>
		/// Note: The pool item must be disposed after use
		/// </remarks>

		internal GetAvatarDelegate     GetAvatar;
	}
}
