using System.Linq;
using UnityEngine;

namespace Kinetix.Utils
{
	public static class SkinedMeshListUtils
	{
		public static SkinnedMeshRenderer[] GetARKitRenderers(this SkinnedMeshRenderer[] renderers)
		{
			return renderers.Where(Where).ToArray();

			bool Where(SkinnedMeshRenderer s)
			{
				if (s.sharedMesh == null)
					return false;
				
				return s.sharedMesh.blendShapeCount != 0 &&
					s.sharedMesh.GetBlendShapeIndex( ARKitBlendshapes.mouthClose  .ToString() ) != -1 ||
					s.sharedMesh.GetBlendShapeIndex( ARKitBlendshapes.mouthOpen   .ToString() ) != -1 ||
					s.sharedMesh.GetBlendShapeIndex( ARKitBlendshapes.eyeBlinkLeft.ToString() ) != -1;
			}
		}
	}
}
