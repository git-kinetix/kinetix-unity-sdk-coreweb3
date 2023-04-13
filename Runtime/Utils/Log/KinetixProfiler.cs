#if !UNITY_EDITOR
#undef KINETIX_PROFILER
#endif

using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kinetix.Internal.Utils
{
	internal static class KinetixProfiler
	{
		internal static bool _isWindowOpen = false;
		public static bool s_enableCustomProfiler=true;

		internal struct MethodTime
		{
			public string methodName;
			public decimal startTime;
			public decimal endTime;
			public int parent;
			public bool HasParent => parent != -1;

			public MethodTime(string methodName, decimal startTime, int parent = -1)
			{
				this.methodName = methodName;
				this.startTime = startTime;
				this.endTime = -1;
				this.parent = parent;
			}
		}

		internal static Dictionary<int, int> _traceStackIndex = new Dictionary<int, int>();
		internal static Dictionary<int, bool> _breakpoints = new Dictionary<int, bool>();
		internal static Dictionary<int,string> _groupName = new Dictionary<int,string>();
		internal static HoleList<List<MethodTime>> _traceByGroup = new HoleList<List<MethodTime>>();

		private static ulong groupI = 0;

		/// <summary>
		/// Synchronousely sample the cost of your code.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="targetObject"></param>
		public static void Start(string name, Object targetObject = null)
		{
#if KINETIX_PROFILER
			Profiler.BeginSample(name, targetObject);
#endif
		}

		/// <summary>
		/// Don't forget to call <see cref="Start(int, string)"/> to avoid memory leak
		/// </summary>
		/// <returns></returns>
		public static int CreateGroup(string name = null)
		{
#if KINETIX_PROFILER
			if (!s_enableCustomProfiler)
				return 0;

			lock(_traceByGroup)
			{
				_traceByGroup.Add(new List<MethodTime>());
				int latestIndex = _traceByGroup.LatestIndex;
				_traceStackIndex[latestIndex] = -1;

				if (name != null)
					name = name + "#" + groupI++;
				else
					name = "unnamed_group#" + groupI++;
				
				_groupName[latestIndex] = name;
				_breakpoints[latestIndex] = true;
				
				return latestIndex;
			}
#else
			return 0;
#endif
		}

		/// <summary>
		/// Use this for async purposes. Dont forget to create a group via <see cref="CreateGroup"/>
		/// </summary>
		public static void Start(int group, string name)
		{
#if KINETIX_PROFILER
			if (!s_enableCustomProfiler)
				return;

			lock (_traceByGroup)
			{
				_traceByGroup[group].Add(new MethodTime(name, (decimal)Time.timeAsDouble, _traceStackIndex[group]));
				_traceStackIndex[group] = _traceByGroup[group].Count - 1;
			}
#endif
		}

		public static void End()
		{
#if KINETIX_PROFILER
			Profiler.EndSample();
#endif
		}

		/// <summary>
		/// Use this for async purposes.
		/// </summary>
		/// <seealso cref="Start"/>
		public static void End(int group)
		{
#if KINETIX_PROFILER
			if (!s_enableCustomProfiler)
				return;

			lock (_traceByGroup)
			{
				int v = _traceStackIndex[group];
				List<MethodTime> methodTimes = _traceByGroup[group];
				int count = methodTimes.Count;

				MethodTime methodTime = methodTimes[v];
				methodTime.endTime = (decimal)Time.timeAsDouble;
				methodTimes[v] = methodTime;

				_traceStackIndex[group] = methodTime.parent;	
			}
#endif
		}

		public static void EndGroup(int group)
		{
#if KINETIX_PROFILER
			if (!s_enableCustomProfiler)
				return;

			lock (_traceByGroup)
			{
				if (_isWindowOpen && _breakpoints[group]) return;
	
				_traceByGroup.RemoveAt(group);
				_traceStackIndex.Remove(group);
				_groupName.Remove(group);
			}
#endif
		}
	}
}
