namespace Kinetix.Internal
{
    public class FileDownloaderConfig : OperationConfig
    {
        public readonly string Url;
        public readonly string Path;

        public FileDownloaderConfig(string _Url, string _Path)
        {
            Url    = _Url;
            Path    = _Path;
        }
    }
}
