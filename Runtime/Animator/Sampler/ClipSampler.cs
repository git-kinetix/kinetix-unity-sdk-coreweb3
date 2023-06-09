using System;
using UnityEngine;

namespace Kinetix.Internal
{
	public enum BlendState { NONE, IN, SAMPLE, OUT, COUNT };

	[RequireComponent(typeof(Animator))]
	public class ClipSampler : MonoBehaviour
	{
#if UNITY_EDITOR && DEV_KINETIX
		private readonly Color[] m_blendDebugColors = new Color[] { new Color(0.6f, 0.6f, 0.6f), Color.green, Color.yellow, Color.red, Color.magenta };
#endif

		const string LOCK_SAMPLER_ID = "ClipSampler";

		public Action OnStop;

		private AnimationClip			m_toPlay;
		private AnimationQueue			m_animQueue;
		private Animator				m_animatorToPause;
		private BlendAnimation			m_blendAnimations;
		private float					m_duration;
		private float					m_time;
		private int						m_animIndex;
		private bool					_m_animatorWasEnable;
		private BlendState				m_blendState = BlendState.NONE;
		internal BlendState BlendState
		{
#if UNITY_EDITOR && DEV_KINETIX && !DEBUG_AVATAR
			get => m_blendState;
			private set
			{
				KinetixLogger.Log<ClipSampler>($"m_BlendState: {m_blendState} -> {value}", LogKind.Debug, true);
				m_blendState = value;

				if (m_blendAnimations)
					m_blendAnimations.m_colorRigClone = m_blendDebugColors[(int)value];
			}
#else
			get => m_blendState;
			private set => m_blendState = value;
#endif
		}
		private AnimatorStateInfo[]		m_animatorStatesInfos;
		private float					m_blendTime;						
		private const float				m_blendInTime = 0.25f;
		private const float				m_blendOutTime = 0.25f;
		private const float				m_blendInterTime = 0.25f;

		private AnimationIds lastAnimationPlayed;

		public Action<AnimationIds> OnAnimationStart;
		public Action<AnimationIds> OnAnimationEnd;

		public bool m_animatorWasEnable
		{
			get => _m_animatorWasEnable;
			set => _m_animatorWasEnable = value;
		}

		private bool IsPlaying => m_time < (m_duration - m_blendTime) && m_time >= 0;

		private int ClipsCount => m_animQueue.m_animationsClips.Length;

		public RootMotionConfig RootMotionConfig { get { return rootMotionUtil.Config; } set { rootMotionUtil.Config = value; } }
		private RootMotionUtil rootMotionUtil;

		private void Start()
		{
			Init();
		}

		public void Init()
		{
			if (m_blendAnimations == null) {
				m_blendAnimations = gameObject.AddComponent<BlendAnimation>();
				m_blendAnimations.Init();
			}

			if (rootMotionUtil == null) {
				rootMotionUtil = new RootMotionUtil(m_blendAnimations.GetHips(transform), transform, new RootMotionConfig(false, false, false));
			}
		}


        public void Dispose()
        {
            if (m_animatorToPause != null)
                m_animatorToPause.enabled = true;
            if (m_blendAnimations != null)
                m_blendAnimations.Dispose();
            EnableAnimator();
        }

		private void GetAnimator()
		{
			if (!m_animatorToPause) m_animatorToPause = GetComponent<Animator>();
		}

		public void Play(AnimationClip _AnimationClip, AnimationIds _AnimationIds = null)
		{
            AnimationQueue animationQueue = new AnimationQueue(new []{_AnimationIds}, false)
            {
                m_animationsClips = new AnimationClip[] { _AnimationClip }
            };
            Play(animationQueue);
		}

		public void Play(AnimationQueue _AnimationsQueue)
		{
            if (m_animQueue != null)
                OnAnimationEnd?.Invoke(m_animQueue.m_animationsIds[m_animIndex]); 
            
			if (m_animQueue != null)
				EnableAnimator();
            
			if (_AnimationsQueue.m_animationsClips.Length > 0)
			{

				m_animQueue = _AnimationsQueue;
				m_animIndex = 0;
				StartAnimation();
				BlendTime();
			}
		}

		/// <summary>
		/// Start Blend In
		/// Disable Animator and store states 
		/// </summary>
		/// <param name="_AnimationClip">Animation to blend in to </param>
		private void StartAnimation()
		{
			if (m_toPlay != null)
				EnableAnimator();

			if (!m_animQueue.m_animationsClips[m_animIndex])
				return;

			m_toPlay = m_animQueue.m_animationsClips[m_animIndex];
			float animLength = m_toPlay.length;
			if (!enabled || animLength == 0) return;

			GetAnimator();

			if (m_duration != 0) {
				rootMotionUtil.RevertToOffsets();
			}

			DisableAnimator();

			m_time = m_blendInTime;
			m_duration = animLength;

			m_blendState = BlendState.IN;
			
			
			rootMotionUtil.SaveOffsets();

			//If animation duration is too short, reduce blend IN time
			float blendDuration = m_duration >= m_blendInTime ? m_blendInTime : m_duration;

            if (m_animQueue.m_animationsIds.Length >= m_animIndex && m_animQueue.m_animationsIds[m_animIndex] != null)
                OnAnimationStart?.Invoke(m_animQueue.m_animationsIds[m_animIndex]);
			m_blendAnimations.BlendAnim(m_toPlay, blendDuration, blendDuration);
			m_blendAnimations.OnBlendEnded += OnBlendInEnded;
		}

		/// <summary>
		/// Action done when blend in end
		/// </summary>
		private void OnBlendInEnded()
		{
			m_blendAnimations.OnBlendEnded -= OnBlendInEnded;

			if (m_blendState == BlendState.IN) {
				KinetixCore.Animation.LockLocalPlayerAnimation(m_animQueue.m_animationsIds[m_animIndex], LOCK_SAMPLER_ID);
				
				m_blendState = BlendState.SAMPLE;

				rootMotionUtil.SaveOffsets();
			}
		}

