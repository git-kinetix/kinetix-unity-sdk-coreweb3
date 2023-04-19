using UnityEngine;

namespace Kinetix.Internal.Utils
{
	public static class Vector3Utils
	{
		public static Vector3 Multiply(this Vector3 a, Vector3 b)
		{
			Vector3 toReturn = new Vector3
			(
				a.x * b.x,
				a.y * b.y,
				a.z * b.z
			);

			return toReturn;
		}
	}
}
