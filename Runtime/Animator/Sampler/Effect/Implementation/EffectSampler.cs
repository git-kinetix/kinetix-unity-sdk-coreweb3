// // ----------------------------------------------------------------------------
// // <copyright file="EffectSampler.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kinetix.Internal
{
	/// <summary>
	/// Class used by the <see cref="SimulationSampler"/> to sample effects before sending the frame to the network and the
	/// </summary>
	public class EffectSampler
	{
#pragma warning disable IDE0044 // readonly
		private List<IFrameEffect>         frameEffect   = new List<IFrameEffect>();
		private List<IFrameEffectAdd>      frameAdds     = new List<IFrameEffectAdd>();
		private List<IFrameEffectModify>   frameModify   = new List<IFrameEffectModify>();
		private List<ISamplerAuthority>     authority     = new List<ISamplerAuthority>();
		
		private SamplerAuthorityBridge authorityBridge;


		/// <param name="authorityBridge">
		/// Authority on the samplers.<br/>
		/// You can call some methods like "StartNextClip" or "GetAvatarPos".
		/// </param>
		public EffectSampler(SamplerAuthorityBridge authorityBridge)
		{
			this.authorityBridge = authorityBridge;
		}

		/// <summary>
		/// Called when <see cref="IFrameEffectAdd"/> added a frame to play
		/// </summary>
		public event Action<KinetixFrame> OnFrameAdded;
#pragma warning restore IDE0044 // readonly

		/// <summary>
		/// Add an effect to the list of effect
		/// </summary>
		/// <param name="effect"></param>
		public void RegisterEffect(IFrameEffect effect)
		{
			bool added = false;

			if (effect is IFrameEffectAdd add)
			{
				added = true;
				if (!frameAdds.Contains(add))
				{
					frameAdds.Add(add);
					add.OnAddFrame -= IFrameEffectAdd_OnAddFrame;
					add.OnAddFrame += IFrameEffectAdd_OnAddFrame;
				}
			}
			
			if (effect is IFrameEffectModify modify)
			{
				added = true;
				
				if (!frameModify.Contains(modify))
				{
					frameModify.Add(modify);
				}
			}

			if (effect is ISamplerAuthority auth)
			{
				added = true;
				if (!authority.Contains(auth))
				{
					authority.Add(auth);
					auth.Authority = authorityBridge;
				}
			}

			if (!added)
			{
				KinetixLogger.LogError(
					nameof(EffectSampler),
					nameof(IFrameEffect) + " shall be specialised in at least one of the child interface. See " + nameof(IFrameEffectAdd) + " and " + nameof(IFrameEffectModify),
					true
				);
			}
			else
			{
				if (!frameEffect.Contains(effect))
					frameEffect.Add(effect);
			}
		}

		/// <summary>
		/// Remove an effect from the list of effect
		/// </summary>
		/// <param name="effect"></param>
		public void UnRegisterEffect(IFrameEffect effect)
		{
			bool removed = false;

			frameEffect.Remove(effect);
	
			if (effect is IFrameEffectAdd add)
			{
				removed = true;
				frameAdds.Remove(add);
				add.OnAddFrame -= IFrameEffectAdd_OnAddFrame;
			}

			if (effect is IFrameEffectModify modify)
			{
				removed = true;
				frameModify.Remove(modify);
			}

			if (effect is ISamplerAuthority start)
			{
				removed = true;
				authority.Remove(start);
			}

			if (!removed)
			{
				KinetixLogger.LogError(
					nameof(EffectSampler),
					nameof(IFrameEffect) + " shall be specialised in at least one of the child interface. See " + nameof(IFrameEffectAdd) + " and " + nameof(IFrameEffectModify),
					true
				);
			}
		}

		private void IFrameEffectAdd_OnAddFrame(KinetixFrame frame)
		{
			ModifyFrame(ref frame);
			OnFrameAdded?.Invoke(frame);
		}
		
		/// <summary>
		/// Modify a single frame (called by <see cref="IFrameEffectAdd_OnAddFrame"/>
		/// </summary>
		/// <param name="frame"></param>
		public void ModifyFrame(ref KinetixFrame frame)
		{
			KinetixFrame[] frames = new KinetixFrame[] { frame };

			int count = frameModify.Count;
			for (int i = 0; i < count; i++)
			{
				frameModify[i].OnPlayedFrame(ref frame, in frames, 0);
			}
		}

		/// <summary>
		/// Compute the effects for each <see cref="SimulationSampler.Sampler"/>'s result. The first non null frame is considered the main frame
		/// </summary>
		/// <param name="frame">Result of the samplers</param>
		/// <returns></returns>
		public KinetixFrame ModifyFrame(in KinetixFrame[] frame)
		{
			var kvp =
				frame.Select( (f,i) => new KeyValuePair<KinetixFrame, int>(f,i) )
					 .First ( f     => f.Key != null );

			KinetixFrame toReturn = new KinetixFrame(kvp.Key);
			int clonedIndex = kvp.Value;

			int count = frameModify.Count;
			for (int i = 0; i < count; i++)
			{
				frameModify[i].OnPlayedFrame(ref toReturn, in frame, clonedIndex);
			}

			return toReturn;
		}

		/// <summary>
		/// Dispatch the "Update" to the effects
		/// </summary>
		/// <param name="clip"></param>
		public void Update()
		{
			int lenght = frameEffect.Count;
			for (int i = 0; i < lenght; i++)
			{
				frameEffect[i].Update();
			}
		}

		/// <summary>
		/// Dispatch the "OnAnimationStart" to the effects
		/// </summary>
		/// <param name="clip">Clip to give to the effects</param>
		public void OnAnimationStart(KinetixClip clip)
		{
			int lenght = frameEffect.Count;
			for (int i = 0; i < lenght; i++)
			{
				frameEffect[i].OnAnimationStart(clip);
			}
		}

		/// <summary>
		/// Dispatch the "OnAnimationEnd" to the effects
		/// </summary>
		/// <param name="_">Unused by the effects</param>
		public void OnAnimationStop(KinetixClipWrapper _ = default)
		{
			int lenght = frameEffect.Count;
			for (int i = 0; i < lenght; i++)
			{
				frameEffect[i].OnAnimationEnd();
			}
		}

		/// <summary>
		/// Dispatch the "OnAnimationEnd" to the effects
		/// </summary>
		/// <param name="_">Unused by the effects</param>
		public void OnSoftStop(float blendTime)
		{
			int lenght = frameEffect.Count;
			for (int i = 0; i < lenght; i++)
			{
				frameEffect[i].OnSoftStop(blendTime);
			}
		}

		/// <summary>
		/// Dispatch the "OnQueueStart" to the effects
		/// </summary>
		public void OnQueueStart()
		{
			int lenght = frameEffect.Count;
			for (int i = 0; i < lenght; i++)
			{
				frameEffect[i].OnQueueStart();
			}
		}

		/// <summary>
		/// Dispatch the "OnQueueEnd" to the effects
		/// </summary>
		public void OnQueueStop()
		{
			int lenght = frameEffect.Count;
			for (int i = 0; i < lenght; i++)
			{
				frameEffect[i].OnQueueEnd();
			}
		}

	}
}
