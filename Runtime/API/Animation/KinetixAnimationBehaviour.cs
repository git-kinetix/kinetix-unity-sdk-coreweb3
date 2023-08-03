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
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().AddPlayerCharacterComponent(_Animator);
        }

        /// <summary>
        /// Register the Local Player Animator
        /// </summary>
        /// <param name="_Animator">Animator of the Local Player</param>
        /// <param name="_Config">Configuration of the root motion</param>
        public static void RegisterLocalPlayerAnimator(Animator _Animator, RootMotionConfig _Config)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().AddPlayerCharacterComponent(_Animator, _Config);
        }

        /// <summary>
        /// Register the Local Player with a custom hierarchy
        /// </summary>
        /// <param name="_Root">The root of the skeleton's hierarchy. In T pose</param>
        /// <param name="_RootTransform">The root GameObject of your avatar</param>
        /// <param name="_PoseInterpreter">The interpretor to apply poses to your avatar</param>
        public static void RegisterLocalPlayerCustom(DataBoneTransform _Root, Transform _RootTransform, IPoseInterpreter _PoseInterpreter)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().AddPlayerCharacterComponent(_Root, _RootTransform, _PoseInterpreter);
        }

		/// <summary>
		/// Register the Local Player with a custom hierarchy
		/// </summary>
		/// <param name="_Root">The root of the skeleton's hierarchy. In T pose</param>
		/// <param name="_RootTransform">The root GameObject of your avatar</param>
		/// <param name="_PoseInterpreter">The interpretor to apply poses to your avatar</param>
		/// <param name="_Config">Configuration of the root motion</param>
		public static void RegisterLocalPlayerCustom(DataBoneTransform _Root, Transform _RootTransform, IPoseInterpreter _PoseInterpreter, RootMotionConfig _Config)
		{
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().AddPlayerCharacterComponent(_Root, _RootTransform, _PoseInterpreter, _Config);
        }

        /// <summary>
        /// Register the Local Player Avatar and Root Transform
        /// </summary>
        /// <param name="_Avatar">Avatar of the Local Player</param>
        /// <param name="_RootTransform">Root Transform of the Local Player</param>
        /// <param name="_ExportType">Type of Export File</param>
        public static void RegisterLocalPlayerCustom(Avatar _Avatar, Transform _RootTransform, EExportType _ExportType)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().AddPlayerCharacterComponent(_Avatar, _RootTransform, _ExportType);
        }

        /// <summary>
        /// Unregister the local player
        /// </summary>
        public static void UnregisterLocalPlayer()
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().UnregisterPlayerComponent();
        }

        public static void PlayAnimationOnLocalPlayer(AnimationIds _Ids, Action<AnimationIds> _OnPlayedAnimation)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().PlayAnimation(_Ids, _OnPlayedAnimation);
        }
        
        public static void PlayAnimationQueueOnLocalPlayer(AnimationIds[] _Ids, bool _Loop = false, Action<AnimationIds[]> _OnPlayedAnimation = null)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().PlayAnimationQueue(_Ids, _Loop, _OnPlayedAnimation);
        }
        
        public static void GetRetargetedKinetixClipOnLocalPlayer(AnimationIds _AnimationIds, Action<KinetixClip> _OnSuccess, Action _OnFailure)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().GetRetargetedKinetixClipLegacy(_AnimationIds, _OnSuccess, _OnFailure);
        }
        
        public static void GetRetargetedAnimationClipLegacyOnLocalPlayer(AnimationIds _AnimationIds, Action<AnimationClip> _OnSuccess, Action _OnFailure)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().GetRetargetedAnimationClipLegacy(_AnimationIds, _OnSuccess, _OnFailure);
        }

        public static void StopAnimationOnLocalPlayer()
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().StopAnimation();
        }

        public static void LoadLocalPlayerAnimation(AnimationIds _Ids, string _LockId, Action _OnSuccess)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().LoadLocalPlayerAnimation(_Ids, _LockId, _OnSuccess);
        }
        
        public static void LoadLocalPlayerAnimations(AnimationIds[] _Ids, string _LockId, Action _OnSuccess)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().LoadLocalPlayerAnimations(_Ids, _LockId, _OnSuccess);
        }
        
        public static void UnloadLocalPlayerAnimation(AnimationIds _Ids, string _LockId)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().UnloadLocalPlayerAnimation(_Ids, _LockId);
        }
        
        public static void UnloadLocalPlayerAnimations(AnimationIds[] _Ids, string _LockId)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().UnloadLocalPlayerAnimations(_Ids, _LockId);
        }

        public static void LockLocalPlayerAnimation(AnimationIds _Ids, string _LockId)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().LockLocalPlayerAnimation(_Ids, _LockId);
        }
        
        public static void LockLocalPlayerAnimations(AnimationIds[] _Ids, string _LockId)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().LockLocalPlayerAnimations(_Ids, _LockId);
        }

        public static void UnlockLocalPlayerAnimation(AnimationIds _Ids, string _LockId)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().UnlockLocalPlayerAnimation(_Ids, _LockId);
        }
        
        public static void UnlockLocalPlayerAnimations(AnimationIds[] _Ids, string _LockId)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().UnlockLocalPlayerAnimations(_Ids, _LockId);
        }

        public static bool IsAnimationAvailableOnLocalPlayer(AnimationIds _Ids)
        { 
            return KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().IsAnimationAvailable(_Ids);
        }

        public static void GetNotifiedOnAnimationReadyOnLocalPlayer(AnimationIds _Ids, Action _OnSucceed)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().GetNotifiedOnAnimationReadyOnLocalPlayer(_Ids, _OnSucceed);
        }

        public static KinetixCharacterComponentLocal GetLocalKCC()
        {
            return KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().GetLocalKCC();
        }
    }
}
