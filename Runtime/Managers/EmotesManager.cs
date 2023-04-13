using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kinetix.Internal.Cache;
using Kinetix.Utils;
using UnityEngine;

namespace Kinetix.Internal
{
    public static class EmotesManager
    {
        private static Dictionary<string, KinetixEmote> kinetixEmotes;

        public static void Initialize()
        {
            MonoBehaviourHelper.Instance.OnDestroyEvent += OnDestroy;
            kinetixEmotes                               =  new Dictionary<string, KinetixEmote>();
        }

        public static KinetixEmote GetEmote(AnimationIds _AnimationIds)
        {
            if (!kinetixEmotes.ContainsKey(_AnimationIds.UUID))
                kinetixEmotes.Add(_AnimationIds.UUID, new KinetixEmote(_AnimationIds));
            return kinetixEmotes[_AnimationIds.UUID];
        }

        public static KinetixEmote[] GetEmotes(AnimationIds[] _AnimationIds)
        {
            KinetixEmote[] kinetixEmotesTmp = new KinetixEmote[_AnimationIds.Length];
            for (int i = 0; i < _AnimationIds.Length; i++)
            {
                kinetixEmotesTmp[i] = GetEmote(_AnimationIds[i]);
            }

            return kinetixEmotesTmp;
        }

        public static KinetixEmote[] GetAllEmotes()
        {
            return kinetixEmotes?.Values.ToArray();
        }

        #region ANIMATION_CLIP
        
        public static async void GetAnimationClip(AnimationIds _AnimationIds, KinetixAvatar _KinetixAvatar, SequencerPriority _Priority, bool _Force, Action<AnimationClip> _OnSuccess, Action _OnFailure)
        {
            if (_KinetixAvatar == null)
            {
                KinetixDebug.LogWarning("No Avatar Registered");
                _OnFailure?.Invoke();
                return;
            }

            try
            {
                KinetixEmote  emote         = GetEmote(_AnimationIds);
                AnimationClip animationClip = await PreloadRetargetedAnimationClip(emote, _KinetixAvatar, _Priority, _Force);
                _OnSuccess?.Invoke(animationClip);
            }
            catch (Exception e)
            {
                KinetixDebug.LogWarning(_KinetixAvatar == null
                    ? "No Avatar Setup"
                    : $"Error while getting animation {_AnimationIds.UUID} on Avatar {_KinetixAvatar.Avatar.GetInstanceID()} : {e.Message}");

                _OnFailure?.Invoke();
            }
        }
        
        public static void GetAnimationsClip(AnimationIds[] _AnimationIds, KinetixAvatar _KinetixAvatar, SequencerPriority _Priority, bool _Force, Action<AnimationClip[]> _OnSuccess, Action _OnFailure)
        {
            Dictionary<AnimationIds, AnimationClip> animationClips = new Dictionary<AnimationIds, AnimationClip>();
            for (int i = 0; i < _AnimationIds.Length; i++)
            {
                AnimationIds ids = _AnimationIds[i];
                GetAnimationClip(_AnimationIds[i], _KinetixAvatar, _Priority, _Force, (animationClip) =>
                {
                    if (!animationClips.ContainsKey(ids))
                    {
                        animationClips.Add(ids, animationClip);
                    }

                    if (animationClips.Count == _AnimationIds.Length)
                    {
                        _OnSuccess?.Invoke(GetOrderedAnimationClips(animationClips, _AnimationIds));
                    }
                }, () => { _OnFailure?.Invoke(); });
            }
        }

        private static AnimationClip[] GetOrderedAnimationClips(Dictionary<AnimationIds, AnimationClip> _AnimationsMatchingClips, AnimationIds[] _Ids)
        {
            AnimationClip[] animationClips = new AnimationClip[_Ids.Length];

            for (int i = 0; i < _Ids.Length; i++)
            {
                animationClips[i] = _AnimationsMatchingClips[_Ids[i]];
            }

            return animationClips;
        }

        #endregion

