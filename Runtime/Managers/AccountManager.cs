using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kinetix.Internal.Cache;
using Kinetix.Internal.Utils;
using Kinetix.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Kinetix.Internal
{
    public static class AccountManager
    {
        public static Action OnUpdatedAccount;
        public static Action OnConnectedAccount;

        private static List<Account> Accounts;
        private static string        VirtualWorldId;


        public static void Initialize()
        {
            Initialize(string.Empty);
        }

        public static void Initialize(string _VirtualWorldId)
        {
            Accounts = new List<Account>();

            VirtualWorldId = _VirtualWorldId;
        }

        public static void ConnectWallet(string _WalletAddress, Action _OnSuccess = null)
        {
            if (IsAccountAlreadyConnected(_WalletAddress))
            {
                KinetixDebug.LogWarning("Account is already connected");
            }

            WalletAccount account = new WalletAccount(_WalletAddress);
            Accounts.Add(account);

            OnUpdatedAccount?.Invoke();

            _OnSuccess?.Invoke();
        }

        public static void DisconnectWallet(string _WalletAddress)
        {
            int foundIndex = -1;

            for (int i = 0; i < Accounts.Count; i++)
            {
                if (Accounts[i].AccountId == _WalletAddress && Accounts[i] is WalletAccount)
                {
                    foundIndex = i;
                }
            }

            RemoveEmotesAndAccount(foundIndex);
        }

        public static void DisconnectAllWallets()
        {
            foreach (Account acc in Accounts)
            {
                if (acc is WalletAccount)
                    DisconnectWallet(acc.AccountId);
            }
        }


        public static bool IsAccountAlreadyConnected(string _AccountId)
        {
            foreach (Account acc in Accounts)
            {
                if (acc.AccountId == _AccountId)
                {
                    return true;
                }
            }

            return false;
        }


        public static async void GetAllUserEmotes(Action<AnimationMetadata[]> _OnSuccess, Action _OnFailure = null)
        {
            List<KinetixEmote> emotesAccountAggregation = new List<KinetixEmote>();
            int                countAccount             = Accounts.Count;


            if (Accounts.Count == 0)
            {
                _OnSuccess?.Invoke(emotesAccountAggregation.Select(emote => emote.Metadata).ToArray());
                return;
            }

            try
            {
                for (int i = 0; i < Accounts.Count; i++)
                {
                    KinetixEmote[] accountEmotes = await Accounts[i].FetchMetadatas();

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


        private static void RemoveEmotesAndAccount(int accountIndex)
        {
            if (accountIndex == -1)
                return;

            GetAllUserEmotes(beforeAnimationMetadatas =>
            {
                List<AnimationIds> idsBeforeRemoveWallet = beforeAnimationMetadatas.ToList().Select(metadata => metadata.Ids).ToList();

                Accounts.RemoveAt(accountIndex);

                GetAllUserEmotes(afterAnimationMetadatas =>
                {
                    List<AnimationIds> idsAfterRemoveWallet = afterAnimationMetadatas.ToList().Select(metadata => metadata.Ids).ToList();
                    idsBeforeRemoveWallet = idsBeforeRemoveWallet.Except(idsAfterRemoveWallet).ToList();

                    LocalPlayerManager.ForceUnloadLocalPlayerAnimations(idsBeforeRemoveWallet.ToArray());
                    LocalPlayerManager.RemoveLocalPlayerEmotesToPreload(idsBeforeRemoveWallet.ToArray());
                    OnUpdatedAccount?.Invoke();
                });
            });
        }
    }
}
