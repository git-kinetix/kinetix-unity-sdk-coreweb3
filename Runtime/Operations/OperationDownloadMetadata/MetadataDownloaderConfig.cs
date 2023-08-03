namespace Kinetix.Internal
{
    public class MetadataDownloaderConfig : OperationConfig
    {
        public readonly string Url;
        public readonly string ApiKey;

        public MetadataDownloaderConfig(string _Url, string _ApiKey)
        {
            Url    = _Url;
            ApiKey = _ApiKey;
        }
    }
}
