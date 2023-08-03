// // ----------------------------------------------------------------------------
// // <copyright file="TransformPoseInterpreter.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using Kinetix.Internal.Retargeting;
using Kinetix.Internal.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kinetix.Internal
{
	/// <summary>
	/// A pose interpreter for Avatars using unity's transform system
	/// </summary>
	public class TransformPoseInterpreter : IPoseInterpreter, IPoseInterpreterStartEnd
	{
		private readonly GameObject root;
		private readonly GameObject armature;
		private readonly Dictionary<HumanBodyBones, GameObject> m_map = new Dictionary<HumanBodyBones, GameObject>();
		private readonly SkinnedMeshRenderer[] skinnedMeshRenderer;
		protected KinetixClip clip;

		public TransformPoseInterpreter(GameObject root, Avatar avatar, SkinnedMeshRenderer[] skinnedMeshRenderer = null)
		{
			this.root = root;

			if (skinnedMeshRenderer != null)
				this.skinnedMeshRenderer = skinnedMeshRenderer.Length == 0 ? null : skinnedMeshRenderer;

			AvatarRetargetTable table = RetargetTableCache.GetTableSync(new AvatarData(avatar, root.transform));
			int count = UnityHumanUtils.HUMANS.Count;
			for (int i = 0; i < count; i++)
			{
				try
				{
					string name = UnityHumanUtils.HUMANS[i];
					m_map[ (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), name.Replace(" ","") ) ] = root.transform.Find( table.m_boneMapping[name] )?.gameObject;
				}
				catch (Exception) {}
			}

			armature = m_map[HumanBodyBones.Hips].transform.parent.gameObject;
			armature = armature == root ? null : armature;
		}

		///<inheritdoc/>
		public virtual void AnimationStart(KinetixClip clip)
		{
			this.clip = clip;
		}

		public virtual void AnimationEnd(KinetixClip clip)
		{
		}

		public virtual void QueueStart()
		{

		}

		public virtual void QueueEnd()
		{
			clip = null;
		}


		///<inheritdoc/>
		public virtual void ApplyResetPose(string bonePath, TransformData pose)
		{
			Transform tr = root.transform.Find(bonePath);
			if (tr)
			{
				ApplyDataToTransform(pose, tr);
			}
		}

		///<inheritdoc/>
		public virtual void ApplyBone(HumanBodyBones bone, TransformData pose)
		{
			if (m_map.TryGetValue(bone, out GameObject go) && go)
			{
				Transform tr = go.transform;
				ApplyDataToTransform(pose, tr);
			}
		}

		public virtual void ApplyOther(HumanSpecialBones bone, TransformData pose)
		{
			switch (bone)
			{
				case HumanSpecialBones.Root:
					ApplyDataToTransform(pose, root.transform);
					break;
				case HumanSpecialBones.Armature:
					Transform armature = m_map[HumanBodyBones.Hips].transform.parent;
					ApplyDataToTransform(pose, armature);
					break;
				default:
					break;
			}
		}

		public virtual void ApplyBlendshape(ARKitBlendshapes blendshape, float pose)
		{
			if (skinnedMeshRenderer != null)
			{
				try
				{
					SkinnedMeshRenderer mesh;
					for (int i = 0; i < skinnedMeshRenderer.Length; i++)
					{
						mesh = skinnedMeshRenderer[i];
						int index = mesh.sharedMesh.GetBlendShapeIndex(blendshape.ToString());
						if (index == -1)
							continue;
						mesh.SetBlendShapeWeight(index, pose);
					}
				}
				catch (Exception)
				{
				}
			}
		}

		public virtual string GetArmature()
		{
			if (armature == null)
				return null;

			string path = armature.name;
			Transform parent = armature.transform.parent;
			while (parent != root.transform)
			{
				path = parent.name + "/" + path;
			}

			return path;
		}

		///<inheritdoc/>
		public virtual KinetixPose GetPose()
		{
			HumanBodyBones[] bones = m_map.Keys.ToArray();
			int length = bones.Length;

			TransformData[] trs = new TransformData[length];

			for (int i = 0; i < length; i++)
			{
				HumanBodyBones b = bones[i];
				if (m_map.TryGetValue(b, out GameObject go) && go)
				{
					Transform transform = go.transform;
					trs[i] = TransformToData(transform);
				}
			}

			TransformData rootTrData = TransformToData(root.transform);

			TransformData? armatureTrData = armature == null ? default(TransformData?) : TransformToData(armature.transform);

            SkinnedMeshRenderer skinnedMeshRenderer = this.skinnedMeshRenderer?.FirstOrDefault();
            return new KinetixPose(trs, bones, skinnedMeshRenderer == null ? null : new float[(int)ARKitBlendshapes.Count].Select(SelectBlendshapes), rootTrData, armatureTrData);

			float SelectBlendshapes(float arg1, int arg2)
				=> skinnedMeshRenderer.GetBlendShapeWeight(arg2);
		}

        private static void ApplyDataToTransform(TransformData pose, Transform tr)
		{
			if (pose.position.HasValue) tr.localPosition = pose.position.Value;
			if (pose.rotation.HasValue) tr.localRotation = pose.rotation.Value;
			if (pose.scale.HasValue) tr.localScale = pose.scale.Value;
		}

		private static TransformData TransformToData(Transform transform) => new TransformData()
		{
			position = transform.localPosition,
			rotation = transform.localRotation,
			scale = transform.localScale
		};

	}
}
