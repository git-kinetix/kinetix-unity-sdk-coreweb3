namespace Kinetix.Internal
{
    public class GetRawAPIResultConfig : OperationConfig
    {
        public readonly string Url;
        public readonly string ApiKey;

        public GetRawAPIResultConfig(string _Url, string _ApiKey)
        {
            Url    = _Url;
            ApiKey = _ApiKey;
        }
    }
}
