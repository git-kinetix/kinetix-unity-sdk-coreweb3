// // ----------------------------------------------------------------------------
// // <copyright file="KinetixLocalPlayerCache.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kinetix.Internal.Cache
{
    internal static class LocalPlayerManager
    {
        public static Action<AnimationIds> OnAnimationStartOnLocalPlayerAnimator;
        public static Action<AnimationIds> OnAnimationEndOnLocalPlayerAnimator;

        // Local Avatar
        public static KinetixAvatar KAvatar;

        // Callback to notify on retarget local avatar
        private static Dictionary<AnimationIds, List<Action>> callbackOnRetargetedAnimationIdOnLocalPlayer;

        // Character component to play automatically on animator
        private static KinetixCharacterComponent LocalKinetixCharacterComponent;

        // Animation to preload before the local avatar was registered
        private static List<AnimationIds> emotesToPreload;

        // Animations Ids downloaded and retargeted on Local Avatar
        private static List<AnimationIds> downloadedEmotesReadyToPlay;
        
        // Play Automatically on Animator
        private static bool playAutomaticallyOnAnimator;

        public static void Initialize(bool _PlayAutomaticallyOnAnimator)
        {
            playAutomaticallyOnAnimator = _PlayAutomaticallyOnAnimator;
            
            callbackOnRetargetedAnimationIdOnLocalPlayer = new Dictionary<AnimationIds, List<Action>>();
            downloadedEmotesReadyToPlay                  = new List<AnimationIds>();
            emotesToPreload                              = new List<AnimationIds>();
        }

        public static void AddPlayerCharacterComponent(Animator _Animator)
        {
            if (KAvatar != null)
                UnregisterPlayerComponent();
            
            KAvatar            = CreateKinetixAvatar(_Animator.avatar, _Animator.transform, EExportType.AnimationClipLegacy);
            LocalKinetixCharacterComponent = AddKCCAndInit(_Animator, KAvatar);
            OnRegisterLocalPlayer();
        }

        public static void AddPlayerCharacterComponent(Animator _Animator, RootMotionConfig _RootMotionConfig)
        {
            if (KAvatar != null)
                UnregisterPlayerComponent();
 
            KAvatar            = CreateKinetixAvatar(_Animator.avatar, _Animator.transform, EExportType.AnimationClipLegacy);
            LocalKinetixCharacterComponent = AddKCCAndInit(_Animator, KAvatar, _RootMotionConfig);
            OnRegisterLocalPlayer();
        }

        public static void AddPlayerCharacterComponent(Avatar _Avatar, Transform _RootTransform, EExportType _ExportType)
        {
            if (KAvatar != null)
                UnregisterPlayerComponent();
            
            KAvatar = CreateKinetixAvatar(_Avatar, _RootTransform, _ExportType);
            OnRegisterLocalPlayer();
        }

        private static KinetixAvatar CreateKinetixAvatar(Avatar _Avatar, Transform _RootTransform, EExportType _ExportType)
        {
            return new KinetixAvatar()
            {
                Avatar = _Avatar, Root = _RootTransform, ExportType = _ExportType
            };
        }
        
        public static void GetRetargetedAnimationClipLegacy(AnimationIds _AnimationIds, Action<AnimationClip> _OnSuccess, Action _OnFailure)
        {
            EmotesManager.GetAnimationClip(_AnimationIds, KAvatar, SequencerPriority.VeryLow, true, _OnSuccess, _OnFailure);
        }
        
        public static void UnregisterPlayerComponent()
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

            AccountManager.OnUpdatedAccount?.Invoke();
        }

        #region LOAD
        public static void LoadLocalPlayerAnimation(AnimationIds _Ids, string _LockId, Action _OnSuccess = null, Action _OnFailure = null)
        {
            KinetixEmote emote = EmotesManager.GetEmote(_Ids);
            emote.Lock(_LockId);
            
            LoadLocalPlayerAnimationInternal(emote, _OnSuccess, _OnFailure);
        }

        public static void LoadLocalPlayerAnimations(AnimationIds[] _Ids, string _LockId, Action _OnSuccess = null, Action _OnFailure = null)
        {
            KinetixEmote[] kinetixEmotes   = new KinetixEmote[_Ids.Length];
            for (int i = 0; i < kinetixEmotes.Length; i++)
            {
                kinetixEmotes[i] = EmotesManager.GetEmote(_Ids[i]);
                kinetixEmotes[i].Lock(_LockId);
            }

            int toLoadAnimations = kinetixEmotes.Length;
            foreach (KinetixEmote kinetixEmote in kinetixEmotes)
            {
                LoadLocalPlayerAnimationInternal(kinetixEmote, () =>
                {
                    toLoadAnimations--;
                    if (toLoadAnimations == 0)
                        _OnSuccess?.Invoke();
                }, () =>
                {
                    _OnFailure?.Invoke();
                });
            }
        }

        // To be called only in this class in case emote are preloaded with a lock
        private static void LoadLocalPlayerAnimations(AnimationIds[] _Ids, Action _OnSuccess = null, Action _OnFailure = null)
        {
            LoadLocalPlayerAnimations(_Ids, "", _OnSuccess, _OnFailure);
        }

        private static void LoadLocalPlayerAnimationInternal(KinetixEmote _KinetixEmote, Action _OnSuccess, Action _OnFailure)
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
                EmotesManager.LoadAnimation(_KinetixEmote, KAvatar, SequencerPriority.Low, _OnSuccess, _OnFailure);
            }
            catch (Exception e)
            {
                KinetixDebug.LogWarning(e.Message);
            }
        }
        #endregion

        #region UNLOAD
        public static void UnloadLocalPlayerAnimation(AnimationIds _Ids, string _LockId)
        {
            UnlockLocalPlayerAnimation(_Ids, _LockId);
            RemoveLocalPlayerEmotesReadyToPlay(_Ids);
        }

        public static void UnloadLocalPlayerAnimations(AnimationIds[] _Ids, string _LockId = "")
        {
            foreach (AnimationIds ids in _Ids)
            {
                UnlockLocalPlayerAnimation(ids, _LockId);
                RemoveLocalPlayerEmotesReadyToPlay(ids);
            }
        }

        public static void ForceUnloadLocalPlayerAnimations(AnimationIds[] _Ids)
        {
            foreach (AnimationIds ids in _Ids)
            {
                RemoveLocalPlayerEmotesReadyToPlay(ids);
                EmotesManager.ForceUnloadEmote(ids, KAvatar);
            }
        }

        public static void RemoveLocalPlayerEmotesToPreload(AnimationIds[] _Ids)
        {
            emotesToPreload ??= new List<AnimationIds>();
            for (int i = 0; i < _Ids.Length; i++)
            {
                if (emotesToPreload.Contains(_Ids[i]))
                    emotesToPreload.Remove(_Ids[i]);
            }
        }
        
        private static void RemoveLocalPlayerEmotesReadyToPlay(AnimationIds _Ids)
        {
            downloadedEmotesReadyToPlay ??= new List<AnimationIds>();
            if (downloadedEmotesReadyToPlay.Contains(_Ids))
                downloadedEmotesReadyToPlay.Remove(_Ids);
        }
        
        #endregion

        #region LOCKS
        
        public static void LockLocalPlayerAnimation(AnimationIds _Ids, string _LockId)
        {
            EmotesManager.LockEmote(_Ids, _LockId);
        }
        
        public static void LockLocalPlayerAnimations(AnimationIds[] _Ids, string _LockId)
        {
            foreach (AnimationIds ids in _Ids)
            {
                LockLocalPlayerAnimation(ids, _LockId);
            }
        }

        public static void UnlockLocalPlayerAnimation(AnimationIds _Ids, string _LockId)
        {
            EmotesManager.UnlockEmote(_Ids, _LockId, KAvatar);
        }
        
        public static void UnlockLocalPlayerAnimations(AnimationIds[] _Ids, string _LockId)
        {
            foreach (AnimationIds ids in _Ids)
            {
                UnlockLocalPlayerAnimation(ids, _LockId);
            }
        }

        #endregion
        
        public static bool IsAnimationAvailable(AnimationIds _Ids)
        {
            return KAvatar != null && EmotesManager.GetEmote(_Ids).HasAnimationRetargeted(KAvatar);
        }

        public static bool IsEmoteUsedByPlayer(AnimationIds _Ids)
        {
            return KAvatar != null && downloadedEmotesReadyToPlay.Contains(_Ids);
        }

        public static void GetNotifiedOnAnimationReadyOnLocalPlayer(AnimationIds _Ids, Action _OnSucceed)
        {
            try
            {
                if (KAvatar != null)
                {
                    EmotesManager.RegisterAnimationToNotifyOnReady(KAvatar, _Ids, _OnSucceed);
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

        private static KinetixCharacterComponent AddKCCAndInit(Animator _Animator, KinetixAvatar _KinetixAvatar)
        {
            KinetixCharacterComponent kcc = _Animator.gameObject.AddComponent<KinetixCharacterComponent>();
            kcc.Init(_KinetixAvatar);
            kcc.OnAnimationStart += AnimationStartOnLocalPlayerAnimator;
            kcc.OnAnimationEnd   += AnimationEndOnLocalPlayerAnimator;
            return kcc;
        }

        private static KinetixCharacterComponent AddKCCAndInit(Animator _Animator, KinetixAvatar _KinetixAvatar, RootMotionConfig _RootMotionConfig)
        {            
            KinetixCharacterComponent kcc = _Animator.gameObject.AddComponent<KinetixCharacterComponent>();
            kcc.Init(_KinetixAvatar, _RootMotionConfig);
            kcc.OnAnimationStart += AnimationStartOnLocalPlayerAnimator;
            kcc.OnAnimationEnd   += AnimationEndOnLocalPlayerAnimator;
            return kcc;
        }

        private static void AnimationStartOnLocalPlayerAnimator(AnimationIds _AnimationIds)
        {
            OnAnimationStartOnLocalPlayerAnimator?.Invoke(_AnimationIds);
        }

        private static void AnimationEndOnLocalPlayerAnimator(AnimationIds _AnimationIds)
        {
            OnAnimationEndOnLocalPlayerAnimator?.Invoke(_AnimationIds);
        }

        public static AnimationIds[] GetDownloadedAnimationsReadyToPlay()
        {
            return downloadedEmotesReadyToPlay.ToArray();
        }
        
        public static void PlayAnimation(AnimationIds _AnimationsIds, Action<AnimationIds> _OnPlayedAnimation)
        {
            if (LocalKinetixCharacterComponent == null)
            {
                return;
            }

            if (playAutomaticallyOnAnimator)
                LocalKinetixCharacterComponent.PlayAnimation(_AnimationsIds, true, _OnPlayedAnimation);
            else
                _OnPlayedAnimation?.Invoke(_AnimationsIds);
        }

        public static void PlayAnimationQueue(AnimationIds[] _AnimationIdsArray, bool _Loop, Action<AnimationIds[]> _OnPlayedAnimations)
        {
            if (LocalKinetixCharacterComponent == null)
            {
                KinetixDebug.LogWarning("Local player was not registered");
                return;
            }

            if (playAutomaticallyOnAnimator)
                LocalKinetixCharacterComponent.PlayAnimationQueue(_AnimationIdsArray, _Loop, true, _OnPlayedAnimations);
            else
                _OnPlayedAnimations?.Invoke(_AnimationIdsArray);
        }

        public static void StopAnimation()
        {
            if (LocalKinetixCharacterComponent == null)
            {
                KinetixDebug.LogWarning("Local player was not registered");
                return;
            }

            if (playAutomaticallyOnAnimator)
                LocalKinetixCharacterComponent.StopAnimation();
        }
        
        private static void OnRegisterLocalPlayer()
        {
            foreach (KeyValuePair<AnimationIds, List<Action>> kvp in callbackOnRetargetedAnimationIdOnLocalPlayer)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    EmotesManager.RegisterAnimationToNotifyOnReady(KAvatar, kvp.Key, kvp.Value[i]);
                }
            }

            if (emotesToPreload.Count <= 0)
                return;
            
            LoadLocalPlayerAnimations(emotesToPreload.ToArray());
        }


        public static KinetixCharacterComponent GetLocalKCC()
        {
            return LocalKinetixCharacterComponent;
        }
    }
}