        public static async void LoadAnimation(KinetixEmote _KinetixEmote, KinetixAvatar _KinetixAvatar, SequencerPriority _Priority, Action _OnSuccess = null, Action _OnFailure = null)
        {
            
            switch (_KinetixAvatar.ExportType)
            {
                case EExportType.AnimationClipLegacy:
                {
                    try
                    {
                        AnimationClip clip = await PreloadRetargetedAnimationClip(_KinetixEmote, _KinetixAvatar, _Priority, false);
                        if (clip != null)
                        {
                            _OnSuccess?.Invoke();
                        }
                        else
                        {
                            _OnFailure?.Invoke();
                        }
                    }
                    catch (Exception e)
                    {
                        KinetixDebug.LogException(e);
                        _OnFailure?.Invoke();
                    }

                    break;
                }
            }
        }

        #region Preload Animation

        private static async Task<AnimationClip> PreloadRetargetedAnimationClip(KinetixEmote _KinetixEmote, KinetixAvatar _KinetixAvatar, SequencerPriority _Priority, bool _Force)
        {
            try
            {
                AnimationClip animationClip = await _KinetixEmote.GetRetargetedAnimationClipByAvatar(_KinetixAvatar, _Priority, _Force);
                return animationClip;
            }
            catch (Exception e)
            {
                KinetixDebug.LogWarningException(e);
            }

            return null;
        }

        #endregion

        public static bool IsAnimationAvailable(KinetixAvatar _KinetixAvatar, AnimationIds _Ids)
        {
            return GetEmote(_Ids).HasAnimationRetargeted(_KinetixAvatar);
        }

        public static void RegisterAnimationToNotifyOnReady(KinetixAvatar _KinetixAvatar, AnimationIds _Ids, Action _OnSucceed)
        {
            try
            {
                KinetixEmote emote = GetEmote(_Ids);
                emote.RegisterCallbacksOnRetargetedByAvatar(_KinetixAvatar, _OnSucceed);
            }
            catch (Exception e)
            {
                KinetixDebug.Log("Can't register animation to notify on ready" + e.Message);
            }
        }

        public static void ClearAvatar(KinetixAvatar _KinetixAvatar)
        {
            try
            {
                if (_KinetixAvatar == null)
                    return;

                if (GetTotalKinetixAvatarByType(_KinetixAvatar) > 0)
                    return;

                foreach (KinetixEmote kinetixEmotesValue in kinetixEmotes.Values)
                {
                    kinetixEmotesValue.ClearAvatar(_KinetixAvatar);
                }

                KinetixDebug.Log("[CLEAR] Avatar : " + _KinetixAvatar.GetHashCode());
                MemoryManager.CheckStorage();
            }
            catch (Exception e)
            {
                KinetixDebug.Log("Can't clear avatar : " + e.Message);
            }
        }


        public static void ForceClearEmote(KinetixEmote _kinetixEmote, KinetixAvatar[] _AvoidAvatars = null)
        {
            _kinetixEmote.ClearAllAvatars(_AvoidAvatars);
        }

        public static void ClearEmote(KinetixAvatar _KinetixAvatar, AnimationIds _Ids)
        {
            try
            {
                if (_KinetixAvatar == null)
                    return;

                if (_KinetixAvatar.Equals(LocalPlayerManager.KAvatar) && LocalPlayerManager.IsEmoteUsedByPlayer(_Ids))
                    return;

                KinetixDebug.Log("[CLEAR] Animation : " + _Ids);
                GetEmote(_Ids).ClearAvatar(_KinetixAvatar);
                FileOperationManager.ClearEmote(_Ids);
            }
            catch (Exception e)
            {
                KinetixDebug.LogWarning("Can't clear animation : " + e.Message);
            }
        }

        private static int GetTotalKinetixAvatarByType(KinetixAvatar _KinetixAvatar)
        {
            try
            {
                if (_KinetixAvatar == null)
                    return 0;

                int count = 0;

                if (LocalPlayerManager.KAvatar != null && LocalPlayerManager.KAvatar.Equals(_KinetixAvatar))
                    count++;
                    
                return count;
            }
            catch (Exception e)
            {
                KinetixDebug.Log("Failed getting Total Kinetix By Type : " + e.Message);
                return 0;
            }
        }

        private static void OnDestroy()
        {
            KinetixEmote[] emotes = GetAllEmotes();
            for (int i = 0; i < emotes.Length; i++)
            {
                emotes[i].Destroy();
            }
        }
    }
}
