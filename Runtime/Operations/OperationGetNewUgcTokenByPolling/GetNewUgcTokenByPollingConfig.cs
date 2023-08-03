namespace Kinetix.Internal
{
    public class GetNewUgcTokenByPollingConfig : OperationConfig
    {
        public readonly string Url;
        public readonly string ApiKey;

        public readonly float TotalTimeInSeconds;
        public readonly float IntervalTimeInSeconds;
        
        public GetNewUgcTokenByPollingConfig(string _Url, string _ApiKey, float _TotalTimeInSeconds, float _IntervalTimeInSeconds)
        {
            Url    = _Url;
            ApiKey = _ApiKey;

            TotalTimeInSeconds    = _TotalTimeInSeconds;
            IntervalTimeInSeconds = _IntervalTimeInSeconds;
        }
    }
}
