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
	}
}
