// // ----------------------------------------------------------------------------
// // <copyright file="KinetixAnimation.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using Kinetix.Internal.Cache;
using UnityEngine;

namespace Kinetix.Internal
{
	public class KinetixAnimation
	{
		/// <summary>
		/// Event called when Local player is registered
		/// </summary>
		public event Action OnRegisteredLocalPlayer;

		/// <summary>
		/// Event called when animation is played from local player
		/// </summary>
		public event Action<AnimationIds> OnPlayedAnimationLocalPlayer;
		
		/// <summary>
		/// Event called when animation queue is played from local player
		/// </summary>
		public event Action<AnimationIds[]> OnPlayedAnimationQueueLocalPlayer;
		
		/// <summary>
		/// Event called when animation start on animator
		/// </summary>
		/// <param>UUID of the animation</param>
		public event Action<AnimationIds> OnAnimationStartOnLocalPlayerAnimator;

		/// <summary>
		/// Event called when animation end on animator
		/// </summary>
		/// <param>UUID of the animation</param>
		public event Action<AnimationIds> OnAnimationEndOnLocalPlayerAnimator;
		
		/// <summary>
		/// Register the local player animator with avatar setup to play animation on it 
		/// </summary>
		/// <param name="_Animator">Animator of your local character</param>
		public void RegisterLocalPlayerAnimator(Animator _Animator)
		{
			KinetixAnimationBehaviour.RegisterLocalPlayerAnimator(_Animator);
			OnRegisteredLocalPlayer?.Invoke();
		}

		/// <summary>
		/// Register the local player animator with avatar setup to play animation on it 
		/// </summary>
		/// <param name="_Animator">Animator of your local character</param>
		/// <param name="_Config">Configuration for the root motion</param>
		public void RegisterLocalPlayerAnimator(Animator _Animator, RootMotionConfig _Config)
		{
			KinetixAnimationBehaviour.RegisterLocalPlayerAnimator(_Animator, _Config);
			OnRegisteredLocalPlayer?.Invoke();
		}

		/// <summary>
		/// Register the Local Player with a custom hierarchy
		/// </summary>
		/// <param name="_Root">The root of the skeleton's hierarchy. In T pose</param>
		/// <param name="_RootTransform">The root GameObject of your avatar</param>
		/// <param name="_PoseInterpreter">The interpretor to apply poses to your avatar</param>
		public void RegisterLocalPlayerCustom(DataBoneTransform _Root, Transform _RootTransform, IPoseInterpreter _PoseInterpreter)
		{
			KinetixAnimationBehaviour.RegisterLocalPlayerCustom(_Root, _RootTransform, _PoseInterpreter);
			OnRegisteredLocalPlayer?.Invoke();
		}

		/// <summary>
		/// Register the Local Player with a custom hierarchy
		/// </summary>
		/// <param name="_Root">The root of the skeleton's hierarchy. In T pose</param>
		/// <param name="_RootTransform">The root GameObject of your avatar</param>
		/// <param name="_PoseInterpreter">The interpretor to apply poses to your avatar</param>
		/// <param name="_Config">Configuration of the root motion</param>
		public void RegisterLocalPlayerCustom(DataBoneTransform _Root, Transform _RootTransform, IPoseInterpreter _PoseInterpreter, RootMotionConfig _Config)
		{
			KinetixAnimationBehaviour.RegisterLocalPlayerCustom(_Root, _RootTransform, _PoseInterpreter, _Config);
			OnRegisteredLocalPlayer?.Invoke();
		}

		/// <summary>
		/// Register the local player configuration for custom animation system.
		/// </summary>
		/// <param name="_Avatar">Avatar of your character</param>
		/// <param name="_RootTransform">Root Transform of your character</param>
		/// <param name="_ExportType">The type of file for animations to export</param>
		public void RegisterLocalPlayerCustom(Avatar _Avatar, Transform _RootTransform, EExportType _ExportType)
		{
			KinetixAnimationBehaviour.RegisterLocalPlayerCustom(_Avatar, _RootTransform, _ExportType);
			OnRegisteredLocalPlayer?.Invoke();
		}
		
		/// <summary>
		/// Unregister the local player animator.
		/// </summary>
		public void UnregisterLocalPlayer()
		{
			KinetixAnimationBehaviour.UnregisterLocalPlayer();
		}
		
