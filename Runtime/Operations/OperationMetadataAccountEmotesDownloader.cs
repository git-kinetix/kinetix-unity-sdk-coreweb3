using System;
using System.Threading.Tasks;

namespace Kinetix.Internal
{
    public class OperationMetadataAccountEmotesDownloader : OperationAsync<AnimationMetadata[]>
    {
        public readonly Account              account;
        private          AnimationMetadata[] metadata;
        
        public OperationMetadataAccountEmotesDownloader(Account _Account)
        {
            account = _Account;
        }

        public override async Task<AnimationMetadata[]> Execute()
        {
            if (ProgressStatus == EProgressStatus.NONE)
            {
                Task<AnimationMetadata[]> task = ProviderManager.GetAnimationMetadataOfOwner(account);
                ProgressStatus = EProgressStatus.PENDING;
                Task           = task;
                metadata       = await task;
                return metadata;
            }

            if (ProgressStatus != EProgressStatus.COMPLETED)
            {
                metadata = await Task;
                return metadata;
            }
                
            return metadata;
        }
    }
}
