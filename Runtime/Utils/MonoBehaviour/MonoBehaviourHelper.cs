using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kinetix.Utils
{
    public class MonoBehaviourHelper : MonoBehaviour
    {
        #region Singleton
        private static MonoBehaviourHelper instance;
        
        public static MonoBehaviourHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("[Kinetix] MonoBehaviourHelper")
                    {
                        hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector
                    };
                    instance = obj.AddComponent<MonoBehaviourHelper>();
                    DontDestroyOnLoad(obj);
                }

                return instance;
            }
        }
        #endregion

        public Action OnDestroyEvent;

        private void OnDestroy()
        {
            OnDestroyEvent?.Invoke();
        }
    }
    

}

