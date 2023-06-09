// // ----------------------------------------------------------------------------
// // <copyright file="KinetixCharacterComponent.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using UnityEngine;
using Kinetix.Internal;

namespace Kinetix
{
    public class KinetixCharacterComponent : MonoBehaviour
    {
        public Action<AnimationIds> OnAnimationStart;
        public Action<AnimationIds> OnAnimationEnd;

        public RootMotionConfig RootMotionConfig { get { return clipSampler.RootMotionConfig; } set { clipSampler.RootMotionConfig = value; } }

        private string      guid;
        private ClipSampler clipSampler;
        private NetworkPoseSampler networkSampler;

        // CACHE
        private KinetixAvatar kinetixAvatar;

        public void Init(KinetixAvatar _KinetixAvatar)
        {
            kinetixAvatar                =  _KinetixAvatar;
            clipSampler                  =  gameObject.AddComponent<ClipSampler>();
            clipSampler.OnAnimationStart += AnimationStart;
            clipSampler.OnAnimationEnd   += AnimationEnd;
            clipSampler.OnStop += OnClipSamplerBlendedOut;
            networkSampler = new NetworkPoseSampler(this, GetComponent<Animator>(), clipSampler);

            guid = Guid.NewGuid().ToString();
        }

        public void Init(KinetixAvatar _KinetixAvatar, RootMotionConfig _RootMotionConfig)
        {
            Init(_KinetixAvatar);
            clipSampler.Init();
            clipSampler.RootMotionConfig = _RootMotionConfig;
        }

        private void OnDestroy()
        {
            Dispose();
        }
        
        public void PlayAnimation(AnimationIds _AnimationIds, bool _Local, Action<AnimationIds> _OnPlayedAnimation)
        {
            if (_AnimationIds.UUID == null)
            {
                KinetixDebug.LogWarning($"Animation {_AnimationIds.UUID} cannot be null when Play Animation");
                return;
            }
            
            PlayAnimationQueue(new [] {_AnimationIds}, false, _Local, (queue =>
            {
                _OnPlayedAnimation?.Invoke(_AnimationIds);
            }));
        }
        
        public void PlayAnimationQueue(AnimationIds[] _AnimationIdsArray, bool _Loop, bool _Local, Action<AnimationIds[]> _OnPlayedAnimation)
        {
            AnimationQueue animationQueue = new AnimationQueue(_AnimationIdsArray, _Loop);
            foreach(AnimationIds ids in animationQueue.m_animationsIds)
            {
                if (ids.UUID == null)
                {
                    KinetixDebug.LogWarning($"Animation {ids.UUID} cannot be null when Play Animation");
                    return;
                }

                if (!EmotesManager.GetEmote(ids).HasAnimationRetargeted(kinetixAvatar))
                {
                    KinetixDebug.LogWarning($"Animation {ids.UUID} was not loaded when Play Animation");
                    return;
                }
            }
            

            EmotesManager.GetAnimationsClip(animationQueue.m_animationsIds, kinetixAvatar, _Local ? SequencerPriority.VeryHigh : SequencerPriority.High, _Local, (clips) =>
            {
                animationQueue.m_animationsClips = clips;
                clipSampler.Play(animationQueue);
                networkSampler.StartPose();
                _OnPlayedAnimation?.Invoke(_AnimationIdsArray);
            }, () =>
            {
                KinetixDebug.LogWarning("Can't get all animations for the queue.");
            });
        }


        #region Network callbacks

        public void ApplySerializedPose(byte[] pose, double timestamp)
        {
            networkSampler.ApplyPose(KinetixNetworkedPose.FromByte(pose), timestamp);
        }

        public byte[] GetSerializedPose()
        {
            byte[] pose = networkSampler.GetPose()?.ToByteArray();

            if (pose == null) {
                pose = new byte[0];
            }

            return pose;
        }

        public void OnClipSamplerBlendedOut()
        {
            networkSampler.StopPose();
        }


        #endregion
        
        public void StopAnimation()
        {
            clipSampler.Stop();
        }
        
        private void AnimationStart(AnimationIds _AnimationIds)
        {
            OnAnimationStart?.Invoke(_AnimationIds);
        }

        private void AnimationEnd(AnimationIds _AnimationIds)
        {
            OnAnimationEnd?.Invoke(_AnimationIds);
        }

        public void Dispose()
        {
            KinetixEmote[] kinetixEmotes = EmotesManager.GetAllEmotes();
            if (kinetixEmotes == null)
                return;
            
            
            if (clipSampler != null)
                clipSampler.Dispose();
        }
    }
}
