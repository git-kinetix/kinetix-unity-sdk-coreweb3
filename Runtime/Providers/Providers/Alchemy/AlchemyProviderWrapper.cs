// // ----------------------------------------------------------------------------
// // <copyright file="AlchemyProviderWrapper.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

// FILE_WEB3

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kinetix.Internal;
using Kinetix.Utils.Alchemy;
using UnityEngine;
using System.Linq;


namespace Kinetix.Utils
{
    public class AlchemyProviderWrapper : IProviderWrapper
    {
        private readonly string apiKey;

        public AlchemyProviderWrapper(string _APIKey)
        {
            apiKey = _APIKey;
        }

        /// <summary>
        /// Make a Web Request to get all the NFTs Metadata of the User's Wallet
        /// </summary>
        public async Task<AnimationMetadata[]> GetAnimationsMetadataOfOwner(string _AccountId)
        {
            TaskCompletionSource<AnimationMetadata[]> tcs = new TaskCompletionSource<AnimationMetadata[]>();

            try
            {
                List<KinetixContract> contracts = await KinetixBackendAPI.GetContracts();
                List<string>          addresses = contracts.Select(contract => contract.contractAddress).ToList();

                GetAnimationsMetadataOfOwnerInternal(_AccountId, addresses, null, null, (metadatas) =>
                {
                    tcs.SetResult(metadatas);   
                });
            }
            catch (Exception)
            {
                return Array.Empty<AnimationMetadata>();
            }
            
            return await tcs.Task;
        }

        private async void GetAnimationsMetadataOfOwnerInternal(string _WalletAddress, List<string> _Contracts, List<AnimationMetadata> _AnimationMetadatas = null, string _PageKey = null, Action<AnimationMetadata[]> _OnSuccess = null)
        {
            if (String.IsNullOrEmpty(apiKey))
            {
                Debug.LogWarning("API Key not registered");
                _OnSuccess?.Invoke(Array.Empty<AnimationMetadata>());
                return;
            }

            string uri = KinetixConstants.c_BaseAlchemyURL + "/" + apiKey + "/" + KinetixConstants.c_AlchemyGetNFTsForOwnerEndpoint;
            KeyValuePair<string, string>[] parameters =
            {
                new KeyValuePair<string, string>("owner", _WalletAddress),
                new KeyValuePair<string, string>("withMetadata", "true"),
            };

            if (_Contracts.Count == 0)
                throw new Exception("No contracts found");
            
            for (int i = 0; i < _Contracts.Count; i++)
            {
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new KeyValuePair<string, string>("contractAddresses[]", _Contracts[i]);
            }
            
            if (!string.IsNullOrEmpty(_PageKey))
            {
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new KeyValuePair<string, string>("pageKey", _PageKey);
            }

            _AnimationMetadatas ??= new List<AnimationMetadata>();

            GetNftsForOwner alchemyResult = await WebRequestHandler.Instance.GetAsync<GetNftsForOwner>(uri, parameters);

            try
            {
                AnimationMetadata[] metadatas = alchemyResult.Deserialize();
                _AnimationMetadatas.AddRange(metadatas);

                if (alchemyResult.HasNextPage())
                {
                    GetAnimationsMetadataOfOwnerInternal(_WalletAddress, _Contracts, _AnimationMetadatas, alchemyResult.pageKey, _OnSuccess);
                }
                else
                {
                    for (int i = 0; i < metadatas.Length; i++)
                    {
                        metadatas[i].CollectionName        = KinetixBackendAPI.GetContractName(metadatas[i].Ids.ContractAddress);
                        metadatas[i].CollectionDescription = KinetixBackendAPI.GetContractDescription(metadatas[i].Ids.ContractAddress);
                    }
                    _OnSuccess?.Invoke(metadatas);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        

        /// <summary>
        /// Make a Web Request to get metadata of specific Emote
        /// </summary>
        public async Task<AnimationMetadata> GetAnimationMetadataOfEmote(AnimationIds _AnimationIds)
        {
            TaskCompletionSource<AnimationMetadata> tcs = new TaskCompletionSource<AnimationMetadata>();

            if (String.IsNullOrEmpty(apiKey))
            {
                throw new Exception("No Alchemy API Key registered");
            }
            
            string uri = KinetixConstants.c_BaseAlchemyURL + "/" + apiKey + "/" + KinetixConstants.c_AlchemyGetNFTMetadataEndpoint;

            KeyValuePair<string, string>[] parameters =
            {
                new KeyValuePair<string, string>("contractAddress", _AnimationIds.ContractAddress),
                new KeyValuePair<string, string>("tokenId", _AnimationIds.TokenID),
                new KeyValuePair<string, string>("refreshCache", "true")
            };

            try
            {
                GetNftMetadataResult alchemyResult     = await WebRequestHandler.Instance.GetAsync<GetNftMetadataResult>(uri, parameters);
                AnimationMetadata    animationMetadata = alchemyResult.Deserialize();
                animationMetadata.CollectionName        = KinetixBackendAPI.GetContractName(_AnimationIds.ContractAddress);
                animationMetadata.CollectionDescription = KinetixBackendAPI.GetContractDescription(_AnimationIds.ContractAddress);
                
                if (string.IsNullOrEmpty(animationMetadata.AnimationURL))
                {
                    tcs.SetException(new Exception("Corrupted metadata for Emote => " + _AnimationIds));
                }
                else
                {
                    tcs.SetResult(animationMetadata);
                }
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
            
            return await tcs.Task;
        }
    }
}

