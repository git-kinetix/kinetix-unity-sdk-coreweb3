// // ----------------------------------------------------------------------------
// // <copyright file="ProviderManager.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kinetix.Utils;
using UnityEngine;

namespace Kinetix.Internal
{
    internal static class ProviderManager
    {
        private static Dictionary<EKinetixNodeProvider, IProviderWrapper> ProviderWrappers;


        /// <summary>
        /// Initialize the wallet cache with a specific provider
        /// </summary>
        /// <param name="_APIKey">API Key of provider</param>
        public static void Initialize(EKinetixNodeProvider provider, string _APIKey)
        {
            if (ProviderWrappers == null)
                ProviderWrappers = new Dictionary<EKinetixNodeProvider, IProviderWrapper>();
            
            CreateProvider(provider, _APIKey);
        }


        /// <summary>
        /// Create an instance of the Wrapper based on the provider
        /// </summary>
        /// <param name="_Provider">Node URL Provider</param>
        private static void CreateProvider(EKinetixNodeProvider _Provider, string _APIKey = "")
        {
            switch (_Provider)
            {
                case EKinetixNodeProvider.ALCHEMY:
                {
                    ProviderWrappers[EKinetixNodeProvider.ALCHEMY] = new AlchemyProviderWrapper(_APIKey);
                    break;
                }
            }
        }


        /// <summary>
        /// Make a Web Request to get all the user's emotes
        /// </summary>
        /// <param name="_Account">The account type</param>
        public static async Task<AnimationMetadata[]> GetAnimationMetadataOfOwner(Account _Account)
        {
            try
            {
                if (String.IsNullOrEmpty(_Account.AccountId))
                {
                    throw new Exception("Account id is empty");
                }
                
                AnimationMetadata[] animationMetadatas = await ProviderWrappers[GetTypeForAccountSpecialization(_Account)].GetAnimationsMetadataOfOwner(_Account.AccountId);
                
                return animationMetadatas;
            }
            catch (Exception e)
            {
                KinetixDebug.LogException(e);
                return new AnimationMetadata[] { };
            }
        }

        /// <summary>
        /// Make a Web Request to get metadata of a specific NFT
        /// </summary>
        /// <param name="_AnimationIds">Animation Ids</param>
        public static async Task<AnimationMetadata> GetAnimationMetadataOfEmote(AnimationIds _AnimationIds)
        {
            try
            {
                AnimationMetadata     emoteMetadata = await ProviderWrappers[_AnimationIds.GetExpectedProvider()].GetAnimationMetadataOfEmote(_AnimationIds);
                
                return emoteMetadata;
            }
            catch (Exception e)
            {
                KinetixDebug.LogWarningException(e);
                return null;
            }
        }

        private static EKinetixNodeProvider GetTypeForAccountSpecialization(Account _Account)
        {

            if (_Account is WalletAccount)
                return EKinetixNodeProvider.ALCHEMY;
            

            return EKinetixNodeProvider.NONE;
        }
    }
}