		/// <summary>
		/// Last blend of the queue before enable animator controller
		/// </summary>
		private void OnBlendOutEnded()
		{
			if (m_blendState == BlendState.OUT) {
				m_blendAnimations.OnBlendEnded -= OnBlendOutEnded;
				m_duration = 0;
				EnableAnimator();
			}
		}

		/// <summary>
		/// Blend between 2 legacy animation
		/// </summary>
		private void OnBlendInterEnded()
		{
			if (m_blendState == BlendState.IN || m_blendState == BlendState.OUT) {
				m_blendAnimations.OnBlendEnded -= OnBlendInterEnded;
				m_time = 0f;
				m_blendState = BlendState.SAMPLE;

				if (RootMotionConfig.BakeIntoPoseXZ) {
					rootMotionUtil.SaveOffsets();
				}
			}
		}

		/// <summary>
		/// Define blend time 
		/// amount of time to anticipate before end of anim
		/// </summary>
		private void BlendTime()
		{
			if (m_animQueue.m_loop)
				m_blendTime = m_blendInterTime;
			//last animation of Queue
			else if (m_animIndex == ClipsCount - 1)
				m_blendTime = m_blendOutTime;
			else
				m_blendTime = m_blendInterTime;
		}

		/// <summary>
		/// Start Blend out
		/// </summary>
		public void Stop()
		{
			if (!m_blendAnimations || !m_animatorToPause || m_animatorStatesInfos == null) return;

			// Already stopping, no need to call it again
			if (m_blendState == BlendState.NONE || m_blendState == BlendState.OUT) return;

			ForceBlendOut();
		}

		public void ForceBlendOut()
		{
			GetAnimator();

			// Revert to original state
			rootMotionUtil.RevertToOffsets();
			
			m_blendState = BlendState.OUT;
			m_blendAnimations.BlendAnim(m_animatorToPause.runtimeAnimatorController, m_animatorStatesInfos, m_blendOutTime);
			m_blendAnimations.OnBlendEnded += OnBlendOutEnded;
		}

		/// <summary>
		/// Enable animator and reapply animator controller states 
		/// </summary>
		private void EnableAnimator()
		{
			if (m_toPlay == null)
				return;

			GetAnimator();

			//Re-enable animator
			m_animatorToPause.enabled = m_animatorWasEnable;
			int lc = m_animatorToPause.layerCount;
			//Put the animator back to its state
			for (int i = 0; i < lc; i++)
			{
				m_animatorToPause.Play(m_animatorStatesInfos[i].fullPathHash, i, m_animatorStatesInfos[i].normalizedTime);
			}

			KinetixCore.Animation.UnlockLocalPlayerAnimation(m_animQueue.m_animationsIds[m_animIndex], LOCK_SAMPLER_ID);
			
			m_blendState = BlendState.NONE;
			m_time = -1;
			m_toPlay = null;
			m_animQueue = null;
			OnStop?.Invoke();
		}

		/// <summary>
		/// Disable animator and store animator controller states 
		/// </summary>
		public void DisableAnimator()
		{
			GetAnimator();
			
			//Pre init animator to restore its state
			int lc = m_animatorToPause.layerCount;
			m_animatorStatesInfos = new AnimatorStateInfo[lc];
			for (int i = 0; i < lc; i++)
			{
				m_animatorStatesInfos[i] = m_animatorToPause.GetCurrentAnimatorStateInfo(i);
			}

			//Pause animator
			m_animatorWasEnable = m_animatorToPause.enabled;
			m_animatorToPause.enabled = false;
		}

		private void Update()
		{
			if (m_blendState == BlendState.SAMPLE && m_toPlay != null)
			{
				if (IsPlaying)
				{
					m_toPlay.SampleAnimation(gameObject, m_time);
					m_time += Time.deltaTime;

					// Apply root motion if wanted (apply hips movements to the root: the object with the KCC)
					rootMotionUtil.ProcessRootMotionAfterAnimSampling();
				}
				else
				{
					// Revert to original state
					rootMotionUtil.RevertToOffsets();

					OnAnimationEnd?.Invoke(m_animQueue.m_animationsIds[m_animIndex]);
					m_blendState = BlendState.OUT;
					NextAnimation();
				}
			}
		}

		/// <summary>
		/// Determine which animation blend to,
		/// blend to animator controller if queue ended, blend to next legacy animation otherwise
		/// </summary>
		private void NextAnimation()
		{
			if (m_animIndex < ClipsCount - 1)
			{
				//Set next anim to sample and next blendOut time
				m_toPlay = m_animQueue.m_animationsClips[++m_animIndex];
				m_duration = m_toPlay.length;
				BlendTime();

				m_blendAnimations.BlendAnim(m_toPlay, m_blendInterTime);
				m_blendAnimations.OnBlendEnded += OnBlendInterEnded;
			}
			else
			{
				if (m_animQueue.m_loop)
				{
					//Loop Queue, reset to index 0
					m_toPlay = m_animQueue.m_animationsClips[0];
					m_animIndex = 0;
					m_duration = m_toPlay.length;

					m_blendAnimations.BlendAnim(m_toPlay, m_blendInterTime);
					m_blendAnimations.OnBlendEnded += OnBlendInterEnded;
				}
				else
				{
					m_blendAnimations.BlendAnim(m_animatorToPause.runtimeAnimatorController, m_animatorStatesInfos, m_blendOutTime);
					m_blendAnimations.OnBlendEnded += OnBlendOutEnded;
				}
			}
        }
	}
}
