using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kinetix.Internal
{
	internal enum LogKind
	{
		Info,
		Debug,
		Warning,
		Error,
	}

	/// <summary>
	/// A logger with naming conventions. Check KinetixDebug for log hiding
	/// </summary>
	internal static class KinetixLogger
	{
		private static readonly Regex PREFIX_MATCHER = new Regex(@"([^,]+?)(\s*)(,|$)", RegexOptions.IgnoreCase);
		private const string PREFIX_SUBSTITUTE = "[$1]$3";
#if UNITY_EDITOR || UNITY_ANDROID
		private const string DEBUG_COLOR = "<color=#979797>";
		private const string DEBUG_COLOR_END = "</color>";
#else
		private const string DEBUG_COLOR = "";
		private const string DEBUG_COLOR_END = "";

#endif
		private const string KINETIX = "[Kinetix]";

		/// <param name="condition">Condition you expect to be true</param>
		/// <param name="message"></param>
		/// <param name="logKind"></param>
		/// <param name="devLog">If true, requires DEV_KINETIX define to log</param>
		public static void Assert<T>(bool condition, string message, LogKind logKind, bool devLog)
		{

			Log(typeof(T).Name, message, logKind, devLog);
		}

		/// <param name="message"></param>
		/// <param name="logKind"></param>
		/// <param name="devLog">If true, requires DEV_KINETIX define to log</param>
		public static void Log<T>(string message, LogKind logKind, bool devLog) => Log(typeof(T).Name, message, logKind, devLog);

		/// <param name="prefix">The prefix to use without the []. Use , for multiple prefixes</param>
		/// <param name="message"></param>
		/// <param name="logKind"></param>
		/// <param name="devLog">If true, requires DEV_KINETIX define to log</param>
		public static void Log(string prefix, string message, LogKind logKind, bool devLog)
		{
#if !(UNITY_EDITOR && DEV_KINETIX)
			if (devLog) return;
#endif

			switch (logKind)
			{
				case LogKind.Info:
					LogInfo(prefix, message, devLog);
					break;
				case LogKind.Debug:
					LogDebug(prefix, message, devLog);
					break;
				case LogKind.Warning:
					LogWarning(prefix, message, devLog);
					break;
				case LogKind.Error:
					LogError(prefix, message, devLog);
					break;
				default:
					break;
			}
		}

		public static void LogInfo(string prefix, string message, bool devLog)
		{
#if !(UNITY_EDITOR && DEV_KINETIX)
			if (devLog) return;
#endif

			prefix = ComputePrefix(prefix);
			Debug.Log(prefix + " " + message);
		}

		public static void LogDebug(string prefix, string message, bool devLog)
		{
#if !(UNITY_EDITOR && DEV_KINETIX)
			if (devLog) return;
#endif

#if UNITY_EDITOR
			prefix = ComputePrefix(prefix);
			Debug.Log(DEBUG_COLOR + prefix + " " + message + DEBUG_COLOR_END);
#endif
		}

		public static void LogWarning(string prefix, string message, bool devLog)
		{
#if !(UNITY_EDITOR && DEV_KINETIX)
			if (devLog) return;
#endif

			prefix = ComputePrefix(prefix);
			Debug.LogWarning(prefix + " " + message);

		}

		public static void LogError(string prefix, string message, bool devLog)
		{
#if !(UNITY_EDITOR && DEV_KINETIX)
			if (devLog) return;
#endif

			prefix = ComputePrefix(prefix);
			Debug.LogError(prefix + " " + message);
		}

		private static string ComputePrefix(string prefix) => KINETIX + PREFIX_MATCHER.Replace(prefix, PREFIX_SUBSTITUTE);

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
				
				DrawPyramidMesh(parentPos + globalOffset, pos + globalOffset, Mathf.Clamp(size * 0.15f * scaleFactor, 0.005f * scaleFactor, 0.03f * scaleFactor));
			}
			Gizmos.color = Color.white;
			Profiler.EndSample();
		}

		private static Mesh s_Mesh = null;
		private static Mesh GetMesh()
		{
			return s_Mesh = s_Mesh != null ? s_Mesh : CreateMesh(Vector3.zero, Vector3.up, 1);

            static Mesh CreateMesh(Vector3 from, Vector3 to, float size)
			{
				Resources.UnloadUnusedAssets();
				
				Vector3 cross;
				if (from.sqrMagnitude == 0)
				{
					Vector3 replacement = to == Vector3.forward ? Vector3.up : Vector3.forward;

					cross = Vector3.Cross(replacement, to).normalized * size;
				}
				else if (to.sqrMagnitude == 0)
				{
					Vector3 replacement = from == Vector3.forward ? Vector3.up : Vector3.forward;
					cross = Vector3.Cross(from, replacement).normalized * size;
				}
				else 
					cross = Vector3.Cross(from, to).normalized * size;

				Quaternion rotateOneThird = Quaternion.AngleAxis(360f / 3, to - from);

				Vector3 a = cross + from;
				Vector3 b = (rotateOneThird * cross) + from;
				Vector3 c = (rotateOneThird * rotateOneThird * cross) + from;
				Vector3 d = to;

				Mesh mesh = new Mesh
				{
					vertices = new[]
					{
						a,c,b,
						a,b,d,
						c,a,d,
						b,c,d,
					},
					
					triangles = new[]
					{
						0, 1, 2,
						3, 4, 5,
						6, 7, 8,
						9,10,11,
					}
				};

				mesh.RecalculateNormals();
				mesh.RecalculateTangents();

				return mesh;
			}
		}

        public static void DrawPyramidMesh(Vector3 from, Vector3 to, float size)
		{
			Gizmos.DrawMesh(
				GetMesh(),
				-1,
				from,
				Quaternion.FromToRotation(Vector3.up, to - from),
				new Vector3(size, (from - to).magnitude, size)
			);
		}

		[System.Obsolete]
		public static void DrawPyramidMeshOld(Vector3 from, Vector3 to, float size)
		{
			Profiler.BeginSample(nameof(DrawPyramidMesh));
			Vector3 cross;
			if (from.sqrMagnitude == 0)
			{
				Vector3 replacement = to == Vector3.forward ? Vector3.up : Vector3.forward;

				cross = Vector3.Cross(replacement, to).normalized * size;
			}
			else if (to.sqrMagnitude == 0)
			{
				Vector3 replacement = from == Vector3.forward ? Vector3.up : Vector3.forward;
				cross = Vector3.Cross(from, replacement).normalized * size;
			}
			else
				cross = Vector3.Cross(from, to).normalized * size;
			Quaternion rotateOneThird = Quaternion.AngleAxis(360f / 3, to - from);

			Vector3 a = cross + from;
			Vector3 b = (rotateOneThird * cross) + from;
			Vector3 c = (rotateOneThird * rotateOneThird * cross) + from;
			Vector3 d = to;

			Profiler.BeginSample("NewMesh");
			Mesh mesh = new Mesh
			{
				vertices = new[]
				{
					a,c,b,
					a,b,d,
					c,a,d,
					b,c,d,
				},
				triangles = new[]
				{
					0, 1, 2,
					3, 4, 5,
					6, 7, 8,
					9,10,11,
				}
			};
			Profiler.EndSample();

			//mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();

			Profiler.BeginSample(nameof(Gizmos)+"."+nameof(Gizmos.DrawMesh));
			Gizmos.DrawMesh(mesh);
			Profiler.EndSample();

			Profiler.BeginSample("Destroy");
			if (!Application.isPlaying)
				new Task(() => {
					Thread.Sleep(1);
					Object.DestroyImmediate(mesh);
				}).Start();
			else
				Object.Destroy(mesh);
			Profiler.EndSample();

			Profiler.EndSample();
		}
	}
}
