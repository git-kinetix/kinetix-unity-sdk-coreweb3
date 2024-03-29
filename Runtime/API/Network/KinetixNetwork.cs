// // ----------------------------------------------------------------------------
// // <copyright file="KinetixNetwork.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using UnityEngine;
using Kinetix.Internal.Network;

namespace Kinetix.Internal
{
    public class KinetixNetwork
    {        
        /// <summary>
        /// Use this to set the current Kinetix Network Configuration.
        /// </summary>
        public void SetConfiguration(KinetixNetworkConfiguration _Configuration)
        {
            KinetixNetworkBehaviour.SetConfiguration(_Configuration);
        }


        /// <summary>
        /// Returns the current Kinetix Network Configuration.
        /// </summary>
        public static KinetixNetworkConfiguration GetConfiguration()
        {
            return KinetixNetworkBehaviour.GetConfiguration();
        }


        /// <summary>
        /// Returns the KinetixCharacterComponent for a remote peer
        /// </summary>
        public KinetixCharacterComponentRemote GetRemoteKCC(string _RemotePeerID) 
        {
            return KinetixNetworkBehaviour.GetRemoteKCC(_RemotePeerID);
        }


        /// <summary>
        /// Register remote peer Animator.
        /// </summary>
        /// <param name="_RemotePeerUUID">UUID of the remote peer in the room</param>
        /// <param name="_Animator">Animator of the remote peer</param>
        public void RegisterRemotePeerAnimator(string _RemotePeerUUID, Animator _Animator)
        {
            KinetixNetworkBehaviour.RegisterRemotePeer(_RemotePeerUUID, _Animator);
        }

		/// <summary>
		/// Register remote peer with a custom hierarchy.
		/// </summary>
		/// <param name="_RemotePeerUUID">UUID of the remote peer in the room</param>
		/// <param name="_Root">The root of the skeleton's hierarchy. In T pose</param>
		/// <param name="_RootTransform">The root GameObject of your avatar</param>
		/// <param name="_PoseInterpreter">The interpretor to apply poses to your avatar</param>
		public void RegisterRemotePeerCustom(string _RemotePeerUUID, DataBoneTransform _Root, Transform _RootTransform, IPoseInterpreter _PoseInterpreter)
		{
			KinetixNetworkBehaviour.RegisterRemotePeerCustom(_RemotePeerUUID, _Root, _RootTransform, _PoseInterpreter);
		}

		/// <summary>
		/// Unregister remote peer.
		/// </summary>
		/// <param name="_RemotePeerUUID">UUID of the remote peer in the room.</param>
		public void UnregisterRemotePeer(string _RemotePeerUUID)
        {
            KinetixNetworkBehaviour.UnregisterRemotePeer(_RemotePeerUUID);
        }

        /// <summary>
        /// Unregister all remote peers.
        /// </summary>
        public void UnregisterAllRemotePeers()
        {
            KinetixNetworkBehaviour.UnregisterAllRemotePeers();
        }
    }
}
