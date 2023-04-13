// FILE_WEB3

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kinetix;
using UnityEngine;

public class MetadataAccountOperationManager : MonoBehaviour
{
    private static Dictionary<string, TaskCompletionSource<AnimationMetadata[]>>                                operationsDownloadMetadataByWalletAddress;
    private static Queue<(OperationMetadataAccountEmotesDownloader, TaskCompletionSource<AnimationMetadata[]>)> queue;

    public static async Task<AnimationMetadata[]> DownloadMetadataByWalletAddress(string _WalletAddress)
    {
        operationsDownloadMetadataByWalletAddress ??= new Dictionary<string, TaskCompletionSource<AnimationMetadata[]>>();
        queue                                     ??= new Queue<(OperationMetadataAccountEmotesDownloader, TaskCompletionSource<AnimationMetadata[]>)>();


        if (operationsDownloadMetadataByWalletAddress.ContainsKey(_WalletAddress))
            return await operationsDownloadMetadataByWalletAddress[_WalletAddress].Task;

        
        TaskCompletionSource<AnimationMetadata[]> tcs                = new TaskCompletionSource<AnimationMetadata[]>();
        OperationMetadataAccountEmotesDownloader  accountDownloader = new OperationMetadataAccountEmotesDownloader(_WalletAddress);
        operationsDownloadMetadataByWalletAddress.Add(_WalletAddress, tcs);
        queue.Enqueue((accountDownloader, tcs));

        if (queue.Count == 1)
            DequeueOperations();
            
        return await tcs.Task;
    }
    
    private static async void DequeueOperations()
    {
        (OperationMetadataAccountEmotesDownloader operation, TaskCompletionSource<AnimationMetadata[]> tcs) = queue.Peek();

        AnimationMetadata[] metadatas = await operation.Execute();
        tcs.SetResult(metadatas);
        queue.Dequeue();
        operationsDownloadMetadataByWalletAddress.Remove(operation.walletAddress);

        if (queue.Count > 0)
            DequeueOperations();
    }
}
