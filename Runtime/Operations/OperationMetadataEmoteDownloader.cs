using System;
using System.Threading.Tasks;

// FILE_WEB3

namespace Kinetix.Internal
{
    public class OperationMetadataEmoteDownloader : OperationAsync<AnimationMetadata>
    {
        public readonly AnimationIds      animationIds;
        private         AnimationMetadata metadata;
        
        public OperationMetadataEmoteDownloader(AnimationIds _AnimationIds)
        {
            animationIds = _AnimationIds;
        }

        public override async Task<AnimationMetadata> Execute()
        {
            if (ProgressStatus == EProgressStatus.NONE)
            {
                try
                {
                    Task<AnimationMetadata> task = ProviderManager.GetAnimationMetadataOfNFT(animationIds);
                    ProgressStatus = EProgressStatus.PENDING;
                    Task           = task;
                    metadata       = await task;
                    return metadata;
                }
                catch (Exception e)
                {
                    ProgressStatus = EProgressStatus.NONE;
                    KinetixDebug.LogException(e);
                }
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

