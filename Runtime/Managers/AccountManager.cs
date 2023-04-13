using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Internal.Cache;
using Kinetix.Internal.Utils;
using UnityEngine;

namespace Kinetix.Internal
{
    public static class AccountManager
    {
        public static Action OnUpdatedAccount;

        private static Dictionary<string, Account> AccountsByWalletAddress;

        public static void Initialize()
        {
            AccountsByWalletAddress = new Dictionary<string, Account>(); 
            AddFreeAnimations();
        }

        public static void ConnectWallet(string _WalletAddress)
        {
            if (AccountsByWalletAddress.ContainsKey(_WalletAddress))
            {
                KinetixDebug.LogWarning("Account is already connected");
                return;
            }

            Account account = new Account(_WalletAddress);
            AccountsByWalletAddress.Add(_WalletAddress, account);
        }

        public static void DisconnectWallet(string _WalletAddress)
        {
            if (!AccountsByWalletAddress.ContainsKey(_WalletAddress))
                return;

            GetAllUserEmotes(beforeAnimationMetadatas =>
            {
                List<AnimationIds> idsBeforeRemoveWallet = beforeAnimationMetadatas.ToList().Select(metadata => metadata.Ids).ToList();
                AccountsByWalletAddress.Remove(_WalletAddress);
                GetAllUserEmotes(afterAnimationMetadatas =>
                {
                    List<AnimationIds> idsAfterRemoveWallet = afterAnimationMetadatas.ToList().Select(metadata => metadata.Ids).ToList();
                    idsBeforeRemoveWallet = idsBeforeRemoveWallet.Except(idsAfterRemoveWallet).ToList();

                    LocalPlayerManager.UnloadLocalPlayerAnimations(idsBeforeRemoveWallet.ToArray());
                    OnUpdatedAccount?.Invoke();
                });
            });
        }

        public static void DisconnectAllWallets()
        {
            foreach (KeyValuePair<string, Account> kvp in AccountsByWalletAddress)
            {
                DisconnectWallet(kvp.Key);
            }
        }

        public static async void GetAllUserEmotes(Action<AnimationMetadata[]> _OnSuccess, Action _OnFailure = null)
        {
            List<KinetixEmote> emotesAccountAggregation = new List<KinetixEmote>();
            List<Account>      accounts = AccountsByWalletAddress.Values.ToList();
            int                countAccount = AccountsByWalletAddress.Count;

            try
            {
                KinetixEmote[] freeEmotes = await FreeAnimationsManager.GetFreeEmotes();            
                emotesAccountAggregation.AggregateAndDistinct(freeEmotes);
            }
            catch (OperationCanceledException)
            {
                _OnFailure?.Invoke();
            }
            catch (Exception e)
            {
                KinetixDebug.LogWarning(e.Message);
                _OnFailure?.Invoke();
            }


            if (accounts.Count == 0)
            {                
                _OnSuccess?.Invoke(emotesAccountAggregation.Select(emote => emote.Metadata).ToArray());
                return;
            }

            try
            {
                for (int i = 0; i < accounts.Count; i++)
                {
                    KinetixEmote[]     accountEmotes = await accounts[i].FetchMetadatas();
                    List<KinetixEmote> accountEmotesList = accountEmotes.ToList();
                    
                    // Remove all animations with are duplicated and not owned
                    emotesAccountAggregation.RemoveAll(metadata => accountEmotesList.Exists(emote => emote.Ids.UUID == metadata.Ids.UUID && emote.Metadata.Ownership != EOwnership.OWNER));
                    
                    emotesAccountAggregation.AggregateAndDistinct(accountEmotes);
                    countAccount--;

                    if (countAccount == 0)
                    {
                        emotesAccountAggregation = emotesAccountAggregation.OrderBy(emote => emote.Metadata.Ownership).ToList();
                        KinetixEmote[] metadatasAccountsAggregationArray = emotesAccountAggregation.ToArray();
                        _OnSuccess?.Invoke(metadatasAccountsAggregationArray.Select(emote => emote.Metadata).ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                _OnSuccess?.Invoke(emotesAccountAggregation.Select(emote => emote.Metadata).ToArray());
                KinetixDebug.LogWarning(e.Message);
            }
        }

        

        public static void AddFreeAnimations()
        {
            FreeAnimationsManager.AddFreeAnimations(OnUpdatedAccount);
        }
        
        public static void IsAnimationOwnedByUser(AnimationIds _AnimationIds, Action<bool> _OnSuccess, Action _OnFailure = null)
        {
            GetAllUserEmotes(metadatas => { _OnSuccess.Invoke(metadatas.ToList().Exists(metadata => metadata.Ids.Equals(_AnimationIds))); }, _OnFailure);
        }
        
        public static void GetUserAnimationsMetadatasByPage(int _Count, int _Page, Action<AnimationMetadata[]> _Callback, Action _OnFailure)
        {
            GetAllUserEmotes(animationMetadatas =>
            {
                if ((_Page + 1) * _Count <= animationMetadatas.Length)
                {
                    _Callback?.Invoke(animationMetadatas.ToList().GetRange((_Page * _Count), _Count).ToArray());
                }
                else
                {
                    int lastPageCount = animationMetadatas.Length % _Count;
                    _Callback?.Invoke(animationMetadatas.ToList()
                        .GetRange(animationMetadatas.Length - lastPageCount, lastPageCount).ToArray());
                }
            }, () => { _OnFailure?.Invoke(); });
        }

        public static void GetUserAnimationsTotalPagesCount(int _CountByPage, Action<int> _Callback, Action _OnFailure)
        {
            GetAllUserEmotes(animationMetadatas =>
            {
                if (animationMetadatas.Length == 0)
                {
                    _Callback?.Invoke(1);
                    return;
                }

                int totalPage = Mathf.CeilToInt((float)animationMetadatas.Length / (float)_CountByPage);
                _Callback?.Invoke(totalPage);
            }, () => { _OnFailure?.Invoke(); });
        }
    }
}
