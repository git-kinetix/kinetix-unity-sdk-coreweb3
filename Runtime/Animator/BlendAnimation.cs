using Kinetix.Internal.Retargeting;
using Kinetix.Internal.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kinetix.Internal
{
	public class BlendAnimation : MonoBehaviour
	{
		#region Members
		public Action OnBlendEnded;

		private Transform                      m_RigRoot;
		private Transform                      m_CloneRigRoot;
		private Dictionary<string, Transform>  m_MatchingRigTransform;
		private Dictionary<string, Quaternion> m_InitialRotation;
		private Dictionary<string, Vector3>    m_InitialPosition;
		private Animator                       m_AnimatorClone;
		private Avatar                         m_Avatar;

		private bool                                    m_Interpolation = false;
		private float                                   m_BlendDuration;
		private float                                   m_TimeElapsed;
		private const float                             m_positionThreshold = 0.1f;
		private const float                             m_AngleThreshold = 0.05f;
		#endregion

		private const string HIPS = "Hips";

		private void Awake()
		{
			m_MatchingRigTransform = new Dictionary<string, Transform>();
			m_InitialRotation = new Dictionary<string, Quaternion>();
			m_InitialPosition = new Dictionary<string, Vector3>();
		}

		private void LateUpdate()
		{
			if (m_Interpolation)
			{
				BonesInterpolation();
			}
		}

		private void OnDestroy()
		{
			if (m_AnimatorClone != null)
				Destroy(m_AnimatorClone.gameObject);
		}

		#region Private Manipulators
		internal Transform GetHips(Transform root)
		{
			try
			{
				var table = RetargetTableCache.GetTableSync(new AvatarData(m_Avatar, root));
				return root.Find(table.m_boneMapping[HIPS]) ?? throw new NullReferenceException();
			}
			catch
			{
				HumanBone humanBone = m_Avatar.humanDescription.human.First(n => n.humanName == HIPS);
				return root.GetComponentsInChildren<Transform>().First(t => (t != null ? t.name : null) == humanBone.boneName);
			}
		}

		/// <summary>
		/// Select Armature Root using the hips
		/// </summary>
		/// <param name="rootTr"></param>
		/// <param name="hips"><see cref="GetHips(Transform)"/></param>
		/// <returns></returns>
		private Transform SelectArmatureRoot(Transform rootTr, Transform hips)
		{
			Transform tr = hips;
			Transform trParent = hips;
			while (trParent != rootTr)
			{
				tr = trParent;
				trParent = tr.parent;
			}

			return tr;
		}

		public void Dispose()
		{
			
			if (m_AnimatorClone != null)
				Destroy(m_AnimatorClone.gameObject);
			m_Interpolation = false;
		}
		
		/// <summary>
		/// Clone the main avatar, but only keep Animator and the hierarchy
		/// </summary>
		private void GenerateCloneRig()
		{
			Animator originalAnimator = gameObject.GetComponent<Animator>();
			Transform[] hierarchy = gameObject.GetComponentsInChildren<Transform>(true);
			int totalTransformCount = hierarchy.Count();
			Transform[] clones = new Transform[totalTransformCount];

			GameObject clone = new GameObject(gameObject.name + "(clone)")
			{
				//		hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector
			};
			clone.transform.SetParent(transform.parent);
			clone.transform.localPosition = Vector3.zero;
			clone.transform.localRotation = Quaternion.identity;

			clones[0] = clone.transform;

			for (int i = 1; i < totalTransformCount; i++)
			{
				Transform tr = hierarchy[i];
				Transform cloneTr = new GameObject(tr.name).transform;
				clones[i] = cloneTr;
				cloneTr.SetParent(clones[Array.IndexOf(hierarchy, tr.parent)]);
				cloneTr.localPosition = tr.localPosition;
				cloneTr.localRotation = tr.localRotation;
				cloneTr.localScale = tr.localScale;
			}

			//Add empty renderer so that the animator doesn't get culled
			clone.AddComponent<MeshRenderer>();

			m_AnimatorClone = clone.AddComponent<Animator>();
			IEnumerator Routine()
			{
				m_AnimatorClone.enabled = true;

				m_AnimatorClone.avatar = originalAnimator.avatar;
				m_AnimatorClone.applyRootMotion = originalAnimator.applyRootMotion;
				m_AnimatorClone.runtimeAnimatorController = originalAnimator.runtimeAnimatorController;
				m_AnimatorClone.speed = originalAnimator.speed;
				m_AnimatorClone.updateMode = originalAnimator.updateMode;
				m_AnimatorClone.stabilizeFeet = originalAnimator.stabilizeFeet;
				m_AnimatorClone.feetPivotActive = originalAnimator.feetPivotActive;
				m_AnimatorClone.cullingMode = originalAnimator.cullingMode;
				m_AnimatorClone.layersAffectMassCenter = originalAnimator.layersAffectMassCenter;
				m_AnimatorClone.logWarnings = originalAnimator.logWarnings;
				m_AnimatorClone.fireEvents = false;


#if UNITY_2022_2_OR_NEWER
                m_AnimatorClone.keepAnimatorStateOnDisable = true;
#else
    			m_AnimatorClone.keepAnimatorControllerStateOnDisable = true;
#endif
				

				yield return new WaitForEndOfFrame();

				m_AnimatorClone.enabled = false;
			};
			StartCoroutine(Routine());

			m_CloneRigRoot = SelectArmatureRoot(clone.transform, GetHips(clone.transform));
		}

		/// <summary>
		/// Fill dictionary with bones path of the main avatar and matching transform bones of clone avatar
		/// </summary>
		private void FillMatchDictionnary()
		{
			Transform[] mainTransforms = m_RigRoot.GetComponentsInChildren<Transform>();
			Transform[] cloneTransforms = m_CloneRigRoot.GetComponentsInChildren<Transform>();

			for (int i = 0; i < mainTransforms.Length; i++)
			{
				if (!m_MatchingRigTransform.TryToAdd(GetPath(mainTransforms[i]), cloneTransforms[i]))
				{
					KinetixDebug.LogError($"Bones with same path {GetPath(mainTransforms[i])}");
				}
			}
		}

		/// <summary>
		/// Store initial position before blend 
		/// </summary>
		private void SaveInitialPosition()
		{
			Transform[] mainTransforms = m_RigRoot.GetComponentsInChildren<Transform>();

			for (int i = 0; i < mainTransforms.Length; i++)
			{
				if (m_InitialRotation.ContainsKey(GetPath(mainTransforms[i])))
				{
					m_InitialRotation[GetPath(mainTransforms[i])] = mainTransforms[i].localRotation;
					m_InitialPosition[GetPath(mainTransforms[i])] = mainTransforms[i].localPosition;
				}
				else
				{
					m_InitialRotation.Add(GetPath(mainTransforms[i]), mainTransforms[i].localRotation);
					m_InitialPosition.Add(GetPath(mainTransforms[i]), mainTransforms[i].localPosition);
				}
			}
		}

		/// <summary>
		/// Interpolation of rotation and position bones
		/// </summary>
		private void BonesInterpolation()
		{
			Transform[] mainTransforms = m_RigRoot.GetComponentsInChildren<Transform>();
			bool lerpEnded = true;
			foreach (Transform bone in mainTransforms)
			{
				if (m_MatchingRigTransform.TryGetValue(GetPath(bone), out Transform cloneBone))
				{
					if (m_InitialRotation.TryGetValue(GetPath(bone), out Quaternion initalRotation)
						&& m_InitialPosition.TryGetValue(GetPath(bone), out Vector3 initalPosition))
					{
						bone.localPosition = Vector3.Lerp(initalPosition, cloneBone.localPosition, m_TimeElapsed / m_BlendDuration);
						bone.localRotation = Quaternion.Lerp(initalRotation, cloneBone.localRotation, m_TimeElapsed / m_BlendDuration);
					}

					if (
						!UnityEqualityComparer.Approximately(bone.localRotation, cloneBone.localRotation, m_AngleThreshold / 180)
						||
						!UnityEqualityComparer.Approximately(bone.localPosition, cloneBone.localPosition, m_positionThreshold)
					)
							lerpEnded = false;
				}
			}

			m_TimeElapsed += Time.deltaTime;

			if (lerpEnded)
			{
				OnBlendEnded?.Invoke();
				m_Interpolation = false;
			}
		}

		private void StartParameters(float blendDuration)
		{
			SaveInitialPosition();
			m_TimeElapsed = 0f;
			m_Interpolation = true;
			m_BlendDuration = blendDuration;
		}
		#endregion


		#region Public Manipulators
		public void Init()
		{
			m_Avatar = transform.GetComponent<Animator>().avatar;
			m_RigRoot = SelectArmatureRoot(transform, GetHips(transform));
			GenerateCloneRig();
			FillMatchDictionnary();
		}

		/// <summary>
		/// Animate avatar clone
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="blendDuration"></param>
		public void BlendAnim(AnimationClip clip, float blendDuration, float startTimeAnim = 0)
		{
			m_AnimatorClone.enabled = false;
			clip.SampleAnimation(m_CloneRigRoot.parent.gameObject, startTimeAnim);
			StartParameters(blendDuration);
		}

		/// <summary>
		/// Apply Animator controller states to avatar clone animator
		/// </summary>
		/// <param name="statesInfos"></param>
		/// <param name="blendDuration"></param>
		public void BlendAnim(RuntimeAnimatorController ctrl, AnimatorStateInfo[] statesInfos, float blendDuration)
		{
			if (m_AnimatorClone == null)
				return;
			
			//The "IF" is to avoid erasing the controller
			if (m_AnimatorClone.runtimeAnimatorController != ctrl)
				m_AnimatorClone.runtimeAnimatorController = ctrl;

			int lc = m_AnimatorClone.layerCount;
			//Put the animator Clone back to its state
			try
			{
				for (int i = 0; i < lc; i++)
				{
					m_AnimatorClone.Play(statesInfos[i].fullPathHash, i, statesInfos[i].normalizedTime);
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			m_AnimatorClone.Update(0.001f);

			StartParameters(blendDuration);
		}

		#endregion


		#region Utilities

		/// <summary>
		/// Compute a transform's full path as a string.
		/// </summary>
		/// <param name="t"></param>
		/// <returns>The full path in the form "/root1/root2/transfrom"</returns>
		public string GetPath(Transform t)
		{
			return GetPathBuilder(t).ToString();
		}

		/// <summary>
		/// Compute transform's full path as a StringBuilder
		/// </summary>
		/// <see cref="GetPath"/>
		/// <param name="t"></param>
		/// <returns>The StringBuilder containing the full path</returns>
		public StringBuilder GetPathBuilder(Transform t)
		{
			if (t == null)
				return new StringBuilder();

			StringBuilder sb = GetPathBuilder(t.parent);
			sb.AppendFormat("/{0}", t.name);
			return sb;
		}

		#endregion


#if UNITY_EDITOR && DEV_KINETIX && !DEBUG_AVATAR
		public Color m_colorRig = Color.blue;
		public Color m_colorRigClone = Color.red;
		private void OnDrawGizmos()
		{
			if (m_RigRoot)      KinetixLogger.DrawSkeleton(m_RigRoot, Vector3.zero, m_colorRig);
			if (m_CloneRigRoot) KinetixLogger.DrawSkeleton(m_CloneRigRoot, m_RigRoot.position - m_CloneRigRoot.position, m_colorRigClone);

		}
#endif
	}
}
