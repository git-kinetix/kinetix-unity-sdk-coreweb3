// // ----------------------------------------------------------------------------
// // <copyright file="KinetixAnimationBehaviour.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using Kinetix.Internal.Cache;
using UnityEngine;

namespace Kinetix.Internal
{
    internal static class KinetixAnimationBehaviour
    {
        /// <summary>
        /// Register the Local Player Animator
        /// </summary>
        /// <param name="_Animator">Animator of the Local Player</param>
        public static void RegisterLocalPlayerAnimator(Animator _Animator)
        {
            LocalPlayerManager.AddPlayerCharacterComponent(_Animator);
        }

        /// <summary>
        /// Register the Local Player Animator
        /// </summary>
        /// <param name="_Animator">Animator of the Local Player</param>
        /// <param name="_Config">Configuration of the root motion</param>
        public static void RegisterLocalPlayerAnimator(Animator _Animator, RootMotionConfig _Config)
        {
            LocalPlayerManager.AddPlayerCharacterComponent(_Animator, _Config);
        }

        /// <summary>
        /// Register the Local Player Avatar and Root Transform
        /// </summary>
        /// <param name="_Avatar">Avatar of the Local Player</param>
        /// <param name="_RootTransform">Root Transform of the Local Player</param>
        /// <param name="_ExportType">Type of Export File</param>
        public static void RegisterLocalPlayerCustom(Avatar _Avatar, Transform _RootTransform, EExportType _ExportType)
        {
            LocalPlayerManager.AddPlayerCharacterComponent(_Avatar, _RootTransform, _ExportType);
        }

        /// <summary>
        /// Unregister the local player
        /// </summary>
        public static void UnregisterLocalPlayer()
        {
            LocalPlayerManager.RemovePlayerCharacterComponent();
        }

        public static void PlayAnimationOnLocalPlayer(AnimationIds _Ids, Action<AnimationIds> _OnPlayedAnimation)
        {
            LocalPlayerManager.PlayAnimation(_Ids, _OnPlayedAnimation);
        }
        
        public static void PlayAnimationQueueOnLocalPlayer(AnimationIds[] _Ids, bool _Loop = false, Action<AnimationIds[]> _OnPlayedAnimation = null)
        {
            LocalPlayerManager.PlayAnimationQueue(_Ids, _Loop, _OnPlayedAnimation);
        }
        
        public static void GetRetargetedAnimationClipLegacyOnLocalPlayer(AnimationIds _AnimationIds, Action<AnimationClip> _OnSuccess, Action _OnFailure)
        {
            LocalPlayerManager.GetRetargetedAnimationClipLegacy(_AnimationIds, _OnSuccess, _OnFailure);
        }

        public static void StopAnimationOnLocalPlayer()
        {
            LocalPlayerManager.StopAnimation();
        }

        public static void LoadLocalPlayerAnimation(AnimationIds _Ids, Action _OnSuccess)
        {
            LocalPlayerManager.LoadLocalPlayerAnimation(_Ids, _OnSuccess);
        }
        
        public static void LoadLocalPlayerAnimations(AnimationIds[] _Ids, Action _OnSuccess)
        {
            LocalPlayerManager.LoadLocalPlayerAnimations(_Ids, _OnSuccess);
        }
        
        public static void UnloadLocalPlayerAnimation(AnimationIds _Ids)
        {
            LocalPlayerManager.UnloadLocalPlayerAnimation(_Ids);
        }
        
        public static void UnloadLocalPlayerAnimations(AnimationIds[] _Ids)
        {
            LocalPlayerManager.UnloadLocalPlayerAnimations(_Ids);
        }

        public static bool IsAnimationAvailableOnLocalPlayer(AnimationIds _Ids)
        { 
            return LocalPlayerManager.IsAnimationAvailable(_Ids);
        }

        public static void GetNotifiedOnAnimationReadyOnLocalPlayer(AnimationIds _Ids, Action _OnSucceed)
        {
            LocalPlayerManager.GetNotifiedOnAnimationReadyOnLocalPlayer(_Ids, _OnSucceed);
        }

        public static KinetixCharacterComponent GetLocalKCC()
        {
            return LocalPlayerManager.GetLocalKCC();
        }
    }
}
