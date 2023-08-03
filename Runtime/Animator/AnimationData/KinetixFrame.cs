// // ----------------------------------------------------------------------------
// // <copyright file="KinetixFrame.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kinetix.Internal
{
	/// <summary>
	/// A frame of a <see cref="KinetixAnimation"/>
	/// </summary>
	public class KinetixFrame : KinetixPose
	{
		/// <summary>
		/// Clip that originate the frame. Can be null.
		/// </summary>
		public readonly KinetixClip clip;
		/// <summary>
		/// See <see cref="KinetixClip.resetKeys"/>.<br/>
		/// Null when this is not the first frame (= <see cref="frame"/> 0)
		/// </summary>
		public readonly Dictionary<string, TransformData> resetKeys;
		public readonly int frame;

		/// <param name="pose">Pose to duplicate as a frame</param>
		public KinetixFrame(KinetixPose pose) : base()
		{
			humanTransforms = (TransformData[])pose.humanTransforms.Clone();
			bones           = (HumanBodyBones[])pose.bones.Clone();
			blendshapes     = (BlendshapeArray)pose.blendshapes.Clone();
			armature        = pose.armature;
			root            = pose.root;

			clip       = null;
			resetKeys  = new Dictionary<string, TransformData>();
			frame = -1;
		}

		/// <param name="frame">Frame to duplicate</param>
		public KinetixFrame(KinetixFrame frame) : base()
		{
			humanTransforms = (TransformData[])frame.humanTransforms.Clone();
			bones           = (HumanBodyBones[])frame.bones.Clone();
			blendshapes     = (BlendshapeArray)frame.blendshapes.Clone();
			armature		= frame.armature;
			root            = frame.root;

			clip            = frame.clip;
			resetKeys       = frame.resetKeys;
			this.frame      = frame.frame;
		}

		/// <param name="transforms">Transforms of the frame (see: <see cref="KinetixPose.humanTransforms")/></param>
		/// <param name="bones">Bones corrisponding to the transforms (see: <see cref="KinetixPose.bones")/></param>
		public KinetixFrame(TransformData[] transforms, HumanBodyBones[] bones, IEnumerable<float> blendshapes) : base(transforms, bones, blendshapes, default, default)
		{
			resetKeys = null;
			frame = -1;
			clip = null;
		}

		/// <param name="humanBones">Bones in the animation for cache (see: <see cref="KinetixPose.bones")/></param>
		/// <param name="clip">Clip of the animation</param>
		/// <param name="frame">Frame to extract from the animation</param>
		public KinetixFrame(out HumanBodyBones[] humanBones, KinetixClip clip, int frame) : base()
		{
			this.clip = clip;
			this.frame = frame;

			int count = clip.humanKeys.Count;
			humanBones = new HumanBodyBones[count];

			bones = new HumanBodyBones[count];
			humanTransforms = new TransformData[count];
			blendshapes = clip.blendshapeKeys.Count == 0 ? new BlendshapeArray() : new BlendshapeArray(clip.blendshapeKeys.Select(SelectBlendshape));
			
			if (frame == 0)
			{
				resetKeys = new Dictionary<string, TransformData>(clip.resetKeys.GetDictionary());
			}
			else
			{
				resetKeys = null;
			}

			int i = 0;
			foreach (var humanKey in clip.humanKeys)
			{
				bones[i] = humanKey.Key;
				humanTransforms[i] = humanKey.Value[frame];
				++i;
			}

			Array.Copy(bones, humanBones, bones.Length);
		}

		/// <param name="humanBones">Bones in the animation using a cache (see: <see cref="KinetixPose.bones")/></param>
		/// <param name="clip">Clip of the animation</param>
		/// <param name="frame">Frame to extract from the animation</param>
		public KinetixFrame(HumanBodyBones[] humanBones, KinetixClip clip, int frame) : base(null, null, null, default, default)
		{
			this.clip = clip;
			this.frame = frame;
			
			int count = humanBones.Length;
			bones = (HumanBodyBones[])humanBones.Clone();
			humanTransforms = new TransformData[count];
			blendshapes = clip.blendshapeKeys.Count == 0 ? new BlendshapeArray() : new BlendshapeArray(clip.blendshapeKeys.Select(SelectBlendshape));

			if (frame == 0)
			{
				resetKeys = new Dictionary<string, TransformData>(clip.resetKeys.GetDictionary());
			}
			else
			{
				resetKeys = null;
			}

			for (int i = 0; i < count; i++)
			{
				humanTransforms[i] = clip.humanKeys[humanBones[i]][frame];
			}
		}

		private KeyValuePair<ARKitBlendshapes, float> SelectBlendshape(KeyValuePair<ARKitBlendshapes, float[]> f) => new KeyValuePair<ARKitBlendshapes, float>(f.Key, f.Value[frame]);

		/// <summary>
		/// Retrieve the armature from the clip's zero pose
		/// </summary>
		/// <param name="interpreter">Interpreter to use to get the path to the armature</param>
		public void AdaptToInterpreter(IPoseInterpreter interpreter)
		{
			string armature = interpreter.GetArmature();
			if (armature == null) return;

			if (resetKeys != null && resetKeys.TryGetValue(armature, out TransformData transformData))
			{
				resetKeys.Remove(armature);
				this.armature = transformData;
			}
			else if (clip.resetKeys.TryGetValue(armature, out transformData))
			{
				this.armature = transformData;
			}
		}

		/// <summary>
		/// Apply the frame on an avatar using its pose interpreter
		/// </summary>
		/// <param name="interpreter">The interpreter to apply the pose on the avatar</param>
		public void Sample(IPoseInterpreter interpreter)
		{
			if (frame == 0)
			{
				ApplyResetPose(interpreter);
			}

			SampleHumanAnimation(interpreter);
			SampleOthers(interpreter);
			SampleBlendshapePos(interpreter);
		}

		/// <summary>
		/// Apply the reset pose (T-Pose of non-human transforms) on an avatar using its pose interpreter
		/// </summary>
		/// <param name="interpreter">The interpreter to apply the pose on the avatar</param>
		public void ApplyResetPose(IPoseInterpreter interpreter)
		{
			if (resetKeys == null)
				return;
			
			foreach (var item in resetKeys)
			{
				interpreter.ApplyResetPose(item.Key, item.Value);
			}
		}

		/// <summary>
		/// Apply the pose without reset pose (T-Pose of non-human transforms) on an avatar using its pose interpreter
		/// </summary>
		/// <param name="interpreter">The interpreter to apply the pose on the avatar</param>
		public void SampleHumanAnimation(IPoseInterpreter interpreter)
		{
			int count = bones.Length;
			for (int i = 0; i < count; i++)
			{
				interpreter.ApplyBone(bones[i], humanTransforms[i]);
			}
		}

		/// <summary>
		/// Apply the pose of the root and the armature on an avatar using its pose interpreter
		/// </summary>
		/// <param name="interpreter">The interpreter to apply the pose on the avatar</param>
		public void SampleOthers(IPoseInterpreter interpreter)
		{
			if (root != null)
			{
				interpreter.ApplyOther(HumanSpecialBones.Root, root.Value);
			}

			if (armature != null)
			{
				interpreter.ApplyOther(HumanSpecialBones.Armature, armature.Value);
			}
		}

		private void SampleBlendshapePos(IPoseInterpreter interpreter)
		{
			if (blendshapes == null)
				return;
			

			ARKitBlendshapes arkit;
			int count = blendshapes.Count;
			for (int i = 0; i < count; i++)
			{
				arkit = (ARKitBlendshapes)i;
				interpreter.ApplyBlendshape(arkit, blendshapes[arkit]);
			}
		}
	}
}
