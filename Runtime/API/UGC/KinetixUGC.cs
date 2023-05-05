using System;

namespace Kinetix.Internal
{
    public class KinetixUGC
    {
        /// <summary>
        /// Event called upon token expiration
        /// </summary>
        public event Action OnUGCTokenExpired;

        public bool IsUGCAvailable()
        {
            // UGC IS NOT AVAILABLE FOR WEB 3
            return false;
        }

        public void StartPollingForUGC()
        {
            // UGC IS NOT AVAILABLE FOR WEB 3
        }

        public void StartPollingForNewUGCToken()
        {
            // UGC IS NOT AVAILABLE FOR WEB 3
        }

        public void GetUgcUrl(Action<string> urlFetchedCallback)
        {
            // UGC IS NOT AVAILABLE FOR WEB 3
        }

        public KinetixUGC()
        {
        }

        private void UGCTokenExpired()
        {
            OnUGCTokenExpired?.Invoke();
        }
    }
}