		/// <summary>
		/// Play animation on local player
		/// </summary>
		/// <param name="_AnimationIds">IDs of the animation</param>
		public void PlayAnimationOnLocalPlayer(AnimationIds _AnimationIds)
		{
			KinetixAnimationBehaviour.PlayAnimationOnLocalPlayer(_AnimationIds, OnPlayedAnimationLocalPlayer);
		}
		
		/// <summary>
		/// Play animations on local player
		/// </summary>
		/// <param name="_Ids">IDs of the animations</param>
		/// <param name="_Loop">Loop the queue</param>
		public void PlayAnimationQueueOnLocalPlayer(AnimationIds[] _Ids, bool _Loop = false)
		{
			KinetixAnimationBehaviour.PlayAnimationQueueOnLocalPlayer(_Ids, _Loop, OnPlayedAnimationQueueLocalPlayer);
		}
		
		/// <summary>
		/// Get Retargeted KinetixClip for local player
		/// </summary>
		/// <param name="_AnimationIds">IDs of the animation</param>
		/// <param name="_OnSuccess">Callback on Success providing KinetixClip Legacy</param>
		/// <param name="_OnFailure">Callback on Failure</param>
		public void GetRetargetedKinetixClipForLocalPlayer(AnimationIds _AnimationIds, Action<KinetixClip> _OnSuccess, Action _OnFailure = null)
		{
			KinetixAnimationBehaviour.GetRetargetedKinetixClipOnLocalPlayer(_AnimationIds, _OnSuccess, _OnFailure);
		}

		/// <summary>
		/// Get Retargeted AnimationClip Legacy for local player
		/// </summary>
		/// <remarks>
		/// The animation clip memory isn't unhandeled by the sdk. You have to call the <see cref="UnityEngine.Object.Destroy"/> after using it.
		/// </remarks>
		/// <param name="_AnimationIds">IDs of the animation</param>
		/// <param name="_OnSuccess">Callback on Success providing KinetixClip Legacy</param>
		/// <param name="_OnFailure">Callback on Failure</param>
		public void GetRetargetedAnimationClipLegacyForLocalPlayer(AnimationIds _AnimationIds, Action<AnimationClip> _OnSuccess, Action _OnFailure = null)
		{
			KinetixAnimationBehaviour.GetRetargetedAnimationClipLegacyOnLocalPlayer(_AnimationIds, _OnSuccess, _OnFailure);
		}

		/// <summary>
		/// Stop animation on local player
		/// </summary>
		public void StopAnimationOnLocalPlayer()
		{
			KinetixAnimationBehaviour.StopAnimationOnLocalPlayer();
		}

		/// <summary>
		/// Load a local player animation
		/// </summary>
		/// <param name="_AnimationIds">IDs of the animation</param>
		/// <param name="_LockId">Arbitrary Identifier of the component asking for the animation, to pass when unloading the animation</param>
		/// <param name="_OnSuccess">Callback when successfully loaded animation</param>
		public void LoadLocalPlayerAnimation(AnimationIds _AnimationIds, string _LockId, Action _OnSuccess = null)
		{
			KinetixAnimationBehaviour.LoadLocalPlayerAnimation(_AnimationIds, _LockId, _OnSuccess);
		}
		
		/// <summary>
		/// Load local player animations
		/// </summary>
		/// <param name="_AnimationIds">IDs of the animations</param>
		/// <param name="_LockId">Arbitrary Identifier of the component asking for the animation, to pass when unloading the animation</param>
		/// <param name="_OnSuccess">Callback when successfully loaded animations</param>
		public void LoadLocalPlayerAnimations(AnimationIds[] _AnimationIds, string _LockId, Action _OnSuccess = null)
		{
			KinetixAnimationBehaviour.LoadLocalPlayerAnimations(_AnimationIds, _LockId, _OnSuccess);
		}
		
		/// <summary>
		/// Unload a local player animation
		/// </summary>
		/// <param name="_AnimationIds">IDs of the animation</param>
		/// <param name="_LockId">Arbitrary Identifier of the component asking for the animation, to pass when unloading the animation</param>
		public void UnloadLocalPlayerAnimation(AnimationIds _AnimationIds, string _LockId)
		{
			KinetixAnimationBehaviour.UnloadLocalPlayerAnimation(_AnimationIds, _LockId);
		}
		
