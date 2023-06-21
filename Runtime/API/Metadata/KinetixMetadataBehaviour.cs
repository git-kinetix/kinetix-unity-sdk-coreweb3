// // ----------------------------------------------------------------------------
// // <copyright file="KinetixWalletBehaviour.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Utils;

namespace Kinetix.Internal
{
    internal static class KinetixMetadataBehaviour
    {
        public static async void GetAnimationMetadataByAnimationIds(AnimationIds _Ids,
            Action<AnimationMetadata>                                            _OnSuccess, Action _OnFailure)
        {
            try
            {
                if (!EmotesManager.GetEmote(_Ids).HasMetadata())
                {
                    EmotesManager.GetEmote(_Ids).SetMetadata(await MetadataOperationManager.DownloadMetadataByAnimationIds(_Ids));
                }

                _OnSuccess?.Invoke(EmotesManager.GetEmote(_Ids).Metadata);
            }
            catch (Exception e)
            {
                KinetixDebug.LogWarning(e.Message);
                _OnFailure?.Invoke();
            }
        }

        public static void IsAnimationOwnedByUser(AnimationIds _Ids, Action<bool> _OnSuccess, Action _OnFailure)
        {
            AccountManager.IsAnimationOwnedByUser(_Ids, _OnSuccess, _OnFailure);
        }

        public static void GetUserAnimationMetadatas(Action<AnimationMetadata[]> _OnSuccess, Action _OnFailure)
        {
            AccountManager.GetAllUserEmotes(_OnSuccess, _OnFailure);
        }

        public static void GetUserAnimationMetadatasByPage(int _Count,     int    _PageNumber,
            Action<AnimationMetadata[]>                        _OnSuccess, Action _OnFailure)
        {
            AccountManager.GetUserAnimationsMetadatasByPage(_Count, _PageNumber, _OnSuccess, _OnFailure);
        }

        public static void GetUserAnimationsTotalPagesCount(int _CountByPage, Action<int> _Callback, Action _OnFailure)
        {
            AccountManager.GetUserAnimationsTotalPagesCount(_CountByPage, _Callback, _OnFailure);
        }

        public static async void LoadIconByAnimationId(AnimationIds _Ids, Action<Sprite> _OnSuccess,
            TokenCancel                                             cancelToken = null)
        {
            if (EmotesManager.GetEmote(_Ids).HasMetadata())
            {
                try
                {
                    Sprite sprite = await AssetManager.LoadIcon(_Ids, cancelToken);
                    _OnSuccess?.Invoke(sprite);
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception)
                {
                    _OnSuccess?.Invoke(null);
                }
            }
        }

        public static void UnloadIconByAnimationId(AnimationIds _Ids, Action _OnSuccess, Action _OnFailure)
        {
            if (EmotesManager.GetEmote(_Ids).HasMetadata())
            {
                AssetManager.UnloadIcon(_Ids);
                _OnSuccess?.Invoke();
            }
        }
    }
}
