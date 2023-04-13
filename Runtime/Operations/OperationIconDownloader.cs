using System;
using System.Threading.Tasks;
using System.Threading;
using Kinetix.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kinetix.Internal
{
    public class OperationIconDownloader : OperationAsync<Texture2D>
    {
        public readonly string    url;
        private         Texture2D texture2D;
        private TokenCancel cancelToken;

        public OperationIconDownloader(string _URL, TokenCancel cancelToken = null)
        {
            url = _URL;
            this.cancelToken = cancelToken;
        }

        public override async Task<Texture2D> Execute()
        {
            if (ProgressStatus == EProgressStatus.NONE)
            {
                if (string.IsNullOrEmpty(url))
                    return null;

                Task<Texture2D> task = DownloadIconTexture(url);

                ProgressStatus = EProgressStatus.PENDING;
                Task           = task;

                try
                {
                    texture2D      = await task;
                    texture2D      = await DownsampleTexture(texture2D, 5);
                    texture2D.name = url;
                    texture2D.Apply();
                }
                catch (TaskCanceledException e)
                {
                    Object.DestroyImmediate(texture2D);
                    throw e;
                }

                ProgressStatus = EProgressStatus.COMPLETED;
                return texture2D;
            }

            if (ProgressStatus == EProgressStatus.PENDING)
            {
                try
                {
                    texture2D = await Task;
                }
                catch (TaskCanceledException e)
                {
                    Object.DestroyImmediate(texture2D);
                    throw e;
                }

                return texture2D;
            }

            return texture2D;
        }

        private Task<Texture2D> DownloadIconTexture(string _URL)
        {
            TaskCompletionSource<Texture2D> tcs = new TaskCompletionSource<Texture2D>();
            WebRequestHandler.Instance.GetTexture(_URL, (downloadedTexture2D) =>
                {
                    tcs.TrySetResult(downloadedTexture2D);
                },
                (e) =>
                {
                    tcs.TrySetException(e);
                }, cancelToken);

            return tcs.Task;
        }

        private static async Task<Texture2D> DownsampleTexture(Texture2D sourceTexture, int ratio)
        {
            int sourceWidth  = sourceTexture.width;
            int sourceHeight = sourceTexture.height;
            int targetWidth  = sourceWidth / ratio;
            int targetHeight = sourceHeight / ratio;

            Color[] sourcePixels = sourceTexture.GetPixels();
            Color[] targetPixels = new Color[targetWidth * targetHeight];

            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    int sourceIndex = (y * ratio) * sourceWidth + (x * ratio);
                    int targetIndex = y * targetWidth + x;

                    Color c0 = sourcePixels[sourceIndex];
                    Color c1 = sourcePixels[sourceIndex + 1];
                    Color c2 = sourcePixels[sourceIndex + sourceWidth];
                    Color c3 = sourcePixels[sourceIndex + sourceWidth + 1];

                    Color averageColor = (c0 + c1 + c2 + c3) / 4f;
                    targetPixels[targetIndex] = averageColor;
                }

                if (y % 10 == 0)
                {
                    await TaskUtils.Delay(0.0f);
                }
            }

            Texture2D targetTexture = new Texture2D(targetWidth, targetHeight, sourceTexture.format, false);
            targetTexture.SetPixels(targetPixels);
            targetTexture.Apply();
            Object.DestroyImmediate(sourceTexture);

            return targetTexture;
        }


    }
}

