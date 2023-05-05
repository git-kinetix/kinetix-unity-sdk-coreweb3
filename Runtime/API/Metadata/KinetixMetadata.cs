// // ----------------------------------------------------------------------------
// // <copyright file="KinetixWallet.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using UnityEngine;
using Kinetix.Utils;

namespace Kinetix.Internal
{
    public class KinetixMetadata
    {
        /// <summary>
        /// Get metadata of a specific animation
        /// </summary>
        /// <param name="_Ids">Ids of the animation</param>
        /// <param name="_OnSuccess">Return the metadata</param>
        public void GetAnimationMetadataByAnimationIds(AnimationIds _Ids, Action<AnimationMetadata> _OnSuccess, Action _OnFailure = null)
        {
            KinetixMetadataBehaviour.GetAnimationMetadataByAnimationIds(_Ids, _OnSuccess, _OnFailure);
        }

        public void IsAnimationOwnedByUser(AnimationIds _Ids, Action<bool> _OnSuccess, Action _OnFailure = null)
        {
            KinetixMetadataBehaviour.IsAnimationOwnedByUser(_Ids, _OnSuccess, _OnFailure);
        }
        
        public void GetUserAnimationMetadatas(Action<AnimationMetadata[]> _OnSuccess, Action _OnFailure = null)
        {
            KinetixMetadataBehaviour.GetUserAnimationMetadatas(_OnSuccess, _OnFailure);
        }

        public void GetUserAnimationMetadatasByPage(int _Count, int _PageNumber, Action<AnimationMetadata[]> _OnSuccess, Action _OnFailure = null)
        {
            KinetixMetadataBehaviour.GetUserAnimationMetadatasByPage(_Count, _PageNumber, _OnSuccess, _OnFailure);
        }

        public void GetUserAnimationMetadatasTotalPagesCount(int _CountByPage, Action<int> _OnSuccess, Action _OnFailure = null)
        {
            KinetixMetadataBehaviour.GetUserAnimationsTotalPagesCount(_CountByPage, _OnSuccess, _OnFailure);
        }

        public void LoadIconByAnimationId(AnimationIds _Ids, Action<Sprite> _OnSuccess, TokenCancel token=null)
        {
            KinetixMetadataBehaviour.LoadIconByAnimationId(_Ids, _OnSuccess, token);
        }

        public void UnloadIconByAnimationId(AnimationIds _Ids, Action _OnSuccess = null, Action _OnFailure = null)
        {
            KinetixMetadataBehaviour.UnloadIconByAnimationId(_Ids, _OnSuccess, _OnFailure);
        }
    }
}
