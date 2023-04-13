// // ----------------------------------------------------------------------------
// // <copyright file="NetworkManager.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kinetix.Internal
{
    internal static class NetworkManager
    {
        public static KinetixNetworkConfiguration Configuration { get { return configuration; } }
        private static KinetixNetworkConfiguration configuration;
        
        private static Dictionary<string, KinetixPeer>           PeerByUUID;

        public static void Initialize()
        {
            Initialize(KinetixNetworkConfiguration.GetDefault()); 
        }

        public static void Initialize(KinetixNetworkConfiguration Configuration)
        {
            PeerByUUID                            = new Dictionary<string, KinetixPeer>();


            // Setting configuration
            if (Configuration == null)
            {
                SetConfiguration(KinetixNetworkConfiguration.GetDefault());
                return;
            }

            SetConfiguration(Configuration);
        }

        public static void RegisterRemotePeerAnimator(string _RemotePeerID, Animator _Animator)
        {
            KinetixCharacterComponent kcc = _Animator.gameObject.AddComponent<KinetixCharacterComponent>();
            kcc.Init(null);
            
            PeerByUUID.Add(_RemotePeerID, new KinetixPeer {
                KCharacterComponent         = _Animator == null ? null : kcc
            });
        }

        public static void UnregisterRemotePeer(string _RemotePeerID)
        {
            KinetixDebug.Log("[UNREGISTERING PEER] : " + _RemotePeerID);
            UnlinkRemotePeer(_RemotePeerID);
        }

        public static void UnregisterAllRemotePeers()
        {
            if (PeerByUUID == null)
                return;

            foreach (string key in PeerByUUID.Keys.ToList())
            {
                UnregisterRemotePeer(key);
            }
        }

        private static void UnlinkRemotePeer(string _RemotePeerID)
        {
            UnlinkRemotePeers(new[] { _RemotePeerID });
        }

        private static void UnlinkRemotePeers(string[] _RemotePeerIDs)
        {
            for (int i = 0; i < _RemotePeerIDs.Length; i++)
            {
                string remotePeerID = _RemotePeerIDs[i];
                
                if (PeerByUUID.ContainsKey(remotePeerID))
                {
                    KinetixPeer   kPeer   = PeerByUUID[remotePeerID];
                    kPeer.KCharacterComponent.Dispose();

                    PeerByUUID.Remove(remotePeerID);
                }
            }
        }

        public static void SetConfiguration(KinetixNetworkConfiguration _Configuration)
        {
            configuration = _Configuration;
        }

        public static KinetixCharacterComponent GetRemoteKCC(string _RemotePeerID) 
        {

            if (PeerByUUID.ContainsKey(_RemotePeerID))
            {
                return PeerByUUID[_RemotePeerID].KCharacterComponent;
            }

            return null;
        }
    }
}
