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
        public readonly KinetixEmote kinetixEmote;
        private         Texture2D    texture2D;
        public          TokenCancel  CancelToken;

        public OperationIconDownloader(KinetixEmote _KinetixEmote, TokenCancel cancelToken = null)
        {
            kinetixEmote = _KinetixEmote;
            CancelToken  = cancelToken;
        }

        public override async Task<Texture2D> Execute()
        {
            if (ProgressStatus == EProgressStatus.NONE)
            {
                if (kinetixEmote == null)
                    return null;

                if (!kinetixEmote.HasMetadata())
                    return null;

                string url = kinetixEmote.Metadata.IconeURL;
                if (string.IsNullOrEmpty(url))
                    return null;
                
                try
                {
                    Task<Texture2D> task = DownloadIconTexture(url);
                    ProgressStatus = EProgressStatus.PENDING;
                    Task           = task;
                    texture2D      = await task;

                    texture2D      = await ProcessDownsampling(texture2D);
                    ProgressStatus = EProgressStatus.COMPLETED;
                    return texture2D;
                }
                catch (OperationCanceledException e)
                {
                    ProgressStatus = EProgressStatus.NONE;
                    throw e;
                }
                catch (Exception e)
                {
                    ProgressStatus = EProgressStatus.COMPLETED;
                    throw e;
                }
            }

            if (ProgressStatus != EProgressStatus.COMPLETED)
            {
                texture2D = await Task;
                texture2D = await ProcessDownsampling(texture2D);
                return texture2D;
            }

            return texture2D;
        }

        private async Task<Texture2D> DownloadIconTexture(string _URL)
        {
            TaskCompletionSource<Texture2D> tcs = new TaskCompletionSource<Texture2D>();
            WebRequestHandler.Instance.GetTexture(_URL,
                (downloadedTexture2D) =>
                {
                    
                    
                    tcs.TrySetResult(downloadedTexture2D);
                },
                (e) =>
                {
                    if (CancelToken.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                        return;
                    }
                    tcs.TrySetException(e);
                }, CancelToken);

            return await tcs.Task;
        }

        private async Task<Texture2D> ProcessDownsampling(Texture2D _Texture2D)
        {
            _Texture2D      = await DownsampleTexture(_Texture2D, 5);
            _Texture2D.name = "Texture_" + kinetixEmote.Metadata.Name;
            _Texture2D.Apply();
            return _Texture2D;
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
