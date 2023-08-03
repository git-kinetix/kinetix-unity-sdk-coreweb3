using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kinetix.Internal.Utils
{
	public class SkeletonGizmo
	{
		public static void DrawSkeleton(Transform root, float scaleFactor = 1) => DrawSkeleton(root, Vector3.zero, Color.white, scaleFactor);
		public static void DrawSkeleton(Transform root, Vector3 globalOffset, float scaleFactor = 1) => DrawSkeleton(root, globalOffset, Color.white, scaleFactor);
		public static void DrawSkeleton(Transform root, Vector3 globalOffset, Color color, float scaleFactor = 1)
		{
			Profiler.BeginSample(nameof(DrawSkeleton));
			Gizmos.color = color;
			var trs = root.GetComponentsInChildren<Transform>();
			foreach (Transform tr in trs)
			{
				if (tr == root) continue;

				Vector3 pos = tr.position;
				Vector3 parentPos = tr.parent.position;
				float size = Vector3.Distance(parentPos, pos);

				if (size <= 0)
					continue;

				KinetixLogger.DrawPyramidMesh(parentPos + globalOffset, pos + globalOffset, Mathf.Clamp(size * 0.15f * scaleFactor, 0.005f * scaleFactor, 0.03f * scaleFactor));
			}
			Gizmos.color = Color.white;
			Profiler.EndSample();
		}

		public static void DrawSkeleton(DataBoneTransform root, float scaleFactor = 1) => DrawSkeleton(root, Vector3.zero, Color.white, scaleFactor);
		public static void DrawSkeleton(DataBoneTransform root, Vector3 globalOffset, float scaleFactor = 1) => DrawSkeleton(root, globalOffset, Color.white, scaleFactor);
		public static void DrawSkeleton(DataBoneTransform root, Vector3 globalOffset, Color color, float scaleFactor = 1)
		{
			Profiler.BeginSample(nameof(DrawSkeleton));
			Gizmos.color = color;
			foreach (DataBoneTransform tr in root)
			{
				if (tr == root) continue;

				Vector3 pos, parentPos;

				if (tr is GenericBoneTransform gTr)
				{
					pos = gTr.position;
					parentPos = gTr.parent.position;
				}
				else
				{
					DataBoneTransform parent = tr.parent;

					Matrix4x4 matrix4x4 = parent.IterateParent<DataBoneTransform>().Skip(1).Aggregate(Matrix4x4.identity, DataBoneTransform_Agregate);
					parentPos = matrix4x4.MultiplyPoint(parent.localPosition);

					matrix4x4 *= Matrix4x4.TRS(parent.localPosition, parent.localRotation.normalized, parent.localScale);
					pos = matrix4x4.MultiplyPoint(tr.localPosition);
				}
				float size = Vector3.Distance(parentPos, pos);

				if (size <= 0)
					continue;

				KinetixLogger.DrawPyramidMesh(parentPos + globalOffset, pos + globalOffset, Mathf.Clamp(size * 0.15f * scaleFactor, 0.005f * scaleFactor, 0.03f * scaleFactor));
			}
			Gizmos.color = Color.white;
			Profiler.EndSample();
		}

		private static Matrix4x4 DataBoneTransform_Agregate(Matrix4x4 q, DataBoneTransform t)
			=> Matrix4x4.TRS(t.localPosition, t.localRotation.normalized, t.localScale) * q;
	}

}
