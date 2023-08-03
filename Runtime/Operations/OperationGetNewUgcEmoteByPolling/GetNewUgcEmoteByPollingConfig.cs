using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kinetix.Internal
{
    public class GetNewUgcEmoteByPollingConfig : OperationConfig
    {
        public readonly string Url;
        public readonly string ApiKey;

        public readonly float TotalTimeInSeconds;
        public readonly float IntervalTimeInSeconds;
        
        public GetNewUgcEmoteByPollingConfig(string _Url, string _ApiKey, float _TotalTimeInSeconds, float _IntervalTimeInSeconds)
        {
            Url    = _Url;
            ApiKey = _ApiKey;

            TotalTimeInSeconds    = _TotalTimeInSeconds;
            IntervalTimeInSeconds = _IntervalTimeInSeconds;
        }
    }
}
