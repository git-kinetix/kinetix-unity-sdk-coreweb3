using System.Collections.Generic;
using System.Threading.Tasks;
using Kinetix.Utils;

namespace Kinetix.Internal
{
    public static class MetadataOperationManager
    {
        private static Dictionary<AnimationIds, TaskCompletionSource<AnimationMetadata>>                  operationsDownloadMetadataByTokens;
        private static Queue<(OperationMetadataEmoteDownloader, TaskCompletionSource<AnimationMetadata>)> queue;
        
        public static async Task<AnimationMetadata> DownloadMetadataByAnimationIds(AnimationIds _AnimationIds)
        {
            operationsDownloadMetadataByTokens ??= new Dictionary<AnimationIds, TaskCompletionSource<AnimationMetadata>>();
            queue                              ??= new Queue<(OperationMetadataEmoteDownloader, TaskCompletionSource<AnimationMetadata>)>();

            if (operationsDownloadMetadataByTokens.ContainsKey(_AnimationIds))
                return await operationsDownloadMetadataByTokens[_AnimationIds].Task;


            TaskCompletionSource<AnimationMetadata> tcs                = new TaskCompletionSource<AnimationMetadata>();
            OperationMetadataEmoteDownloader        metadataDownloader = new OperationMetadataEmoteDownloader(_AnimationIds);
            operationsDownloadMetadataByTokens.Add(_AnimationIds, tcs);
            queue.Enqueue((metadataDownloader, tcs));

            if (queue.Count == 1)
                DequeueOperations();
            
            return await tcs.Task;
        }

        private static async void DequeueOperations()
        {
            (OperationMetadataEmoteDownloader operation, TaskCompletionSource<AnimationMetadata> tcs) = queue.Peek();

            AnimationMetadata metadata = await operation.Execute();
            tcs.SetResult(metadata);
            queue.Dequeue();
            operationsDownloadMetadataByTokens.Remove(operation.animationIds);

            if (queue.Count > 0)
            {
                await TaskUtils.Delay(0.0f);
                DequeueOperations();
            }
        }
    }
}
