using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kinetix.Internal
{
    public class MetadataAccountOperationManager
    {
        private static Dictionary<string, TaskCompletionSource<AnimationMetadata[]>> operationsDownloadMetadataByAccount;
        private static Queue<(OperationMetadataAccountEmotesDownloader, TaskCompletionSource<AnimationMetadata[]>)> queue;


        public static async Task<AnimationMetadata[]> DownloadMetadataByAccount(Account _Account)
        {
            operationsDownloadMetadataByAccount ??= new Dictionary<string, TaskCompletionSource<AnimationMetadata[]>>();
            queue                                     ??= new Queue<(OperationMetadataAccountEmotesDownloader, TaskCompletionSource<AnimationMetadata[]>)>();

            if (operationsDownloadMetadataByAccount.ContainsKey(_Account.AccountId))
                return await operationsDownloadMetadataByAccount[_Account.AccountId].Task;

            
            TaskCompletionSource<AnimationMetadata[]> tcs                = new TaskCompletionSource<AnimationMetadata[]>();
            OperationMetadataAccountEmotesDownloader  accountDownloader = new OperationMetadataAccountEmotesDownloader(_Account);
            operationsDownloadMetadataByAccount.Add(_Account.AccountId, tcs);
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
            operationsDownloadMetadataByAccount.Remove(operation.account.AccountId);

            if (queue.Count > 0)
                DequeueOperations();
        }

        
    }
}
