// // ----------------------------------------------------------------------------
// // <copyright file="AnimatorPoseInterpetor.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Kinetix.Internal
{
	/// <summary>
	/// A pose interpreter for Avatars using unity's Animator
	/// </summary>
	public class AnimatorPoseInterpetor : TransformPoseInterpreter
	{
		private readonly Animator animator;
		private bool wasOn;
		private bool keepAnimatorState;

        public AnimatorPoseInterpetor(Animator animator, Avatar avatar, SkinnedMeshRenderer[] skinnedMeshRenderer = null) : base(animator.gameObject, avatar, skinnedMeshRenderer)
		{
			this.animator = animator;
		}

		public override void AnimationStart(KinetixClip clip)
		{
			//Prevent the "animator.enabled" from external modification during interpretation
			animator.enabled = false;
			animator.keepAnimatorControllerStateOnDisable = true;
			base.AnimationStart(clip);
		}

		public override void AnimationEnd(KinetixClip clip)
		{
			base.AnimationStart(clip);
		}

		public override void QueueStart()
		{
			keepAnimatorState = animator.keepAnimatorControllerStateOnDisable;
			animator.keepAnimatorControllerStateOnDisable = true;

			wasOn = animator.enabled;
			animator.enabled = false;

			base.QueueStart();
		}

		public override void QueueEnd()
		{
			animator.enabled = wasOn;
			animator.keepAnimatorControllerStateOnDisable = keepAnimatorState;

			base.QueueEnd();

		}

        public override KinetixPose GetPose()
        {
			bool wasOn = animator.enabled;
			animator.enabled = false;
			animator.Update(Time.deltaTime);
            KinetixPose toReturn = base.GetPose();
			animator.enabled = wasOn;

			clip?.ApplyResetPos(this);

			return toReturn;
        }
    }
}
