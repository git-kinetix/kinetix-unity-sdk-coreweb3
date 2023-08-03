// // ----------------------------------------------------------------------------
// // <copyright file="KinetixClipWrapper.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System.Collections.Generic;

namespace Kinetix.Internal
{
	/// <summary>
	/// A clip and its IDs
	/// </summary>
    public readonly struct KinetixClipWrapper
	{
		public readonly KinetixClip clip;
		public readonly AnimationIds animationIds;

		public KinetixClipWrapper(KinetixClip clip, AnimationIds animationIds)
		{
			this.clip = clip;
			this.animationIds = animationIds;
		}

		public static bool operator ==(KinetixClipWrapper wrapper, AnimationIds id) => wrapper.animationIds == id;
		public static bool operator !=(KinetixClipWrapper wrapper, AnimationIds id) => !(wrapper == id);

		public static bool operator ==(KinetixClipWrapper wrapper, KinetixClip clip) => wrapper.clip == clip;
		public static bool operator !=(KinetixClipWrapper wrapper, KinetixClip clip) => !(wrapper == clip);

		public static implicit operator AnimationIds(KinetixClipWrapper wrapper) => wrapper.animationIds;
		public static implicit operator KinetixClip(KinetixClipWrapper wrapper) => wrapper.clip;
		public static implicit operator KinetixClipWrapper(KinetixClip clip) => new KinetixClipWrapper(clip, null);

		public override bool Equals(object obj)
		{
			return
			(obj is AnimationIds id &&
				EqualityComparer<AnimationIds>.Default.Equals(animationIds, id)
			)
				||
			(obj is KinetixClipWrapper wrapper &&
				EqualityComparer<AnimationIds>.Default.Equals(animationIds, wrapper.animationIds)
			);
		}

		public override int GetHashCode()
		{
			int hashCode = 529863572;
			hashCode = hashCode * -1521134295 + EqualityComparer<KinetixClip>.Default.GetHashCode(clip);
			hashCode = hashCode * -1521134295 + EqualityComparer<AnimationIds>.Default.GetHashCode(animationIds);
			return hashCode;
		}
	}
}
