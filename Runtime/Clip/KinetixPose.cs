// // ----------------------------------------------------------------------------
// // <copyright file="KinetixPose.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Kinetix
{
    /// <summary>
    /// Snapshoot of a human pose.
    /// </summary>
    public class KinetixPose
	{
		public TransformData? root;
		public TransformData? armature;

		/// <summary>
		/// Human bones
		/// </summary>
		public HumanBodyBones[] bones;
		/// <summary>
		/// Transforms corrisponding to the bones (ex: if bones[0] is hips, humanTransforms[0] is the transform for the hips)
		/// </summary>
		public TransformData[] humanTransforms;
		/// <summary>
		/// List of blendshapes
		/// </summary>
		public BlendshapeArray blendshapes;

		protected KinetixPose() {}
		public KinetixPose(TransformData[] transforms, HumanBodyBones[] bones, IEnumerable<float> blendshapes, TransformData? root, TransformData? armature)
		{
			humanTransforms = transforms;
			this.bones = bones;
			this.root = root;
			this.armature = armature;
			this.blendshapes = blendshapes == null ? new BlendshapeArray() : new BlendshapeArray(blendshapes);
		}
	}
}
