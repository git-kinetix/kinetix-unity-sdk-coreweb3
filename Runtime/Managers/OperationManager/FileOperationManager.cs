using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kinetix.Utils;

namespace Kinetix.Internal
{
    public static class FileOperationManager
    {
        private static Dictionary<AnimationIds, OperationFileDownloader>              operationsFileDownloader;
        private static Queue<(OperationFileDownloader, TaskCompletionSource<string>)> queue;

        private static bool isDequeuing;

        public static async Task<string> DownloadGLBByEmote(KinetixEmote _Emote, SequencerCancel _CancelToken)
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            operationsFileDownloader ??= new Dictionary<AnimationIds, OperationFileDownloader>();
            queue                    ??= new Queue<(OperationFileDownloader, TaskCompletionSource<string>)>();

            if (!operationsFileDownloader.ContainsKey(_Emote.Ids))
            {
                OperationFileDownloader fileDownloader = new OperationFileDownloader(_Emote, _CancelToken);
                operationsFileDownloader.Add(_Emote.Ids, fileDownloader);
                queue.Enqueue((fileDownloader, tcs));

                if (!isDequeuing)
                    DequeueOperations();

                return await tcs.Task;
            }

            SequencerCancel cancelToken = operationsFileDownloader[_Emote.Ids].CancelToken;
            if (cancelToken != null && cancelToken.canceled)
            {
                Clear(_Emote.Ids);
                OperationFileDownloader fileDownloader = new OperationFileDownloader(_Emote, _CancelToken);
                operationsFileDownloader.Add(_Emote.Ids, fileDownloader);
                queue.Enqueue((fileDownloader, tcs));

                if (!isDequeuing)
                    DequeueOperations();

                return await tcs.Task;
            }

            if (!isDequeuing)
                DequeueOperations();

            return await operationsFileDownloader[_Emote.Ids].Task;
        }

        private static void Clear(AnimationIds emoteIds)
        {
            if (operationsFileDownloader != null && operationsFileDownloader.ContainsKey(emoteIds))
            {
                operationsFileDownloader.Remove(emoteIds);
            }
        }

        private static async void DequeueOperations()
        {
            isDequeuing                                                           = true;
            (OperationFileDownloader operation, TaskCompletionSource<string> tcs) = queue.Peek();

            try
            {
                if (operation.CancelToken != null && operation.CancelToken.canceled)
                {
                    Clear(operation.kinetixEmote.Ids);
                    tcs.TrySetCanceled();
                }
                else
                {
                    string path = await operation.Execute();
                    Clear(operation.kinetixEmote.Ids);
                    tcs.TrySetResult(path);
                }
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
            }

            if (queue.Count > 0)
            {
                queue.Dequeue();
                if (operation.CancelToken != null && operation.CancelToken.canceled)
                    await TaskUtils.Delay(0.0f);

                if (queue.Count > 0)
                    DequeueOperations();
                else
                    isDequeuing = false;
            }

            isDequeuing = false;
        }
    }
}
