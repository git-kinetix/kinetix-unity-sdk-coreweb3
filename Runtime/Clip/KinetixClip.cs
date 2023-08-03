// // ----------------------------------------------------------------------------
// // <copyright file="KinetixClip.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace Kinetix
{
	public enum ARKitBlendshapes
	{
		browInnerUp,
		browDownLeft,
		browDownRight,
		browOuterUpLeft,
		browOuterUpRight,
		eyeLookUpLeft,
		eyeLookUpRight,
		eyeLookDownLeft,
		eyeLookDownRight,
		eyeLookInLeft,
		eyeLookInRight,
		eyeLookOutLeft,
		eyeLookOutRight,
		eyeBlinkLeft,
		eyeBlinkRight,
		eyeSquintRight,
		eyeSquintLeft,
		eyeWideLeft,
		eyeWideRight,
		cheekPuff,
		cheekSquintLeft,
		cheekSquintRight,
		noseSneerLeft,
		noseSneerRight,
		jawOpen,
		jawForward,
		jawLeft,
		jawRight,
		mouthFunnel,
		mouthPucker,
		mouthLeft,
		mouthRight,
		mouthRollUpper,
		mouthRollLower,
		mouthShrugUpper,
		mouthShrugLower,
		mouthOpen,
		mouthClose,
		mouthSmileLeft,
		mouthSmileRight,
		mouthFrownLeft,
		mouthFrownRight,
		mouthDimpleLeft,
		mouthDimpleRight,
		mouthUpperUpLeft,
		mouthUpperUpRight,
		mouthLowerDownLeft,
		mouthLowerDownRight,
		mouthPressLeft,
		mouthPressRight,
		mouthStretchLeft,
		mouthStretchRight,
        Count
    }

	/// <summary>
	/// A structure that represent a keyFrame of a transform
	/// </summary>
	public struct TransformData
	{
		public Vector3? position;
		public Vector3? scale;
		public Quaternion? rotation;

		public static TransformData Default { get; } = new TransformData()
		{
			position = Vector3.zero,
			scale = Vector3.one,
			rotation = Quaternion.identity,
		};
	}

	/// <summary>
	/// A frame per frame animation with readable keys.
	/// </summary>
	/// <remarks>
	/// Two kind of animation keys exist:<br/>
	/// - The <see cref="humanKeys"/> which is the animation for human bones bases on Unity's avatars<br/>
	/// - The <see cref="resetKeys"/> which are keys to reset non human bones's transform
	/// </remarks>
	public class KinetixClip
	{
		/// <summary>
		/// Duration of the animation.
		/// </summary>
		/// <remarks>
		/// <see cref="KeyCount"/> / <see cref="FrameRate"/>
		/// </remarks>
		public float Duration => KeyCount / FrameRate;
		
		/// <summary>
		/// Number of keys in the animation
		/// </summary>
		public int KeyCount { get; private set; }

		/// <summary>
		/// Number of frame by seconds.<br/>
		/// Modifying this value will change the duration of the anime
		/// </summary>
		public float FrameRate { get; set; }

		/// <summary>
		/// Name of the animation
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Animation for human bones bases on Unity's avatars.<br/>
		/// The key is the Unity avatar's bone.
		/// </summary>
		public KinetixClipDictionary<HumanBodyBones, TransformData[]> humanKeys;

		/// <summary>
		/// Keys to reset the non human bones to T-Pose
		/// </summary>
		public KinetixClipDictionary<string        , TransformData  > resetKeys;

		/// <summary>
		/// Keys to set the pose of the face
		/// </summary>
		public KinetixClipDictionary<ARKitBlendshapes, float[] > blendshapeKeys;

		/// <summary>
		/// Create a new KinetixClip
		/// </summary>
		public KinetixClip()
		{
			humanKeys = new KinetixClipDictionary<HumanBodyBones, TransformData[]>();
			resetKeys = new KinetixClipDictionary<string, TransformData>();
			blendshapeKeys = new KinetixClipDictionary<ARKitBlendshapes, float[]>();
			Reset();
		}

		/// <summary>
		/// Reset the keys and the framerate
		/// </summary>
		public void Reset()
		{
			humanKeys.Clear();
			resetKeys.Clear();
			blendshapeKeys.Clear();
			KeyCount = 0;
			FrameRate = 24;
		}

		/// <summary>
		/// Set the <see cref="humanKeys"/> of a human bone based on his <paramref name="bone"/>
		/// </summary>
		/// <param name="bone">Human Bone to recieve the animation</param>
		/// <param name="frames">Frames of the animation</param>
		public void SetKeys(HumanBodyBones bone, TransformData[] frames)
		{
			humanKeys[bone] = frames;
			KeyCount = Mathf.Max(KeyCount, frames.Length);
		}

		/// <summary>
		/// Set the <see cref="resetKeys"/> of a non human bone based on his <paramref name="path"/>
		/// </summary>
		/// <param name="path">Path of the non human bone</param>
		/// <param name="resetTransform">Reset transform of the non human bone</param>
		public void SetKeys(string path, TransformData resetTransform)
		{
			resetKeys[path] = resetTransform;
		}

		/// <summary>
		/// Set the <see cref="blendshapeKeys"/> of a blendshape based on his <paramref name="blendshape"/>
		/// </summary>
		/// <param name="blendshape">ARKit Blendshape to recieve the animation</param>
		/// <param name="frames">Frames of the animation</param>
		public void SetKeys(ARKitBlendshapes blendshape, float[] frames)
		{
			blendshapeKeys[blendshape] = frames;
			KeyCount = Mathf.Max(KeyCount, frames.Length);
		}


		/// <summary>
		/// Apply the reset poses
		/// </summary>
		/// <param name="interpreter">Class that can apply poses to the avatar</param>
		public void ApplyResetPos(IPoseInterpreter interpreter)
		{
			string armature = interpreter.GetArmature();
			foreach (var item in resetKeys)
			{
				if (armature != null && item.Key == armature)
					interpreter.ApplyOther(HumanSpecialBones.Armature, item.Value);
				else
					interpreter.ApplyResetPose(item.Key, item.Value);
			}
		}

		/// <summary>
		/// Sample the animation at a certain time for human bones.
		/// </summary>
		/// <param name="interpreter">Class that can apply poses to the avatar</param>
		/// <param name="time"></param>
		public void SampleHumanAnimation(IPoseInterpreter interpreter, float time)
			=> SampleHumanAnimation(interpreter, Mathf.FloorToInt(time * FrameRate));


		/// <summary>
		/// Sample the animation at a certain frame for human bones.
		/// </summary>
		/// <param name="interpreter">Class that can apply poses to the avatar</param>
		/// <param name="frame"></param>
		public void SampleHumanAnimation(IPoseInterpreter interpreter, int frame)
		{
			frame = Mathf.Clamp(frame, 0, KeyCount - 1);
			ApplyHumanPos(interpreter, frame);
		}

		/// <summary>
		/// Sample the animation at a certain time for blendshapes.
		/// </summary>
		/// <param name="interpreter">Class that can apply poses to the avatar</param>
		/// <param name="time"></param>
		public void SampleFacialAnimation(IPoseInterpreter interpreter, float time)
			=> SampleFacialAnimation(interpreter, Mathf.FloorToInt(time * FrameRate));


		/// <summary>
		/// Sample the animation at a certain frame for blendshapes.
		/// </summary>
		/// <param name="interpreter">Class that can apply poses to the avatar</param>
		/// <param name="frame"></param>
		public void SampleFacialAnimation(IPoseInterpreter interpreter, int frame)
		{
			frame = Mathf.Clamp(frame, 0, KeyCount - 1);
			ApplyBlendshapePos(interpreter, frame);
		}

		/// <summary>
		/// Sample the animation at a certain time.
		/// </summary>
		/// <param name="interpreter">Class that can apply poses to the avatar</param>
		/// <param name="time"></param>
		public void SampleAnimation(IPoseInterpreter interpreter, float time)
			=> SampleAnimation(interpreter, Mathf.FloorToInt(time * FrameRate));

		/// <summary>
		/// Sample the animation at a certain frame.
		/// </summary>
		/// <param name="interpreter">Class that can apply poses to the avatar</param>
		/// <param name="frame"></param>
		public void SampleAnimation(IPoseInterpreter interpreter, int frame)
		{
			frame = Mathf.Clamp(frame, 0, KeyCount - 1);

			if (frame <= 10)
			{
				ApplyResetPos(interpreter);
			}
			ApplyHumanPos(interpreter, frame);
			ApplyBlendshapePos(interpreter, frame);
		}

		private void ApplyHumanPos(IPoseInterpreter interpreter, int frame)
		{
			foreach (KeyValuePair<HumanBodyBones, TransformData[]> item in humanKeys)
			{
				interpreter.ApplyBone(item.Key, item.Value[frame]);
			}
		}

		private void ApplyBlendshapePos(IPoseInterpreter interpreter, int frame)
		{
			foreach (KeyValuePair<ARKitBlendshapes, float[]> item in blendshapeKeys)
			{
				interpreter.ApplyBlendshape(item.Key, item.Value[frame]);
			}
		}

		public static implicit operator bool(KinetixClip kinetixClip) => kinetixClip != null;
	}
}
