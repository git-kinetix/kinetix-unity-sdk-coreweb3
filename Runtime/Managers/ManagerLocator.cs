using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kinetix.Internal
{
    public class ManagerLocator
    {
        private Dictionary<Type, IKinetixManager> managers;

        public ManagerLocator()
        {
            managers = new Dictionary<Type, IKinetixManager>();
        }

        public bool Register<TManager>(TManager manager) where TManager: IKinetixManager
        {
            if (managers.ContainsKey(typeof(TManager))) return false;

            managers.Add(typeof(TManager), manager);

            return true;
        }

        public TManager Get<TManager>() where TManager: IKinetixManager
        {
            IKinetixManager returnValue;

            managers.TryGetValue(typeof(TManager), out returnValue);

            return (TManager) returnValue;
        }
    }
}
