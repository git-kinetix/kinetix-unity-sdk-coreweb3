// // ----------------------------------------------------------------------------
// // <copyright file="NetworkManager.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using Kinetix.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kinetix.Internal
{
	internal class NetworkManager: AKinetixManager
	{
		public KinetixNetworkConfiguration Configuration { get { return configuration; } }
		private KinetixNetworkConfiguration configuration;
		
		private Dictionary<string, KinetixPeer>           PeerByUUID;

        public NetworkManager(ServiceLocator _ServiceLocator, KinetixCoreConfiguration _Config) : base(_ServiceLocator, _Config) {}

        protected override void Initialize(KinetixCoreConfiguration _Config)
        {
            Initialize(_Config.NetworkConfiguration);
        }

        protected void Initialize()
		{
			Initialize(KinetixNetworkConfiguration.GetDefault()); 
		}

		protected void Initialize(KinetixNetworkConfiguration Configuration)
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

		public void RegisterRemotePeerAnimator(string _RemotePeerID, Animator _Animator)
		{
			KinetixCharacterComponentRemote kcc = new KinetixCharacterComponentRemote();
			KinetixAvatar kinetixAvatar = new KinetixAvatar()
			{
				Root =
					_Animator == null ? null : _Animator.transform,
			};
			kinetixAvatar.Avatar = new AvatarData(_Animator.avatar, kinetixAvatar.Root);

			kcc.Init(
				kinetixAvatar
			);

			kcc.RegisterPoseInterpreter(new AnimatorPoseInterpetor(_Animator, _Animator.avatar, _Animator.GetComponentsInChildren<SkinnedMeshRenderer>().GetARKitRenderers()));
			kcc.AutoPlay = true;

			PeerByUUID.Add(_RemotePeerID, new KinetixPeer {
				KCharacterComponent = _Animator == null ? null : kcc
			});
		}

		public void RegisterRemotePeerCustom(string _RemotePeerID, DataBoneTransform _Root, Transform _RootTransform, IPoseInterpreter _PoseInterpreter)
		{
			KinetixCharacterComponentRemote kcc = new KinetixCharacterComponentRemote();
			KinetixAvatar kinetixAvatar = new KinetixAvatar()
			{
				Root = _RootTransform,
				Avatar = new AvatarData(_Root, _RootTransform)
			};

			kcc.Init(
				kinetixAvatar
			);

			kcc.RegisterPoseInterpreter(_PoseInterpreter);
			kcc.AutoPlay = true;

			PeerByUUID.Add(_RemotePeerID, new KinetixPeer {
				KCharacterComponent = kcc
			});
		}

		public void UnregisterRemotePeer(string _RemotePeerID)
		{
			KinetixDebug.Log("[UNREGISTERING PEER] : " + _RemotePeerID);
			UnlinkRemotePeer(_RemotePeerID);
		}

		public void UnregisterAllRemotePeers()
		{
			if (PeerByUUID == null)
				return;

			foreach (string key in PeerByUUID.Keys.ToList())
			{
				UnregisterRemotePeer(key);
			}
		}

		private void UnlinkRemotePeer(string _RemotePeerID)
		{
			UnlinkRemotePeers(new[] { _RemotePeerID });
		}

		private void UnlinkRemotePeers(string[] _RemotePeerIDs)
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

		public void SetConfiguration(KinetixNetworkConfiguration _Configuration)
		{
			configuration = _Configuration;
		}

		public KinetixCharacterComponentRemote GetRemoteKCC(string _RemotePeerID) 
		{

			if (PeerByUUID.ContainsKey(_RemotePeerID))
			{
				return PeerByUUID[_RemotePeerID].KCharacterComponent;
			}

			return null;
		}
	}
}
