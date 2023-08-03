using System.Collections.Generic;
using System.Threading.Tasks;
using Kinetix.Utils;
using UnityEngine;

namespace Kinetix.Internal
{
    public class MetadataDownloader : Operation<MetadataDownloaderConfig, MetadataDownloaderResponse>
    {
        public MetadataDownloader(MetadataDownloaderConfig _Config) : base(_Config)
        {
        }

        public override async Task Execute()
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                CurrentTaskCompletionSource.SetCanceled();
                return;
            }

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Content-type", "application/json" },
                { "Accept", "application/json" },
                { "x-api-key", Config.ApiKey }
            };

            WebRequestDispatcher webRequest = new WebRequestDispatcher();
            RawResponse response = await webRequest.SendRequest<RawResponse>(Config.Url, WebRequestDispatcher.HttpMethod.GET, headers);
            string jsonResponse = response.Content;

            if (CancellationTokenSource.IsCancellationRequested)
            {
                CurrentTaskCompletionSource.SetCanceled();
                return;
            }

            MetadataDownloaderResponse metadataDownloaderResponse = new MetadataDownloaderResponse
            {
                json = jsonResponse
            };

            CurrentTaskCompletionSource.SetResult(metadataDownloaderResponse);
        }

        public override bool Compare(MetadataDownloaderConfig _Config)
        {
            return Config.Url.Equals(_Config.Url);
        }

        public override IOperation<MetadataDownloaderConfig, MetadataDownloaderResponse> Clone()
        {
            return new MetadataDownloader(Config);
        }
    }
}
