using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kinetix.Internal
{
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
					m_behaviour.data.Add(new Data(color ?? m_color, from + m_startOffset, to + m_startOffset, duration ?? 0, Data.DrawType.Line));
				}
			}
		}

		public void DrawPyramidMesh(Vector3 from, Vector3 to, float size, Color? color = null, float? duration = null)
		{
			if (m_behaviour != null)
			{
				lock (m_behaviour.data)
				{
					m_behaviour.data.Add(new Data(color ?? m_color, from + m_startOffset, to + m_startOffset, duration ?? 0, Data.DrawType.Triangle) { size = size});
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

		public class Data
		{
			public enum DrawType
			{
				Line, Triangle
			}

			public DrawType type;
			public Color color;
			public Vector3 from;
			public Vector3 to;
			public float duration;
			public float size;

            public Data(Color color, Vector3 from, Vector3 to, float duration, DrawType type)
            {
                this.color = color;
                this.from = from;
                this.to = to;
                this.duration = duration;
                this.type = type;
            }
        }

		/// <summary>
		/// Behaviour to call unity's methods in unity's threads for drawing
		/// </summary>
		public class ThreadedDrawerBehaviour : MonoBehaviour, IDisposable
		{
			public List<Data> data = new List<Data>();

			public void Dispose()
			{
				data = null;
			}

			// Update is called once per frame
			void LateUpdate()
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

						switch (item.type)
						{
							default:
								continue;
							case Data.DrawType.Line:
								Debug.DrawLine(item.from, item.to, item.color);
								item.duration -= Time.deltaTime;
								break;
						}

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
						Data item = data[i];

						switch (item.type)
						{
							default:
								continue;
							case Data.DrawType.Triangle:
								Gizmos.color = item.color;
								KinetixLogger.DrawPyramidMesh(item.from, item.to, item.size);
								Gizmos.color = Color.white;
#if UNITY_EDITOR
								if (!UnityEditor.EditorApplication.isPaused)
#endif
									item.duration -= Time.deltaTime;
								break;
						}

						if (item.duration <= 0)
							data.RemoveAt(i);
					}
				}
            }

        }
	}
}
