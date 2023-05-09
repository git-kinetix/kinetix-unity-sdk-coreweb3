using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kinetix.Internal
{
	public static class KinetixDebug
	{
		public static bool c_ShowLog = false;

		public static void Log(string _Log)
		{
			if (c_ShowLog)
				Debug.Log("[KINETIX] " + _Log);
		}

		public static void LogWarning(string _Log)
		{
			if (c_ShowLog)
				Debug.LogWarning("[KINETIX] " + _Log);
		}

		public static void LogError(string _Log)
		{
			if (c_ShowLog)
				Debug.LogError("[KINETIX] " + _Log);
		}

		internal static void LogException(Exception argumentException)
		{
			if (c_ShowLog)
				Debug.LogError("[KINETIX] " + argumentException.Message + " \n " + argumentException);
		}

        public static void LogWarningException(Exception argumentException)
        {
            if (c_ShowLog)
                Debug.LogWarning("[KINETIX] " + argumentException.Message + " \n " + argumentException);
        }
	}
}

