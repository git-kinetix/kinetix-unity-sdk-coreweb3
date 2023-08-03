using System;
using System.Threading.Tasks;
using Kinetix.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kinetix.Internal
{
    public class IconDownloader : Operation<IconDownloaderConfig, IconDownloaderResponse>
    {
        private const int COMPUTE_DOWNSAMPLE_EXECUTION_COUNT_BEFORE_YIELD = 40;
        
        public IconDownloader(IconDownloaderConfig _Config) : base(_Config)
        {
        }

        public override async Task Execute()
        {
            WebRequestDispatcher request = new WebRequestDispatcher();
            
            TextureResponse response = await request.GetTexture(Config.Url, CancellationTokenSource.Token);
            
            if (CancellationTokenSource.Token.IsCancellationRequested)
            {
                CurrentTaskCompletionSource.SetCanceled();
                return;
            }

            if (!response.IsSuccess)
            {
                CurrentTaskCompletionSource.SetException(new Exception(response.Error));
                return;
            }
            
            Texture2D downSampleTexture = await ProcessDownsampling(response.Content);
            
            
            if (CancellationTokenSource.Token.IsCancellationRequested)
            {
                Object.Destroy(downSampleTexture);
                CurrentTaskCompletionSource.SetCanceled();
                return;
            }
            
            IconDownloaderResponse iconDownloaderResponse = new IconDownloaderResponse
            {
                texture = downSampleTexture
            };
            

            CurrentTaskCompletionSource.SetResult(iconDownloaderResponse);
        }

        public override bool Compare(IconDownloaderConfig _Config)
        {
            return Config.Url.Equals(_Config.Url);
        }

        public override IOperation<IconDownloaderConfig, IconDownloaderResponse> Clone()
        {
            return new IconDownloader(Config);
        }
        
        private async Task<Texture2D> ProcessDownsampling(Texture2D _Texture2D)
        {
            _Texture2D      = await DownsampleTexture(_Texture2D, 5);
            //_Texture2D.name = "Texture_" + kinetixEmote.Metadata.Name;
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

                if (y % COMPUTE_DOWNSAMPLE_EXECUTION_COUNT_BEFORE_YIELD == 0)
                {
                    await Task.Yield();
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
