using System;
using System.Collections.Generic;

namespace Kinetix.Internal
{
    internal class LockService: IKinetixService
    {
        public event Action<KinetixEmoteAvatarPair> OnRequestEmoteUnload;

        public Dictionary<KinetixEmoteAvatarPair, List<string>> Locks => locksByPairs;
        
        private Dictionary<KinetixEmoteAvatarPair, List<string>> locksByPairs;

        internal LockService()
        {
            locksByPairs = new Dictionary<KinetixEmoteAvatarPair, List<string>>();
        }
        
        /// <summary>
        /// Prevents the animation unloading (register it as used by a component). 
        /// </summary>
        /// <param name="_EmoteAvatarPair"></param>
        /// <param name="_LockId"></param>
        public void Lock(KinetixEmoteAvatarPair _EmoteAvatarPair, string _LockId)
        {
            if (string.IsNullOrEmpty(_LockId))
                return;

            if (!locksByPairs.ContainsKey(_EmoteAvatarPair))
                locksByPairs.Add(_EmoteAvatarPair, new List<string>());
                
            if (locksByPairs[_EmoteAvatarPair].Contains(_LockId))
                return;

            KinetixDebug.Log("[LOCK] Animation : " + _EmoteAvatarPair.Emote.Ids + " by " + _LockId);

            locksByPairs[_EmoteAvatarPair].Add(_LockId);
        }

        /// <summary>
        /// Unregister a usage for an emote, if no component use it it's then unloaded
        /// </summary>
        /// <param name="_EmoteAvatarPair"></param>
        /// <param name="_LockId"></param>
        public void Unlock(KinetixEmoteAvatarPair _EmoteAvatarPair, string _LockId)
        {
            if (!locksByPairs.ContainsKey(_EmoteAvatarPair) || !locksByPairs[_EmoteAvatarPair].Contains(_LockId))
            {
                KinetixDebug.LogWarning("Tried to unlock emote " + _EmoteAvatarPair.Emote.Ids +
                                        " but it wasn't locked anymore and should have been disposed already.");
                return;
            }

            locksByPairs[_EmoteAvatarPair].Remove(_LockId);

            KinetixDebug.Log("[UNLOCK] Animation : " + _EmoteAvatarPair.Emote.Ids + ". Locks left: " + locksByPairs[_EmoteAvatarPair].Count);

            if (locksByPairs[_EmoteAvatarPair].Count == 0)
                Unload(_EmoteAvatarPair);
        }
        
        /// <summary>
        /// Unloads an emote wheither it's used or not
        /// </summary>
        /// <param name="_EmoteAvatarPair"></param>
        public void ForceUnload(KinetixEmoteAvatarPair _EmoteAvatarPair)
        {
            KinetixDebug.Log("[FORCE UNLOAD] Animation : " + _EmoteAvatarPair.Emote.Ids);

            locksByPairs.Remove(_EmoteAvatarPair);

            Unload(_EmoteAvatarPair);
        }

        /// <summary>
        /// Called when an emote is not used anymore
        /// </summary>
        /// <param name="_EmoteAvatarPair"></param>
        private void Unload(KinetixEmoteAvatarPair _EmoteAvatarPair)
        {
            KinetixDebug.Log("[UNLOAD] Animation : " + _EmoteAvatarPair.Emote.Ids);

            OnRequestEmoteUnload?.Invoke(_EmoteAvatarPair);
        }
    }
}
