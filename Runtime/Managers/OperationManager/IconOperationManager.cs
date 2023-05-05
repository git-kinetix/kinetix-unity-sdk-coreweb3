using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Kinetix.Utils;
using UnityEngine;

namespace Kinetix.Internal
{
    public static class IconOperationManager
    {
        private static Queue<(OperationIconDownloader, TaskCompletionSource<Texture2D>)> queue;
        private static bool                                                              isProcessing;

        public static async Task<Texture2D> DownloadTexture(string _URL, TokenCancel cancelToken = null)
        {
            queue                         ??= new Queue<(OperationIconDownloader, TaskCompletionSource<Texture2D>)>();

            TaskCompletionSource<Texture2D> tcs            = new TaskCompletionSource<Texture2D>();
            OperationIconDownloader         iconDownloader = new OperationIconDownloader(_URL, cancelToken);
            queue.Enqueue((iconDownloader, tcs));

            if (!isProcessing)
                DequeueOperations();

            Texture2D toReturn = null;
            try
            {
                toReturn = await tcs.Task;
            }
            catch (TaskCanceledException e)
            {
                UnityEngine.Object.DestroyImmediate(toReturn);
                throw e;
            }
            
            return toReturn;
        }

        private static async void DequeueOperations()
        {
            if (queue.Count == 0)
            {
                isProcessing = false;
                return;
            }
            
            isProcessing                                                             = true;
            (OperationIconDownloader operation, TaskCompletionSource<Texture2D> tcs) = queue.Dequeue();
            if (tcs.Task.IsCanceled)
            {
                DequeueOperations();
                return;
            }
            
            Texture2D textureIcon = null;
            try
            {
                textureIcon = await operation.Execute();
            }
            catch (TaskCanceledException)
            {
                isProcessing = false;
                UnityEngine.Object.DestroyImmediate(textureIcon);
                tcs.TrySetCanceled();
                await TaskUtils.Delay(0.0f);
                DequeueOperations();
                return;
            }
            catch (Exception e)
            {
                isProcessing = false;
                tcs.TrySetException(e);
                await TaskUtils.Delay(0.0f);
                DequeueOperations();
                return;
            }

            tcs.TrySetResult(textureIcon);

            if (queue.Count > 0)
            {
                await TaskUtils.Delay(0.0f);
                DequeueOperations();
            }
            
            isProcessing = false;
        }
    }
}
