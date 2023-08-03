// // ----------------------------------------------------------------------------
// // <copyright file="SimulationSampler.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using Kinetix.Internal.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kinetix.Internal
{
	public partial class SimulationSampler : IDisposable
	{
		// ====================================================================== //
		//  EVENTS                                                                //
		// ====================================================================== //
		public event Action                                      OnQueueStart             ;
		public event Action                                      OnQueueStop              ;
		public event Action<KinetixClipWrapper>                  OnAnimationStart         ;
		public event Action<KinetixClipWrapper>                  OnAnimationStop          ;
		public event Action<KinetixFrame>                        OnPlayedFrame            ;
		public event Action<KinetixFrame>                        RequestAdaptToInterpreter;
		public event SamplerAuthorityBridge.GetAvatarPosDelegate RequestAvatarPos         ;
		internal event SamplerAuthorityBridge.GetAvatarDelegate  RequestAvatar            ;
		// ---------------------------------------------------------------------- //

		// ====================================================================== //
		//  FIELDS AND PROPERTIES                                                 //
		// ====================================================================== //
		#region FIELDS AND PROPERTIES

		//  Effect Sampler
		// ----------------- //
		private readonly EffectSampler effect;
		public EffectSampler Effect => effect;

		/// <summary>
		/// Instance Singleton shared between every effects
		/// </summary>
		private readonly SamplerAuthorityBridge authorityBridge;


		//  Queue and play
		// ----------------- //
		private Queue<KinetixClipWrapper> queue = new Queue<KinetixClipWrapper>();
		private bool playEnabled = false;
		private float? softStopTime = null;
		public KinetixClipWrapper MainClip    => (Main?.Clip).GetValueOrDefault();
		public double             ElapsedTime => (Main?.ElapsedTime).GetValueOrDefault();

		//  Cache
		// ----------------- //
		internal HumanBodyBones[] bones;

		//  Samplers
		// ----------------- //
		private KinetixClipSampler _main = null;
		protected bool IsMainNull => Main == null;
		private KinetixClipSampler Main
		{
			get
			{
				_main ??= samplers.FirstOrDefault();

				if (_main != null)
				{
					_main.isMain = true;
				}

				return _main;
			}
		}

		/// <remarks>
		///   <u>How many samplers are in my List ?</u><br/>
		///   <list type="bullet">
		///     <item>
		///       0 if nothing is playing
		///     </item>
		///     <item>
		///       1 if one clip is playing
		///     </item>
		///     <item>
		///       2+ if multiple clips are playing simulteanousely
		///     </item>
		///   </list>
		///   <br/>
		/// 
		///   <u>A sampler is destroyed when:</u><br/>
		///   <list type="bullet">
		///     <item>
		///       No more clip needs to be played
		///     </item>
		///     <item>
		///       A sampler ends and more than one sampler exists
		///     </item>
		///   </list>
		/// </remarks>
		private List<KinetixClipSampler> samplers = new List<KinetixClipSampler>();
		#endregion
		// ---------------------------------------------------------------------- //

		// ====================================================================== //
		//  CTOR AND DISPOSE                                                      //
		// ====================================================================== //
		#region CTOR AND DISPOSE
		public SimulationSampler()
		{
			authorityBridge = new SamplerAuthorityBridge()
			{
				StartNextClip = Authority_StartNextClip,
				GetAvatarPos = Authority_GetAvatarPos,
				GetQueue = Authority_GetQueue,
				GetClip = Authority_GetClip,
				CreateSampler = Authority_CreateSampler,
				GetAvatar = Authority_GetAvatar,
			};

			effect = new EffectSampler(authorityBridge);
			effect.OnFrameAdded += SamplerEffect_OnFrameAdded;
		}

		public void Dispose()
		{
			_main = null;
			samplers = null;
			queue = null;

			effect.OnFrameAdded -= SamplerEffect_OnFrameAdded;

			OnQueueStart = null;
			OnQueueStop = null;
			OnAnimationStart = null;
			OnAnimationStop = null;
			OnPlayedFrame = null;

			playEnabled = false;
		}
		#endregion
		// ---------------------------------------------------------------------- //

		// ====================================================================== //
		//  EFFECT AUTHORITY                                                      //
		// ====================================================================== //
		#region AUTHORITY
		private void                      Authority_StartNextClip(bool additive) => Next(additive);
		private KinetixPose               Authority_GetAvatarPos()               => RequestAvatarPos?.Invoke();
		private SkeletonPool.PoolItem     Authority_GetAvatar()                  => RequestAvatar?.Invoke();
		private Queue<KinetixClipWrapper> Authority_GetQueue()                   => queue;
		private KinetixClipSampler        Authority_CreateSampler()              => new KinetixClipSampler() { SampleFrameHandler = SampleFrameHandler, isMain=false };
		private KinetixClip Authority_GetClip(int index)
		{
			if (index < 0 || index >= samplers.Count)
				return null;

			return samplers[index].Clip;
		}
		#endregion
		// ---------------------------------------------------------------------- //

		// ====================================================================== //
		//  SAMPLER                                                               //
		// ====================================================================== //
		#region SAMPLER
		protected KinetixClipSampler CreateSampler()
		{
			KinetixClipSampler sampler = new KinetixClipSampler
			{
				SampleFrameHandler = SampleFrameHandler
			};

			samplers.Add(sampler);
			return sampler;
		}

		protected void DequeueSampler(int index)
		{
			samplers.RemoveAt(index);
			_main = null; //Uncache
			_main = Main; //Recache
		}

		protected KinetixFrame SampleFrameHandler(KinetixClip kinetixClip, int frame)
		{
			KinetixFrame toReturn = bones == null ?
				new KinetixFrame(out bones, kinetixClip, frame) :
				new KinetixFrame(bones, kinetixClip, frame);

			RequestAdaptToInterpreter?.Invoke(toReturn);

			return toReturn;
		}
		#endregion
		// ---------------------------------------------------------------------- //

		// ====================================================================== //
		//  PLAYMODE CONTROLS                                                     //
		// ====================================================================== //
		#region PLAYMODE CONTROLS
		public void Pause()  => playEnabled = false;
		public void Resume() => playEnabled = true ;

		private void PlayImmediate(KinetixClipWrapper clip, bool additive = false)
		{
			if (IsMainNull) 
			{
				CreateSampler().Play(clip);
				InvokeQueueStart();
			}
			else
			{
				if (additive)
				{
					CreateSampler().Play(clip);
				}
				else
				{
					Main.Play(clip);
				}
			}

			playEnabled = true;

			InvokeAnimationStart(clip);
		}

		public void Play(KinetixClipWrapper clip, bool additive = false)
		{
			if (additive)
			{
				PlayImmediate(clip, additive);
			}
			else
			{
				SamplerEnded(0);
				Add(clip);
			}
		}

		public void PlayRange(params KinetixClipWrapper[] clips)
		{
			if (!IsMainNull)
				Stop();

			AddRange(clips);
		}

		public void SoftStop(float stopDelay)
		{
			if (samplers.Count == 0)
				return;

			softStopTime = stopDelay;
			InvokeOnSoftStop(stopDelay);
		}
		
		public void Stop()
		{
			softStopTime = null;

			if (samplers.Count == 0)
				return;

			KinetixClipSampler sampler = Main;
			ClearQueue();
			samplers.Add(sampler); //keep one sampler so that "sampler ended" works
			SamplerEnded(0);
		}

		public void ClearQueue()
		{
			_main = null;
			samplers.Clear();
			queue.Clear();
		}

		public void Add(KinetixClipWrapper clip)
		{
			if (IsMainNull)
			{
				PlayImmediate(clip);
			}
			else
			{
				queue.Enqueue(clip);
			}
		}

		public void AddRange(params KinetixClipWrapper[] clip)
		{
			int i = 0;
			if (IsMainNull)
			{
				PlayImmediate(clip[0]);
				i = 1;
			}

			int length = clip.Length;
			for (; i < length; i++)
			{
				queue.Enqueue(clip[i]);
			}
		}

		private void Next(bool additive)
		{
			if (queue.Count > 0)
				PlayImmediate(queue.Dequeue(), additive);
		}

		private void SamplerEnded(int index)
		{
			if (samplers.Count == 0) return;

			//If more than one sampler, remove at index.
			if (samplers.Count > 1)
			{
				InvokeAnimationStop(samplers[index].Clip);
				DequeueSampler(index);
				return;
			}
			//From here only one sampler exists

			//If it's not null, tell that we've ended a clip
			if (!IsMainNull)
			{
				InvokeAnimationStop(Main.Clip);	
			}

			//If it still have things to play play it
			if (queue.Count > 0)
			{
				PlayImmediate(queue.Dequeue());
			}
			else
			{
				//This was the last Sampler. Dequeue it and send queue stop event
				if (!IsMainNull)
				{
					DequeueSampler(0);
				}
				InvokeQueueStop();
			}
		}
		#endregion
		// ---------------------------------------------------------------------- //

		// ====================================================================== //
		//  UPDATE                                                                //
		// ====================================================================== //
		public void Update()
		{
			float deltaTime = Time.deltaTime;
			int samplersCount = samplers.Count;
			if (!playEnabled)
				return;

			effect.Update();

			if (softStopTime.HasValue)
			{
				if (softStopTime <= 0)
				{
					Stop();
					return;
				}

				softStopTime -= deltaTime;
			}

			if (samplersCount == 0)
				return;

			bool isMainNull;
			KinetixClipSampler sampler;
			List<KinetixFrame> frames = new List<KinetixFrame>( Enumerable.Repeat<KinetixFrame>(null, samplersCount) );

			while (true)
			{
				sampler = samplers[0];

				if (sampler.Ended) //Security
					return;

				isMainNull =
					(frames[0] = sampler.Update(deltaTime))
					== null;
				
				if (sampler.Ended)
				{
					SamplerEnded(0);
					frames.RemoveAt(0);
					if (--samplersCount == 0)
						return;
					continue;
				}

				break;
			}

			for (int i = samplersCount - 1; i >= 1; i--)
			{
				sampler = samplers[i];

				frames[i] = sampler.Update(deltaTime);
			
				if (sampler.Ended)
				{
					frames.RemoveAt(i);
					SamplerEnded(i);
				}
			}

			if (isMainNull)
				return;

			KinetixFrame frame = effect.ModifyFrame(frames.ToArray());
			OnPlayedFrame?.Invoke(frame);
		}
		// ---------------------------------------------------------------------- //

		// ====================================================================== //
		//  EVENT INVOKE                                                          //
		// ====================================================================== //
		#region EVENT INVOKE
		private void InvokeQueueStart()
		{
			effect.OnQueueStart();
			OnQueueStart?.Invoke();
		}
		private void InvokeQueueStop() 
		{
			effect.OnQueueStop();
			OnQueueStop?.Invoke();
		}
		private void InvokeAnimationStart(KinetixClipWrapper  clip) 
		{
			effect.OnAnimationStart(clip);
			OnAnimationStart?.Invoke(clip);
		}
		private void InvokeAnimationStop (KinetixClipWrapper  clip) 
		{
			effect.OnAnimationStop(clip);
			OnAnimationStop ?.Invoke(clip);
		}
		private void InvokeOnSoftStop (float time) 
		{
			effect.OnSoftStop(time);
		}
		#endregion
		// ---------------------------------------------------------------------- //

		// ====================================================================== //
		//  EVENT HANDLER                                                         //
		// ====================================================================== //
		#region EVENT HANDLER
		private void SamplerEffect_OnFrameAdded(KinetixFrame frame)
		{
			OnPlayedFrame?.Invoke(frame);
		}
		#endregion
		// ---------------------------------------------------------------------- //
	}
}
