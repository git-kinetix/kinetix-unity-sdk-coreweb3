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
            KinetixCoreBehaviour.ManagerLocator.Get<NetworkManager>().RegisterRemotePeerAnimator(_RemotePeerID, _Animator);
        }
        
        public static void RegisterRemotePeerCustom(string _RemotePeerID, DataBoneTransform _Root, Transform _RootTransform, IPoseInterpreter _PoseInterpreter)
		{
            KinetixCoreBehaviour.ManagerLocator.Get<NetworkManager>().RegisterRemotePeerCustom(_RemotePeerID, _Root, _RootTransform, _PoseInterpreter);
        }

        public static void UnregisterRemotePeer(string _RemotePeerID)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<NetworkManager>().UnregisterRemotePeer(_RemotePeerID);
        }

        public static void UnregisterAllRemotePeers()
        {
            KinetixCoreBehaviour.ManagerLocator.Get<NetworkManager>().UnregisterAllRemotePeers();
        }

        public static void SetConfiguration(KinetixNetworkConfiguration _Configuration)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<NetworkManager>().SetConfiguration(_Configuration);
        }

        public static KinetixNetworkConfiguration GetConfiguration()
        {
            return KinetixCoreBehaviour.ManagerLocator.Get<NetworkManager>().Configuration;
        }

        public static KinetixCharacterComponentRemote GetRemoteKCC(string _RemotePeerID) 
        {
            return KinetixCoreBehaviour.ManagerLocator.Get<NetworkManager>().GetRemoteKCC(_RemotePeerID);
        }

    }
}
