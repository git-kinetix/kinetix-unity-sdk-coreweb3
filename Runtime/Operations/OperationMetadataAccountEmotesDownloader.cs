using System.Threading.Tasks;
using Kinetix;
using Kinetix.Internal;

// FILE_WEB3

public class OperationMetadataAccountEmotesDownloader : OperationAsync<AnimationMetadata[]>
{
    public readonly string              walletAddress;
    private          AnimationMetadata[] metadata;
        
    public OperationMetadataAccountEmotesDownloader(string _WalletAddress)
    {
        walletAddress = _WalletAddress;
    }

    public override async Task<AnimationMetadata[]> Execute()
    {
        if (ProgressStatus == EProgressStatus.NONE)
        {
            Task<AnimationMetadata[]> task = ProviderManager.GetAnimationMetadataOfOwner(walletAddress);
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

