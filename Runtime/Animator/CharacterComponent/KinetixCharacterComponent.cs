// // ----------------------------------------------------------------------------
// // <copyright file="KinetixCharacterComponent.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using UnityEngine;
using Kinetix.Internal;
using System.Collections.Generic;
using System.Linq;

namespace Kinetix
{
	/// <summary>
	/// Kinetix Character Component
	/// </summary>
	public abstract class KinetixCharacterComponent : IDisposable
	{

		/// <summary>
		/// Called when an animation starts playing
		/// </summary>
		public event Action<AnimationIds> OnAnimationStart;
		/// <summary>
		/// Called when an animation stop playing
		/// </summary>
		public event Action<AnimationIds> OnAnimationEnd;
		/// <summary>
		/// Called on each frame of the animation
		/// </summary>
		public event Action OnPlayedFrame;

		/// <summary>
		/// If true, the animation will automaticaly play.<br/>
		/// If false you can handle the animation using the events<br/>
		/// <list type="bullet">
		///		<item>
		///		<term><see cref="OnAnimationStart"/></term>
		///     <description> Animation Start </description>
		///		</item>
		///		<item>
		///		<term><see cref="OnAnimationEnd"/></term>
		///     <description> Animation End </description>
		///		</item>
		///		<item>
		///		<term><see cref="OnPlayedFrame"/></term>
		///     <description> Animation Update </description>
		///		</item>
		/// </list>
		/// </summary>
		public virtual bool AutoPlay { get => _autoPlay; set => _autoPlay = value; }
		private bool _autoPlay = false;

		/// <summary>
		/// Unique identifier of the character
		/// </summary>
		public string GUID => _guid;
		private string _guid;

		internal NetworkPoseSampler networkSampler;
		protected KinetixCharacterComponentBehaviour behaviour;

		// CACHE
		protected KinetixAvatar kinetixAvatar;
		protected readonly List<IPoseInterpreter> poseInerpretor = new List<IPoseInterpreter>();
		protected HumanBodyBones[] characterBones;

		/// <summary>
		/// Init the Character
		/// </summary>
		/// <param name="kinetixAvatar">The avatar to use for the animation</param>
		public void Init(KinetixAvatar kinetixAvatar)
			=> Init(kinetixAvatar, null);

		/// <summary>
		/// Init the Character
		/// </summary>
		/// <param name="kinetixAvatar">The avatar to use for the animation</param>
		/// <param name="rootMotionConfig">Configuration of the root motion</param>
		public virtual void Init(KinetixAvatar kinetixAvatar, RootMotionConfig rootMotionConfig)
		{
			//networkSampler = new NetworkPoseSampler(this, GetComponent<Animator>(), clipSampler);
			behaviour = kinetixAvatar.Root.gameObject.AddComponent<KinetixCharacterComponentBehaviour>();
			behaviour._kcc = this;
			behaviour.OnUpdate += Update;

			this.kinetixAvatar = kinetixAvatar;
			_guid = Guid.NewGuid().ToString();

			AvatarData avatar = kinetixAvatar.Avatar;

			//Get the human bones
			//NOTE: Maybe we can create an extension method from this
            if (avatar.avatar != null)
            {
                characterBones = avatar.avatar.humanDescription.human.Select(h => UnityHumanUtils.HUMANS.IndexOf(h.humanName) >= 0 ? UnityHumanUtils.HUMANS_UNITY[UnityHumanUtils.HUMANS.IndexOf(h.humanName)] : UnityHumanUtils.HUMANS_UNITY[UnityHumanUtils.HUMANS_UNITY.Count - 1]).ToArray();
            }
			else if (avatar.hierarchy != null)
			{
				characterBones = avatar.hierarchy.Where(h => h.m_humanBone.HasValue).Select(h => h.m_humanBone.Value).ToArray();
			}
			else
			{
				throw new ArgumentNullException(nameof(kinetixAvatar)+"."+nameof(kinetixAvatar.Avatar));
			}
			//----//

			networkSampler = new NetworkPoseSampler(characterBones);

		}

		/// <summary>
		/// Update (unity message given by the <see cref="behaviour"/>)
		/// </summary>
		protected virtual void Update() { }

		#region Abstract

		/// <summary>
		/// Check if a pose is available
		/// </summary>
		/// <returns>Return true if a pose is available</returns>
		public abstract bool IsPoseAvailable();

		#endregion

		#region Interpreter
		/// <summary>
		/// Set a pose interpreter as the main interpreter<br/>
		/// (It's the only one that will recieve <see cref="IPoseInterpreter.GetPose"/>)
		/// </summary>
		/// <param name="interpreter"></param>
		public void SetMainPoseInterpreter(IPoseInterpreter interpreter)
		{
			poseInerpretor.Remove(interpreter);
			poseInerpretor.Insert(0, interpreter);
		}

		/// <summary>
		/// Add a pose interpreter to the list of interpreters
		/// </summary>
		/// <param name="interpreter"></param>
		public void RegisterPoseInterpreter(IPoseInterpreter interpreter)
		{
			poseInerpretor.Remove(interpreter);
			poseInerpretor.Add(interpreter);
		}

		/// <summary>
		/// Remove a pose interpreter from the list of interpreters
		/// </summary>
		/// <param name="interpreter"></param>
		public void UnregisterPoseInterpreter(IPoseInterpreter interpreter)
		{
			poseInerpretor.Remove(interpreter);
		}
		#endregion

		#region Call events
		/// <summary>
		/// Call <see cref="OnAnimationStart"/>
		/// </summary>
		protected void Call_OnAnimationStart(AnimationIds ids) => OnAnimationStart?.Invoke(ids);
		/// <summary>
		/// Call <see cref="OnAnimationEnd"/>
		/// </summary>
		protected void Call_OnAnimationEnd(AnimationIds ids) => OnAnimationEnd?.Invoke(ids);
		/// <summary>
		/// Call <see cref="OnPlayedFrame"/>
		/// </summary>
		protected void Call_OnPlayedFrame() => OnPlayedFrame?.Invoke();
		#endregion

		/// <summary>
		/// Dispose the character.
		/// </summary>
		public virtual void Dispose()
		{
			OnAnimationStart = null;
			OnAnimationEnd = null;
			OnPlayedFrame = null;

			if (behaviour != null)
			{
				behaviour._kcc = null;
				UnityEngine.Object.Destroy(behaviour);
			}
		}
	}
}
