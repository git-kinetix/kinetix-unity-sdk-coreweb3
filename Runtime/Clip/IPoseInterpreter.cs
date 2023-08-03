// // ----------------------------------------------------------------------------
// // <copyright file="IPoseInterpreter.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;

namespace Kinetix
{
    public enum HumanSpecialBones
	{
		Root,
		Armature
	}

	/// <summary>
	/// A pose interpreter is a class that can apply human poses to an Avatar.
	/// </summary>
	public interface IPoseInterpreter
	{
		/// <summary>
		/// This method is called when the sampler wants to know what is the armature path
		/// </summary>
		/// <returns>The armature path or null if there is no armature</returns>
		public string GetArmature();

		/// <summary>
		/// This method is called when the sampler wants to know the position of your system (outside of the animation) usually for blending with your own animator</br>
		/// Only the main pose interpreter receives this event
		/// </summary>
		/// <returns></returns>
		public KinetixPose GetPose();

		/// <summary>
		/// This method is called when a blendshape needs to be updated
		/// </summary>
		/// <param name="bone"></param>
		/// <param name="pose"></param>
		public void ApplyBlendshape(ARKitBlendshapes blendshape, float pose);

		/// <summary>
		/// This method is called when a some special bone (Root / Armature etc...) needs to be updated
		/// </summary>
		/// <param name="bone"></param>
		/// <param name="pose"></param>
		public void ApplyOther(HumanSpecialBones bone, TransformData pose);

		/// <summary>
		/// This method is called when a human bone (Head / Foot etc...) needs to be updated
		/// </summary>
		/// <param name="bone"></param>
		/// <param name="pose"></param>
		public void ApplyBone(HumanBodyBones bone, TransformData pose);

		/// <summary>
		/// This method is called when a non bone ("Armature" for example) needs to be updated.<br/>
		/// <br/>
		/// Example use case: The animator sets the an Armature to (-90, 0, 0) because the avatar's up is globally oriented on z in his T-Pose.<br/>
		/// To avoid having our animation oriented on Z we set the Armature to (0, 0, 0)
		/// </summary>
		/// <param name="bonePath"></param>
		/// <param name="pose"></param>
		public void ApplyResetPose(string bonePath, TransformData pose);
	}

	/// <summary>
	/// An event to bind on the start and the end event of the animation
	/// </summary>
	public interface IPoseInterpreterStartEnd : IPoseInterpreter
	{
		/// <param name="clip">Clip is null on remote</param>
		public void AnimationStart(KinetixClip clip);
		/// <param name="clip">Clip is null on remote</param>
		public void AnimationEnd(KinetixClip clip);
		public void QueueStart();
		public void QueueEnd();
	}
}
