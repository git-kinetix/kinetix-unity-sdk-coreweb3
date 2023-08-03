using System.Collections.Generic;
using UnityEngine;

namespace Kinetix.Internal.Utils
{
	/// <summary>
	/// Internal tool to draw skeleton like Debug.Draw
	/// </summary>
	public static class SkeletonDebugDraw
	{
		private static SkeletonDebugDrawBehaviour _staticInstance;

		[RuntimeInitializeOnLoadMethod]
		private static void Init()
		{
#if DEV_KINETIX && UNITY_EDITOR
			_staticInstance = new GameObject("Kinetix_SkeletonDebugDraw").AddComponent<SkeletonDebugDrawBehaviour>();
			Object.DontDestroyOnLoad(_staticInstance.gameObject);
#endif
		}

		public static void DrawSkeleton(Transform root, float duration=0, float scaleFactor = 1) => DrawSkeleton(root, Vector3.zero, Color.white, duration, scaleFactor);
		public static void DrawSkeleton(Transform root, Vector3 globalOffset, float duration=0, float scaleFactor = 1) => DrawSkeleton(root, globalOffset, Color.white, duration, scaleFactor);
		public static void DrawSkeleton(Transform root, Vector3 globalOffset, Color color, float duration=0, float scaleFactor = 1)
		{
			if (_staticInstance != null)
			{
				_staticInstance.data.Add(new Data(root, globalOffset, color, scaleFactor, duration));
				
			}
		}
		public static void DrawSkeleton(DataBoneTransform root, float duration=0, float scaleFactor = 1) => DrawSkeleton(root, Vector3.zero, Color.white, duration, scaleFactor);
		public static void DrawSkeleton(DataBoneTransform root, Vector3 globalOffset, float duration=0, float scaleFactor = 1) => DrawSkeleton(root, globalOffset, Color.white, duration, scaleFactor);
		public static void DrawSkeleton(DataBoneTransform root, Vector3 globalOffset, Color color, float duration=0, float scaleFactor = 1)
		{
			if (_staticInstance != null)
			{
				_staticInstance.data.Add(new Data(root, globalOffset, color, scaleFactor, duration));
			}
		}

		private class Data
		{
			public readonly Transform tr;
			public readonly DataBoneTransform dataTr;
			public readonly Vector3 globalOffset;
			public readonly Color color;
			public float duration;
			public readonly float size;

			public Data(DataBoneTransform dataTr, float size = 1, float duration = 0) : this(dataTr, Vector3.zero, Color.white, size, duration) {}
			public Data(DataBoneTransform dataTr, Vector3 globalOffset, float size = 1, float duration = 0) : this(dataTr, globalOffset, Color.white, size, duration) {}
			public Data(DataBoneTransform dataTr, Vector3 globalOffset, Color color, float size = 1, float duration = 0)
			{
				this.dataTr = dataTr;
				this.globalOffset = globalOffset;
				this.color = color;
				this.duration = duration;
				this.size = size;
			}

			public Data(Transform tr, float size = 1, float duration = 0) : this(tr, Vector3.zero, Color.white, size, duration) {}
			public Data(Transform tr, Vector3 globalOffset, float size = 1, float duration = 0) : this(tr, globalOffset, Color.white, size, duration) {}
			public Data(Transform tr, Vector3 globalOffset, Color color, float size = 1, float duration = 0)
			{
				this.tr = tr;
				this.globalOffset = globalOffset;
				this.color = color;
				this.duration = duration;
				this.size = size;
			}
		}

		private class SkeletonDebugDrawBehaviour : MonoBehaviour
		{
			public List<Data> data = new List<Data>();

#if UNITY_EDITOR
			private void OnDrawGizmos()
			{
				if (data == null)
				{
					enabled = false;
					Destroy(gameObject);
				}

				lock (data)
				{
					for (int i = data.Count - 1; i >= 0; i--)
					{
						Data item = data[i];

						if (item.dataTr != null)
							SkeletonGizmo.DrawSkeleton(item.dataTr, item.globalOffset, item.color, item.size);
						else if (item.tr != null)
							SkeletonGizmo.DrawSkeleton(item.dataTr, item.globalOffset, item.color, item.size);
						if (!UnityEditor.EditorApplication.isPaused)
							item.duration -= Time.deltaTime;
						

						if (item.duration <= 0)
							data.RemoveAt(i);
					}
				}
			}
#endif
		}
	}
}
