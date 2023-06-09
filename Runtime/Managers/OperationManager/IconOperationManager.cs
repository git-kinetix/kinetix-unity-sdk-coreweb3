using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            queue ??= new Queue<(OperationIconDownloader, TaskCompletionSource<Texture2D>)>();
            Texture2D toReturn = null;

            TaskCompletionSource<Texture2D> tcs            = new TaskCompletionSource<Texture2D>();
            OperationIconDownloader         iconDownloader = new OperationIconDownloader(_URL, cancelToken);
            cancelToken?.Register(tcs);
            queue.Enqueue((iconDownloader, tcs));

            if (!isProcessing)
                DequeueOperations();

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

                List<(OperationIconDownloader, TaskCompletionSource<Texture2D>)> similarOperations = queue.ToList().FindAll(op => op.Item1.url == operation.url);
                List<(OperationIconDownloader, TaskCompletionSource<Texture2D>)> updatedQueue      = queue.ToList();

                updatedQueue.RemoveAll(op => op.Item1.url == operation.url);
                queue = new Queue<(OperationIconDownloader, TaskCompletionSource<Texture2D>)>(updatedQueue);

                while (similarOperations.Count > 0)
                {
                    similarOperations[0].Item2.TrySetResult(textureIcon);
                    similarOperations.RemoveAt(0);
                }
                
                tcs.TrySetResult(textureIcon);
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
            
            if (queue.Count > 0)
            {
                await TaskUtils.Delay(0.0f);
                DequeueOperations();
            }

            isProcessing = false;
        }
    }
}
