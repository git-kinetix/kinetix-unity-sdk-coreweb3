// // ----------------------------------------------------------------------------
// // <copyright file="KinetixLocalPlayerCache.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using Kinetix.Internal.Retargeting;
using Kinetix.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kinetix.Internal.Cache
{
	internal class LocalPlayerManager: AKinetixManager
	{
		public Action<AnimationIds> OnAnimationStartOnLocalPlayerAnimator;
		public Action<AnimationIds> OnAnimationEndOnLocalPlayerAnimator;

		// Local Avatar
		public KinetixAvatar KAvatar;

		// Callback to notify on retarget local avatar
		private Dictionary<AnimationIds, List<Action>> callbackOnRetargetedAnimationIdOnLocalPlayer;

		// Character component to play automatically on animator
		private KinetixCharacterComponentLocal LocalKinetixCharacterComponent;

		// Animation to preload before the local avatar was registered
		private List<AnimationIds> emotesToPreload;

		// Animations Ids downloaded and retargeted on Local Avatar
		private List<AnimationIds> downloadedEmotesReadyToPlay;
		
		// Play Automatically on Animator
		private bool playAutomaticallyOnAnimator;

        public LocalPlayerManager(ServiceLocator _ServiceLocator, KinetixCoreConfiguration _Config) : base(_ServiceLocator, _Config) {}

        protected override void Initialize(KinetixCoreConfiguration _Config)
		{
			playAutomaticallyOnAnimator = _Config.PlayAutomaticallyAnimationOnAnimators;
			
			callbackOnRetargetedAnimationIdOnLocalPlayer = new Dictionary<AnimationIds, List<Action>>();
			downloadedEmotesReadyToPlay                  = new List<AnimationIds>();
			emotesToPreload                              = new List<AnimationIds>();
		}

		public void AddPlayerCharacterComponent(Animator _Animator)
		{
			KAvatar            = CreateKinetixAvatar(_Animator.avatar, _Animator.transform, EExportType.KinetixClip);
			LocalKinetixCharacterComponent = AddKCCAndInit(_Animator, KAvatar);
			OnRegisterLocalPlayer();
		}

		public void AddPlayerCharacterComponent(Animator _Animator, RootMotionConfig _RootMotionConfig)
		{
			if (KAvatar != null)
				UnregisterPlayerComponent();

			KAvatar            = CreateKinetixAvatar(_Animator.avatar, _Animator.transform, EExportType.KinetixClip);
			LocalKinetixCharacterComponent = AddKCCAndInit(_Animator, KAvatar, _RootMotionConfig);
			OnRegisterLocalPlayer();
		}

		public void AddPlayerCharacterComponent(DataBoneTransform _Root, Transform _RootTransform,IPoseInterpreter _PoseInterpreter)
		{
			KAvatar            = CreateKinetixAvatar(_Root, _RootTransform, EExportType.KinetixClip);
			LocalKinetixCharacterComponent = AddKCCAndInit(_PoseInterpreter, KAvatar);
			OnRegisterLocalPlayer();
		}

		public void AddPlayerCharacterComponent(DataBoneTransform _Root, Transform _RootTransform, IPoseInterpreter _PoseInterpreter, RootMotionConfig _RootMotionConfig)
		{
			KAvatar            = CreateKinetixAvatar(_Root, _RootTransform, EExportType.KinetixClip);
			LocalKinetixCharacterComponent = AddKCCAndInit(_PoseInterpreter, KAvatar, _RootMotionConfig);
			OnRegisterLocalPlayer();
		}

        #region LOAD
       
		public void AddPlayerCharacterComponent(Avatar _Avatar, Transform _RootTransform, EExportType _ExportType)
		{
			if (KAvatar != null)
				UnregisterPlayerComponent();
			
			KAvatar = CreateKinetixAvatar(_Avatar, _RootTransform, _ExportType);
			OnRegisterLocalPlayer();
		}

		private KinetixAvatar CreateKinetixAvatar(Avatar _Avatar, Transform _RootTransform, EExportType _ExportType)
		{
			return new KinetixAvatar()
			{
				Avatar = new AvatarData(_Avatar, _RootTransform), Root = _RootTransform, ExportType = _ExportType
			};
		}
		private KinetixAvatar CreateKinetixAvatar(DataBoneTransform _Root, Transform _RootTransform, EExportType _ExportType)
		{
			return new KinetixAvatar()
			{
				Avatar = new AvatarData(_Root, _RootTransform), Root = _RootTransform, ExportType = _ExportType
			};
		}
		
		public async void GetRetargetedKinetixClipLegacy(AnimationIds _AnimationIds, Action<KinetixClip> _OnSuccess, Action _OnFailure)
		{
			KinetixEmote emote = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_AnimationIds);

			try
			{
				KinetixClip clip = await KinetixCoreBehaviour.ServiceLocator.Get<RetargetingService>().GetRetargetedClipByAvatar<KinetixClip, KinetixClipExporter>(emote, KAvatar, SequencerPriority.Low, false);
				_OnSuccess?.Invoke(clip);
			}
			catch (OperationCanceledException)
			{
				KinetixDebug.Log("Loading animation operation was cancelled for emote : " + emote.Ids.UUID);
				_OnFailure?.Invoke();
			}
			catch (Exception e)
			{
				KinetixDebug.LogWarning($"Failed loading animation with id { emote.Ids.UUID } with error : " + e.Message);
				_OnFailure?.Invoke();
			}		
		}
		
		public async void GetRetargetedAnimationClipLegacy(AnimationIds _AnimationIds, Action<AnimationClip> _OnSuccess, Action _OnFailure)
		{
			KinetixEmote emote = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_AnimationIds);

			try
			{
				AnimationClip clip = await KinetixCoreBehaviour.ServiceLocator.Get<RetargetingService>().GetRetargetedClipByAvatar<AnimationClip, AnimationClipExport>(emote, KAvatar, SequencerPriority.Low, false);
				_OnSuccess?.Invoke(clip);
			}
			catch (OperationCanceledException)
			{
				KinetixDebug.Log("Loading animation operation was cancelled for emote : " + emote.Ids.UUID);
				_OnFailure?.Invoke();
			}
			catch (Exception e)
			{
				KinetixDebug.LogWarning($"Failed loading animation with id { emote.Ids.UUID } with error : " + e.Message);
				_OnFailure?.Invoke();
			}
		}
		
		public void UnregisterPlayerComponent()
		{
			if (LocalKinetixCharacterComponent != null)
			{
				LocalKinetixCharacterComponent.Dispose();
				LocalKinetixCharacterComponent = null;
			}
			
			ForceUnloadLocalPlayerAnimations(downloadedEmotesReadyToPlay.ToArray());
			emotesToPreload.Clear();
			callbackOnRetargetedAnimationIdOnLocalPlayer.Clear();
			downloadedEmotesReadyToPlay.Clear();
			KAvatar                        = null;

			KinetixCoreBehaviour.ManagerLocator.Get<AccountManager>().OnUpdatedAccount?.Invoke();
		}

		public void LoadLocalPlayerAnimation(AnimationIds _Ids, string _LockId, Action _OnSuccess = null, Action _OnFailure = null)
        {
            KinetixEmote emote = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_Ids);
            serviceLocator.Get<LockService>().Lock(new KinetixEmoteAvatarPair() { Emote = emote, Avatar = KAvatar}, _LockId);
            
            LoadLocalPlayerAnimationInternal(emote, _OnSuccess, _OnFailure);
        }

        public void LoadLocalPlayerAnimations(AnimationIds[] _Ids, string _LockId, Action _OnSuccess = null, Action _OnFailure = null)
        {
            for (int i = 0; i < _Ids.Length; i++)
            {
				LoadLocalPlayerAnimation(_Ids[i], _LockId, _OnSuccess, _OnFailure);
            }
		}

		// To be called only in this class in case emote are preloaded with a lock
		private void LoadLocalPlayerAnimations(AnimationIds[] _Ids, Action _OnSuccess = null, Action _OnFailure = null)
		{
			LoadLocalPlayerAnimations(_Ids, "", _OnSuccess, _OnFailure);
		}

		private async void LoadLocalPlayerAnimationInternal(KinetixEmote _KinetixEmote, Action _OnSuccess, Action _OnFailure)
		{
			if (KAvatar == null)
			{
				if (!emotesToPreload.Contains(_KinetixEmote.Ids))
					emotesToPreload.Add(_KinetixEmote.Ids);
				return;
			}

			downloadedEmotesReadyToPlay ??= new List<AnimationIds>();

			if (!downloadedEmotesReadyToPlay.Contains(_KinetixEmote.Ids))
				downloadedEmotesReadyToPlay.Add(_KinetixEmote.Ids);

			try
			{
				// TODO find a better way, this is not sexy at all
				object retargetingResult;

				if (KAvatar.ExportType == EExportType.KinetixClip)
				{
					retargetingResult = await KinetixCoreBehaviour.ServiceLocator.Get<RetargetingService>().GetRetargetedClipByAvatar<KinetixClip, KinetixClipExporter>(_KinetixEmote, KAvatar, SequencerPriority.Low, false);
				}
				else
				{
					retargetingResult = await KinetixCoreBehaviour.ServiceLocator.Get<RetargetingService>().GetRetargetedClipByAvatar<AnimationClip, AnimationClipExport>(_KinetixEmote, KAvatar, SequencerPriority.Low, false);
				}

				if (retargetingResult != null)
				{
					_OnSuccess?.Invoke();
				}
				else
				{
					_OnFailure?.Invoke();
				}
			}
			catch (OperationCanceledException)
			{
				KinetixDebug.Log("Loading animation operation was cancelled for emote : " + _KinetixEmote.Ids.UUID);
				_OnFailure?.Invoke();
			}
			catch (Exception e)
			{
				KinetixDebug.LogWarning($"Failed loading animation with id { _KinetixEmote.Ids.UUID } with error : " + e.Message);
				_OnFailure?.Invoke();
			}
		}
		#endregion

		#region UNLOAD
		public void UnloadLocalPlayerAnimation(AnimationIds _Ids, string _LockId)
		{
			UnlockLocalPlayerAnimation(_Ids, _LockId);
			RemoveLocalPlayerEmotesReadyToPlay(_Ids);
		}

		public void UnloadLocalPlayerAnimations(AnimationIds[] _Ids, string _LockId = "")
		{
			foreach (AnimationIds ids in _Ids)
			{
				UnlockLocalPlayerAnimation(ids, _LockId);
				RemoveLocalPlayerEmotesReadyToPlay(ids);
			}
		}

		public void ForceUnloadLocalPlayerAnimations(AnimationIds[] _Ids)
		{
			foreach (AnimationIds ids in _Ids)
			{
				RemoveLocalPlayerEmotesReadyToPlay(ids);
				KinetixEmote emote = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(ids);

				KinetixCoreBehaviour.ServiceLocator.Get<LockService>().ForceUnload(new KinetixEmoteAvatarPair(emote, KAvatar));
			}
		}

		public void RemoveLocalPlayerEmotesToPreload(AnimationIds[] _Ids)
		{
			emotesToPreload ??= new List<AnimationIds>();
			for (int i = 0; i < _Ids.Length; i++)
			{
				if (emotesToPreload.Contains(_Ids[i]))
					emotesToPreload.Remove(_Ids[i]);
			}
		}
		
		private void RemoveLocalPlayerEmotesReadyToPlay(AnimationIds _Ids)
		{
			downloadedEmotesReadyToPlay ??= new List<AnimationIds>();
			if (downloadedEmotesReadyToPlay.Contains(_Ids))
				downloadedEmotesReadyToPlay.Remove(_Ids);
		}
		
		#endregion

		#region LOCKS
		
		public void LockLocalPlayerAnimation(AnimationIds _Ids, string _LockId)
		{
			KinetixEmote emote = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_Ids);

			KinetixCoreBehaviour.ServiceLocator.Get<LockService>().Lock(new KinetixEmoteAvatarPair(emote, KAvatar), _LockId);
		}
		
		public void LockLocalPlayerAnimations(AnimationIds[] _Ids, string _LockId)
		{
			foreach (AnimationIds ids in _Ids)
			{
				LockLocalPlayerAnimation(ids, _LockId);
			}
		}

		public void UnlockLocalPlayerAnimation(AnimationIds _Ids, string _LockId)
		{
			KinetixEmote emote = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_Ids);

			KinetixCoreBehaviour.ServiceLocator.Get<LockService>().Unlock(new KinetixEmoteAvatarPair(emote, KAvatar), _LockId);
		}
		
		public void UnlockLocalPlayerAnimations(AnimationIds[] _Ids, string _LockId)
		{
			foreach (AnimationIds ids in _Ids)
			{
				UnlockLocalPlayerAnimation(ids, _LockId);
			}
		}

		#endregion
		
		public bool IsAnimationAvailable(AnimationIds _Ids)
		{
			return KAvatar != null && KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_Ids).HasAnimationRetargeted(KAvatar);
		}

		public bool IsEmoteUsedByPlayer(AnimationIds _Ids)
		{
			return KAvatar != null && downloadedEmotesReadyToPlay.Contains(_Ids);
		}

		public void GetNotifiedOnAnimationReadyOnLocalPlayer(AnimationIds _Ids, Action _OnSucceed)
		{
			try
			{
				if (KAvatar != null)
				{
					KinetixEmote emote = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_Ids);
					KinetixCoreBehaviour.ServiceLocator.Get<RetargetingService>().RegisterCallbacksOnRetargetedByAvatar(emote, KAvatar, _OnSucceed);
				}
				else
				{
					if (!callbackOnRetargetedAnimationIdOnLocalPlayer.ContainsKey(_Ids))
						callbackOnRetargetedAnimationIdOnLocalPlayer.Add(_Ids, new List<Action>());
					callbackOnRetargetedAnimationIdOnLocalPlayer[_Ids].Add(_OnSucceed);
				}
			}
			catch (Exception e)
			{
				KinetixDebug.LogWarning("Can't get notified on animation ready for local player : " + e.Message);
			}
		}

		private KinetixCharacterComponentLocal AddKCCAndInit(Animator _Animator, KinetixAvatar _KinetixAvatar)
		{
			KinetixCharacterComponentLocal kcc = new KinetixCharacterComponentLocal();

			kcc.RegisterPoseInterpreter(new AnimatorPoseInterpetor(_Animator, _KinetixAvatar.Avatar.avatar, _Animator.GetComponentsInChildren<SkinnedMeshRenderer>().GetARKitRenderers()));
			kcc.AutoPlay = true;

			kcc.Init(_KinetixAvatar);
			kcc.OnAnimationStart += AnimationStartOnLocalPlayerAnimator;
			kcc.OnAnimationEnd   += AnimationEndOnLocalPlayerAnimator;
			return kcc;
		}

		private KinetixCharacterComponentLocal AddKCCAndInit(Animator _Animator, KinetixAvatar _KinetixAvatar, RootMotionConfig _RootMotionConfig)
		{
			KinetixCharacterComponentLocal kcc = new KinetixCharacterComponentLocal();

			kcc.RegisterPoseInterpreter(new AnimatorPoseInterpetor(_Animator, _KinetixAvatar.Avatar.avatar, _Animator.GetComponentsInChildren<SkinnedMeshRenderer>().GetARKitRenderers()));
			kcc.AutoPlay = true;

			kcc.Init(_KinetixAvatar, _RootMotionConfig);
			kcc.OnAnimationStart += AnimationStartOnLocalPlayerAnimator;
			kcc.OnAnimationEnd   += AnimationEndOnLocalPlayerAnimator;
			return kcc;
		}

		private KinetixCharacterComponentLocal AddKCCAndInit(IPoseInterpreter _PoseInterpreter, KinetixAvatar _KinetixAvatar)
		{
			KinetixCharacterComponentLocal kcc = new KinetixCharacterComponentLocal();

			kcc.RegisterPoseInterpreter(_PoseInterpreter);
			kcc.AutoPlay = true;

			kcc.Init(_KinetixAvatar);
			kcc.OnAnimationStart += AnimationStartOnLocalPlayerAnimator;
			kcc.OnAnimationEnd   += AnimationEndOnLocalPlayerAnimator;
			return kcc;
		}

		private KinetixCharacterComponentLocal AddKCCAndInit(IPoseInterpreter _PoseInterpreter, KinetixAvatar _KinetixAvatar, RootMotionConfig _RootMotionConfig)
		{
			KinetixCharacterComponentLocal kcc = new KinetixCharacterComponentLocal();

			kcc.RegisterPoseInterpreter(_PoseInterpreter);
			kcc.AutoPlay = true;

			kcc.Init(_KinetixAvatar, _RootMotionConfig);
			kcc.OnAnimationStart += AnimationStartOnLocalPlayerAnimator;
			kcc.OnAnimationEnd   += AnimationEndOnLocalPlayerAnimator;
			return kcc;
		}

		private void AnimationStartOnLocalPlayerAnimator(AnimationIds _AnimationIds)
		{
			OnAnimationStartOnLocalPlayerAnimator?.Invoke(_AnimationIds);
		}

		private void AnimationEndOnLocalPlayerAnimator(AnimationIds _AnimationIds)
		{
			OnAnimationEndOnLocalPlayerAnimator?.Invoke(_AnimationIds);
		}

		public AnimationIds[] GetDownloadedAnimationsReadyToPlay()
		{
			return downloadedEmotesReadyToPlay.ToArray();
		}
		
		public void PlayAnimation(AnimationIds _AnimationsIds, Action<AnimationIds> _OnPlayedAnimation)
		{
			if (LocalKinetixCharacterComponent == null)
			{
				return;
			}

			if (playAutomaticallyOnAnimator)
				LocalKinetixCharacterComponent.PlayAnimation(_AnimationsIds);
			else
				_OnPlayedAnimation?.Invoke(_AnimationsIds);
		}

		public void PlayAnimationQueue(AnimationIds[] _AnimationIdsArray, bool _Loop, Action<AnimationIds[]> _OnPlayedAnimations)
		{
			if (LocalKinetixCharacterComponent == null)
			{
				KinetixDebug.LogWarning("Local player was not registered");
				return;
			}

			if (playAutomaticallyOnAnimator)
				LocalKinetixCharacterComponent.PlayAnimationQueue(_AnimationIdsArray);
			else
				_OnPlayedAnimations?.Invoke(_AnimationIdsArray);
		}

		public void StopAnimation()
		{
			if (LocalKinetixCharacterComponent == null)
			{
				KinetixDebug.LogWarning("Local player was not registered");
				return;
			}

			if (playAutomaticallyOnAnimator)
				LocalKinetixCharacterComponent.StopAnimation();
		}
		
		private void OnRegisterLocalPlayer()
		{
			foreach (KeyValuePair<AnimationIds, List<Action>> kvp in callbackOnRetargetedAnimationIdOnLocalPlayer)
			{
				for (int i = 0; i < kvp.Value.Count; i++)
				{
					try
					{
						KinetixEmote emote = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(kvp.Key);
						//KinetixCoreBehaviour.ServiceLocator.Get<RetargetingService>().GetRetargetedClipByAvatar<KinetixClip, KInetix>(emote, KAvatar, kvp.Value[i], false);
					}
					catch (Exception e)
					{
						KinetixDebug.LogWarning("Could not subscribe for retargeting callback: " + e.Message);
					}
				}
			}

			if (emotesToPreload.Count <= 0)
				return;
			
			LoadLocalPlayerAnimations(emotesToPreload.ToArray());
		}


		public KinetixCharacterComponentLocal GetLocalKCC()
		{
			return LocalKinetixCharacterComponent;
		}
	}
}
