// FILE_WEB3

using System;
using System.Threading.Tasks;

namespace Kinetix.Internal
{
    public class Account
    {
        private readonly string                                          walletAddress;
        private          KinetixEmote[]                                  emotes;

        public Account(string _WalletAddress)
        {
            walletAddress                 = _WalletAddress;
            PreFetch();
        }

        private async void PreFetch()
        {
            await FetchMetadatas();
        }
        
        public async Task<KinetixEmote[]> FetchMetadatas()
        {
            if (emotes != null)
                return emotes;
            
            try
            {
                AnimationMetadata[] animationMetadatas = await MetadataAccountOperationManager.DownloadMetadataByWalletAddress(walletAddress);
                emotes = new KinetixEmote[animationMetadatas.Length];
                
                for (int i = 0; i < animationMetadatas.Length; i++)
                {
                    KinetixEmote emote = EmotesManager.GetEmote(animationMetadatas[i].Ids);
                    emote.SetMetadata(animationMetadatas[i]);
                    emotes[i] = emote;
                }
                
                //AccountManager.OnUpdatedAccount?.Invoke();
                return emotes;
            }
            catch (Exception)
            {
                return new KinetixEmote[] { };
            }
            
        }

    }
}

