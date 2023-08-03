// // ----------------------------------------------------------------------------
// // <copyright file="ABlending.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using Kinetix.Internal.Utils;
using System;
using UnityEngine;

namespace Kinetix.Internal
{
	/// <summary>
	/// Abstract blending class for <see cref="IFrameEffect"/>
	/// </summary>
	public abstract class ABlending : ISamplerAuthority
	{
		/// <inheritdoc/>
		public SamplerAuthorityBridge Authority { get; set; }

		/// <summary>
		/// Blend a pose (<paramref name="a"/>) and a list of pose (<paramref name="toBlend"/>)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <param name="a">Position blended</param>
		/// <param name="toBlend">List of animations to blend with <paramref name="a"/></param>
		/// <param name="time">between 0 and 1</param>
		/// <param name="blendIndexIgnore">index in the <paramref name="toBlend"/> list to ignore (corrispond to <paramref name="a"/>'s position in the list)</param>
		public void Blend<T, T2>(ref T a, in T2[] toBlend, float time, int blendIndexIgnore = -1)
			where T : KinetixPose
			where T2 : KinetixPose
		{
			int blendCount = toBlend.Length;
			int aLenght = a.bones.Length;

			//Blend human
			//A refer to the original pose
			//B refer to the pose to blend
			for (int i = 0; i < aLenght; i++)
			{
				HumanBodyBones bone = a.bones[i];
				TransformData trA = a.humanTransforms[i];

				Vector3 posA = trA.position.GetValueOrDefault(Vector3.zero);
				Quaternion rotA = trA.rotation.GetValueOrDefault(Quaternion.identity);
				Vector3 scaleA = trA.scale.GetValueOrDefault(Vector3.one);

				Matrix4x4 localToGlobalA = Matrix4x4.identity;

				//This is a fix for the hips only, we're gonna blend the world position instead of the local one
				if (bone == HumanBodyBones.Hips && trA.position.HasValue)
				{
                    TransformData armature = a.armature.GetValueOrDefault(TransformData.Default);
                    TransformData root     = a.root.GetValueOrDefault(TransformData.Default);

					localToGlobalA =
						Matrix4x4.TRS(
							armature.position.GetValueOrDefault(Vector3.zero),
							armature.rotation.GetValueOrDefault(Quaternion.identity),
							Vector3.one
						);

					posA = localToGlobalA.MultiplyPoint(posA);
				}

				Vector3 pos = Vector3.zero;
				Quaternion rot = Quaternion.identity;
				Vector3 scale = Vector3.zero;

				int pCount = 0,
					rCount = 0,
					sCount = 0;

				//Get the average of each B
				for (int j = 0; j < blendCount; j++)
				{
					if (j == blendIndexIgnore)
						continue;

					KinetixPose b = toBlend[j];
					if (b == null)
						continue;

					int bIndex = Array.IndexOf(b.bones, bone);
					if (bIndex == -1)
						continue;

					TransformData trB = b.humanTransforms[bIndex];

					//Tried to lerp on global pose
					if (bone == HumanBodyBones.Hips && trB.position.HasValue)
					{
						TransformData armature = b.armature.GetValueOrDefault(TransformData.Default);
						TransformData root = b.root.GetValueOrDefault(TransformData.Default);

						Matrix4x4 localToGlobalB =
							Matrix4x4.TRS(
								armature.position.GetValueOrDefault(Vector3.zero),
								armature.rotation.GetValueOrDefault(Quaternion.identity),
								Vector3.one
							);

						trB.position = localToGlobalB.MultiplyPoint(trB.position.Value);
					}

					BlendAdd(ref pos, ref rot, ref scale, ref pCount, ref rCount, ref sCount, trB);
				}

				ApplyBlendLerp(ref trA, time, posA, rotA, scaleA, pos, rot, scale, pCount, rCount, sCount);

				if (bone == HumanBodyBones.Hips && trA.position.HasValue)
				{
					trA.position = localToGlobalA.inverse.MultiplyPoint(trA.position.Value);
				}

				a.humanTransforms[i] = trA;
			}

			//Blend armature
			if (a.armature.HasValue)
			{
				TransformData trA = a.armature.Value;

				Vector3 posA = trA.position.GetValueOrDefault(Vector3.zero);
				Quaternion rotA = trA.rotation.GetValueOrDefault(Quaternion.identity);
				Vector3 scaleA = trA.scale.GetValueOrDefault(Vector3.one);

				Vector3 pos = Vector3.zero;
				Quaternion rot = Quaternion.identity;
				Vector3 scale = Vector3.zero;

				int pCount = 0,
					rCount = 0,
					sCount = 0;

				for (int j = 0; j < blendCount; j++)
				{
					if (j == blendIndexIgnore)
						continue;

					KinetixPose b = toBlend[j];
					if (b == null)
						continue;

					if (b.armature == null)
						continue;

					TransformData trB = b.armature.Value;

					BlendAdd(ref pos, ref rot, ref scale, ref pCount, ref rCount, ref sCount, trB);
				}

				ApplyBlendLerp(ref trA, time, posA, rotA, scaleA, pos, rot, scale, pCount, rCount, sCount);

				a.armature = trA;
			}
		}

		private static void ApplyBlendLerp(ref TransformData trA, float time, Vector3 posA, Quaternion rotA, Vector3 scaleA, Vector3 pos, Quaternion rot, Vector3 scale, int pCount, int rCount, int sCount)
		{
			if (trA.position != null) trA.position = pCount == 0 ? trA.position : Vector3.Lerp(posA, pos / pCount, time);
			if (trA.rotation != null) trA.rotation = rCount == 0 ? trA.rotation : Quaternion.Slerp(rotA, rot, time);
			if (trA.scale != null) trA.scale = sCount == 0 ? trA.scale : Vector3.Lerp(scaleA, scale / sCount, time);
		}

		/// <summary>
		/// Add <paramref name="trB"/> to sum calculation
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="rot"></param>
		/// <param name="scale"></param>
		/// <param name="pCount"></param>
		/// <param name="rCount"></param>
		/// <param name="sCount"></param>
		/// <param name="trB"></param>
		private static void BlendAdd(ref Vector3 pos, ref Quaternion rot, ref Vector3 scale, ref int pCount, ref int rCount, ref int sCount, TransformData trB)
		{
			pos += trB.position.GetValueOrDefault(Vector3.zero);
			Quaternion rotB = trB.rotation.GetValueOrDefault(Quaternion.identity);
			if (trB.rotation.HasValue)
			{
				if (rCount == 0)
					rot = rotB;
				else
					rot = Quaternion.Slerp(rot, rotB, 1.0f / (rCount + 1f)); //this code average the quaternions
			}

			scale += trB.scale.GetValueOrDefault(Vector3.zero);

			pCount += trB.position.HasValue ? 1 : 0;
			rCount += trB.rotation.HasValue ? 1 : 0;
			sCount += trB.scale.HasValue ? 1 : 0;
		}
	}
}
