// // ----------------------------------------------------------------------------
// // <copyright file="ProviderManager.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

// FILE_WEB3

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
        private static IProviderWrapper ProviderWrapper;

        /// <summary>
        /// Initialize the wallet cache with a specific provider
        /// </summary>
        /// <param name="_APIKey">API Key of provider</param>
        public static void Initialize(string _APIKey)
        {
            CreateProvider(EKinetixNodeProvider.ALCHEMY, _APIKey);
        }

        /// <summary>
        /// Create an instance of the Wrapper based on the provider
        /// </summary>
        /// <param name="_Provider">Node URL Provider</param>
        private static void CreateProvider(EKinetixNodeProvider _Provider, string _APIKey)
        {
            switch (_Provider)
            {
                case EKinetixNodeProvider.ALCHEMY:
                {
                    ProviderWrapper = new AlchemyProviderWrapper(_APIKey);
                    break;
                }
            }
        }

        /// <summary>
        /// Make a Web Request to get all the NFT of the User's Wallet
        /// </summary>
        /// <param name="_WalletAddress">Called upon fetch nfts</param>
        public static async Task<AnimationMetadata[]> GetAnimationMetadataOfOwner(string _WalletAddress)
        {
            try
            {
                if (String.IsNullOrEmpty(_WalletAddress))
                {
                    throw new Exception("Wallet address is empty");
                }
                
                List<KinetixContract> contracts = await KinetixBackendAPI.GetContracts();
                List<string>          addresses = contracts.Select(contract => contract.contractAddress).ToList();
                
                AnimationMetadata[]   animationMetadatas = await ProviderWrapper.GetAnimationsMetadataOfOwner(_WalletAddress, addresses);
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
        public static async Task<AnimationMetadata> GetAnimationMetadataOfNFT(AnimationIds _AnimationIds)
        {
            try
            {
                // TODO : MOVE CONTRACTS IN ALCHEMY PROVIDER 
                await KinetixBackendAPI.GetContracts();
                AnimationMetadata     emoteMetadata = await ProviderWrapper.GetAnimationMetadataOfNft(_AnimationIds);
                return emoteMetadata;
            }
            catch (Exception e)
            {
                KinetixDebug.LogWarningException(e);
                return null;
            }
        }
    }
}

