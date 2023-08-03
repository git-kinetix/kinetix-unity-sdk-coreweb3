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
using System.Threading;
using Kinetix.Utils;

namespace Kinetix.Internal
{
    internal static class KinetixMetadataBehaviour
    {
        public static async void GetAnimationMetadataByAnimationIds(AnimationIds _Ids,
            Action<AnimationMetadata>                                            _OnSuccess, Action _OnFailure)
        {
            //try
            //{
                if (!KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_Ids).HasMetadata())
                    KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_Ids).SetMetadata(
                        await KinetixCoreBehaviour.ServiceLocator.Get<ProviderService>().GetAnimationMetadataOfEmote(_Ids)
                    );

                _OnSuccess?.Invoke(KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_Ids).Metadata);
            //}
            //catch (Exception e)
            //{
            //    KinetixDebug.LogWarning(e.Message);
            //    //_OnFailure?.Invoke();
            //}
        }

        public static void IsAnimationOwnedByUser(AnimationIds _Ids, Action<bool> _OnSuccess, Action _OnFailure)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<AccountManager>().IsAnimationOwnedByUser(_Ids, _OnSuccess, _OnFailure);
        }

        public static void GetUserAnimationMetadatas(Action<AnimationMetadata[]> _OnSuccess, Action _OnFailure)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<AccountManager>().GetAllUserEmotes(_OnSuccess, _OnFailure);
        }

        public static void GetUserAnimationMetadatasByPage(int _Count,     int    _PageNumber,
            Action<AnimationMetadata[]>                        _OnSuccess, Action _OnFailure)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<AccountManager>().GetUserAnimationsMetadatasByPage(_Count, _PageNumber, _OnSuccess, _OnFailure);
        }

        public static void GetUserAnimationsTotalPagesCount(int _CountByPage, Action<int> _Callback, Action _OnFailure)
        {
            KinetixCoreBehaviour.ManagerLocator.Get<AccountManager>().GetUserAnimationsTotalPagesCount(_CountByPage, _Callback, _OnFailure);
        }

        public static async void LoadIconByAnimationId(AnimationIds _Ids, Action<Sprite> _OnSuccess, CancellationTokenSource cancelToken = null)
        {
            if (KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_Ids).HasMetadata())
            {
                try
                {
                    Sprite sprite = await KinetixCoreBehaviour.ServiceLocator.Get<AssetService>().LoadIcon(_Ids, cancelToken);
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
            if (KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetEmote(_Ids).HasMetadata())
            {
                KinetixCoreBehaviour.ServiceLocator.Get<AssetService>().UnloadIcon(_Ids);
                _OnSuccess?.Invoke();
            }
        }
    }
}