		/// <summary>
		/// Unload local player animations
		/// </summary>
		/// <param name="_AnimationIds">IDs of the animations</param>
		/// <param name="_LockId">Arbitrary Identifier of the component asking for the animation, to pass when unloading the animation</param>
		public void UnloadLocalPlayerAnimations(AnimationIds[] _AnimationIds, string _LockId)
		{
			KinetixAnimationBehaviour.UnloadLocalPlayerAnimations(_AnimationIds, _LockId);
		}

		/// <summary>
		/// Lock a local player animation, preventing its removal from memory
		/// </summary>
		/// <param name="_AnimationIds">IDs of the animations</param>
		/// <param name="_LockId">Arbitrary Identifier of the component asking for the animation, to pass when unloading the animation</param>
		public void LockLocalPlayerAnimation(AnimationIds _Ids, string _LockId)
        {
            KinetixAnimationBehaviour.LockLocalPlayerAnimation(_Ids, _LockId);
        }
        
		/// <summary>
		/// Lock all local player animations, preventing their removal from memory
		/// </summary>
		/// <param name="_AnimationIds">IDs of the animations</param>
		/// <param name="_LockId">Arbitrary Identifier of the component asking for the animation, to pass when unloading the animation</param>
        public void LockLocalPlayerAnimations(AnimationIds[] _Ids, string _LockId)
        {
            KinetixAnimationBehaviour.LockLocalPlayerAnimations(_Ids, _LockId);
        }

		/// <summary>
		/// Remove a lock on a local player animation, allowing its removal from memory if all locks are removed
		/// </summary>
		/// <param name="_AnimationIds">IDs of the animations</param>
		/// <param name="_LockId">Arbitrary Identifier of the component asking for the animation, to pass when unloading the animation</param>
        public void UnlockLocalPlayerAnimation(AnimationIds _Ids, string _LockId)
        {
            KinetixAnimationBehaviour.UnlockLocalPlayerAnimation(_Ids, _LockId);
        }
        
		/// <summary>
		/// Remove a lock on all player animations, allowing their removal from memory if all of their locks are removed
		/// </summary>
		/// <param name="_AnimationIds">IDs of the animations</param>
		/// <param name="_LockId">Arbitrary Identifier of the component asking for the animation, to pass when unloading the animation</param>
        public void UnlockLocalPlayerAnimations(AnimationIds[] _Ids, string _LockId)
        {
            KinetixAnimationBehaviour.UnlockLocalPlayerAnimations(_Ids, _LockId);
        }

		/// <summary>
		/// Is animation available on local player
		/// </summary>
		/// <param name="_AnimationIds">IDs of the animation</param>
		/// <returns>True if animation available on local player</returns>
		public bool IsAnimationAvailableOnLocalPlayer(AnimationIds _AnimationIds)
		{
			return KinetixAnimationBehaviour.IsAnimationAvailableOnLocalPlayer(_AnimationIds);
		}

		/// <summary>
		/// Get notified when an animation is ready on local player
		/// </summary>
		/// <param name="_Ids">IDs of the animation</param>
		/// <param name="_OnSuccess">Callback on animation ready</param>
		public void GetNotifiedOnAnimationReadyOnLocalPlayer(AnimationIds _Ids, Action _OnSuccess)
		{
			KinetixAnimationBehaviour.GetNotifiedOnAnimationReadyOnLocalPlayer(_Ids, _OnSuccess);
		}

		public KinetixCharacterComponentLocal GetLocalKCC()
		{
			return KinetixAnimationBehaviour.GetLocalKCC();
		}

        public bool IsLocalPlayerRegistered()
        {
            return KinetixAnimationBehaviour.IsLocalPlayerRegistered();
        }

		#region Internal

		public KinetixAnimation()
		{
			KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().OnAnimationStartOnLocalPlayerAnimator += AnimationStartOnLocalPlayerAnimator;
			KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().OnAnimationEndOnLocalPlayerAnimator   += AnimationEndOnLocalPlayerAnimator;
		}

		private void AnimationStartOnLocalPlayerAnimator(AnimationIds _AnimationIds)
		{
			OnAnimationStartOnLocalPlayerAnimator?.Invoke(_AnimationIds);
		}

		private void AnimationEndOnLocalPlayerAnimator(AnimationIds _AnimationIds)
		{
			OnAnimationEndOnLocalPlayerAnimator?.Invoke(_AnimationIds);
		}

		#endregion
	}
}
