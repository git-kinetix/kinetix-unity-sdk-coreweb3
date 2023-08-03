using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kinetix.Internal
{
	public struct RangeFloat
	{
		public float min;
		public float max;

        public RangeFloat(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float Center => (min + max)/2;
		public float Size => Mathf.Abs(max-min);
	}

	/// <summary>
	/// A tool to debug draw line etc... in the scene
	/// </summary>
	public class ThreadedDrawer : IDisposable
	{
		private static ThreadedDrawer _staticInstance;
		public static ThreadedDrawer Static => _staticInstance ??= new ThreadedDrawer(Vector3.zero, Color.white, "StaticThreadedDrawer");

		private ThreadedDrawerBehaviour m_behaviour;
		public Vector3 m_startOffset;
		private Color m_color;

		public ThreadedDrawer(Vector3 startOffset, Color color, string name = "ThreadedDrawer")
		{
			m_color = color;
			m_startOffset = startOffset;
#if DEV_KINETIX
			m_behaviour = new GameObject(name).AddComponent<ThreadedDrawerBehaviour>();
#endif
		}

		public void DrawFromTo(Vector3 from, Vector3 to, Color? color = null, float? duration = null)
		{
			if (m_behaviour != null)
			{
				lock (m_behaviour.data)
				{
					m_behaviour.data.Add(new DataLine(from + m_startOffset, to + m_startOffset, color ?? m_color, duration ?? 0));
				}
			}
		}

		public void DrawPyramidMesh(Vector3 from, Vector3 to, float size, Color? color = null, float? duration = null)
		{
			if (m_behaviour != null)
			{
				lock (m_behaviour.data)
				{
					m_behaviour.data.Add(new DataTriangle(from + m_startOffset, to + m_startOffset, color ?? m_color, duration ?? 0, size));
				}
			}
		}

		public void DrawCube(RangeFloat rangeX, RangeFloat rangeY, RangeFloat rangeZ, Quaternion rotation, Color? color = null, float? duration = null)
		{
			if (m_behaviour != null)
			{
				lock (m_behaviour.data)
				{
					DataCube item = new DataCube(rangeX, rangeY, rangeZ, rotation, color ?? m_color, duration ?? 0);
					item.center += m_startOffset;
					m_behaviour.data.Add(item);
				}
			}
		}

		public void DrawPlane(Vector3 position, Quaternion rotation, float size, Color? color = null, float? duration = null)
		{
			if (m_behaviour != null)
			{
				lock (m_behaviour.data)
				{
					m_behaviour.data.Add(new DataPlane(position + m_startOffset, rotation, size, color ?? m_color, duration ?? 0));
				}
			}
		}

		public void DrawCube(Vector3 center, Vector3 size, Quaternion rotation, Color? color = null, float? duration = null)
		{
			if (m_behaviour != null)
			{
				lock (m_behaviour.data)
				{
					m_behaviour.data.Add(new DataCube(center + m_startOffset, size, rotation, color ?? m_color, duration ?? 0));
				}
			}
		}

		public void Dispose()
		{
			if (m_behaviour == null) return;

			//just in case
			m_behaviour.gameObject.SetActive(true);
			m_behaviour.enabled = true;

			m_behaviour.Dispose();
			m_behaviour = null;
		}

		public abstract class AData
		{
			public Color color;
			public float duration;

			protected AData(Color color, float duration)
			{
				this.color = color;
				this.duration = duration;
			}
			public abstract void Draw();
		}
		public class DataLine : AData
		{
			public Vector3 from;
			public Vector3 to;

            public DataLine(Vector3 from, Vector3 to, Color color, float duration) : base(color, duration)
            {
                this.from = from;
                this.to = to;
                this.duration = duration;
            }

            public override void Draw()
            {
				Debug.DrawLine(from, to, color);
			}
        }
		public class DataTriangle : AData
		{
			public Vector3 from;
			public Vector3 to;
			public float size;

			public DataTriangle(Vector3 from, Vector3 to, Color color, float duration, float size = 1) : base(color, duration)
			{
				this.from = from;
				this.to = to;
				this.duration = duration;
				this.size = size;
			}

            public override void Draw()
			{
				KinetixLogger.DrawPyramidMesh(from, to, size);
			}
        }
		public class DataPlane : AData
		{
			public Vector3 position;
			public Quaternion rotation;
			public float size;

			public DataPlane(Vector3 position, Quaternion rotation, float size, Color color, float duration) : base(color, duration)
			{
				this.position = position;
				this.rotation = rotation;
				this.size = size;
			}

            public override void Draw()
            {
				var matrix = Gizmos.matrix;

				Gizmos.matrix *= Matrix4x4.TRS(position, rotation, Vector3.one);
				Gizmos.DrawCube(Vector3.zero, new Vector3(size, 0.001f, size));

				Gizmos.matrix = matrix;
			}
        }
        public class DataCube : AData
        {
            public Vector3 center;
            public Vector3 size;
			public Quaternion rotation;

            public DataCube(RangeFloat rangeX, RangeFloat rangeY, RangeFloat rangeZ, Quaternion rotation, Color color, float duration) : base(color, duration)
			{
                center = new Vector3(rangeX.Center, rangeY.Center, rangeZ.Center);
                size = new Vector3(rangeX.Size, rangeY.Size, rangeZ.Size);

				this.rotation = rotation;
			}

            public DataCube(Vector3 center, Vector3 size, Quaternion rotation, Color color, float duration) : base(color, duration)
            {
                this.center = center;
                this.size = size;
                this.rotation = rotation;
            }

            public override void Draw()
            {
				var matrix = Gizmos.matrix;

				Gizmos.matrix *= Matrix4x4.TRS(center, rotation, Vector3.one);
				Gizmos.DrawCube(Vector3.zero, size);
				Gizmos.matrix = matrix;
			}
        }

        /// <summary>
        /// Behaviour to call unity's methods in unity's threads for drawing
        /// </summary>
        public class ThreadedDrawerBehaviour : MonoBehaviour, IDisposable
		{
			public bool disposed = false;
			public List<AData> data = new List<AData>();

			public void Dispose()
			{
				disposed = true;
			}

			// Update is called once per frame
			void LateUpdate()
			{
				if (data.Count == 0 && disposed)
				{
					enabled = false;
					Destroy(gameObject);
					return;
				}

				lock (data)
				{
					for (int i = data.Count - 1; i >= 0; i--)
					{
						AData item = data[i];
						item.duration -= Time.deltaTime;

						if (item.duration <= 0)
							data.RemoveAt(i);
					}
				}
			}

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
						AData item = data[i];
#if UNITY_EDITOR
						if (!UnityEditor.EditorApplication.isPaused)
#endif
							item.duration -= Time.deltaTime;

						Gizmos.color = item.color;
						item.Draw();
						Gizmos.color = Color.white;

						if (item.duration <= 0)
							data.RemoveAt(i);
					}
				}
            }

        }
	}
}
