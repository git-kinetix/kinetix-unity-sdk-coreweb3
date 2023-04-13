// // ----------------------------------------------------------------------------
// // <copyright file="KinetixNetworkBehaviour.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Kinetix.Internal.Network
{
    internal static class KinetixNetworkBehaviour
    {
        
        public static void RegisterRemotePeer(string _RemotePeerID, Animator _Animator)
        {
            NetworkManager.RegisterRemotePeerAnimator(_RemotePeerID, _Animator);
        }

        public static void UnregisterRemotePeer(string _RemotePeerID)
        {
            NetworkManager.UnregisterRemotePeer(_RemotePeerID);
        }

        public static void UnregisterAllRemotePeers()
        {
            NetworkManager.UnregisterAllRemotePeers();
        }

        public static void SetConfiguration(KinetixNetworkConfiguration _Configuration)
        {
            NetworkManager.SetConfiguration(_Configuration);
        }

        public static KinetixNetworkConfiguration GetConfiguration()
        {
            return NetworkManager.Configuration;
        }

        public static KinetixCharacterComponent GetRemoteKCC(string _RemotePeerID) 
        {
            return NetworkManager.GetRemoteKCC(_RemotePeerID);
        }

    }
}
