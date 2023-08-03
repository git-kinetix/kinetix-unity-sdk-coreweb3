using System;
using System.Threading.Tasks;
using Kinetix.Utils;

namespace Kinetix.Internal
{
    public class FileDownloader : Operation<FileDownloaderConfig, FileDownloaderResponse>
    {
        public FileDownloader(FileDownloaderConfig _Config): base(_Config) {}

        public override async Task Execute()
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                CurrentTaskCompletionSource.SetCanceled();
                return;
            }
            
            WebRequestDispatcher request = new WebRequestDispatcher();

            FileResponse response = await request.GetFile(Config.Url, Config.Path);

            if (CancellationTokenSource.IsCancellationRequested)
            {
                CurrentTaskCompletionSource.SetCanceled();
                return;
            }
            
            if (!response.IsSuccess)
            {
                CurrentTaskCompletionSource.SetException(new Exception("File not downloaded : " + Config.Url));
                return;
            }

            FileDownloaderResponse fileDownloaderResponse = new FileDownloaderResponse
            {
                path = Config.Path
            };

            CurrentTaskCompletionSource.SetResult(fileDownloaderResponse);
        }

        public override bool Compare(FileDownloaderConfig _Config)
        {
            return Config.Url.Equals(_Config.Url);
        }

        public override IOperation<FileDownloaderConfig, FileDownloaderResponse> Clone()
        {
            return new FileDownloader(Config);
        }
    }
}
