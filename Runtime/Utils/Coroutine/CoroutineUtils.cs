using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class CoroutineUtils : MonoBehaviour
{
    private static CoroutineUtils instance;
    public static CoroutineUtils Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("CoroutineUtils")
                {
                    hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector
                };
                instance = obj.AddComponent<CoroutineUtils>();
            }

            return instance;
        }
    }
}
