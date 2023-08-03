using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kinetix.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Kinetix.Internal
{
    public class GetNewUgcTokenByPolling : Operation<GetNewUgcTokenByPollingConfig, GetNewUgcTokenByPollingResponse>
    {
        public GetNewUgcTokenByPolling(GetNewUgcTokenByPollingConfig config) : base(config)
        {
        }

        public override async Task Execute()
        {
            int totalTries = Mathf.CeilToInt(Config.TotalTimeInSeconds / Config.IntervalTimeInSeconds);

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Content-type", "application/json" },
                { "Accept", "application/json" },
                { "x-api-key", Config.ApiKey }
            };

            WebRequestDispatcher webRequest = new WebRequestDispatcher();
            for (int i = 0; i < totalTries; i++)
            {
                if (CancellationTokenSource.IsCancellationRequested)
                {
                    CurrentTaskCompletionSource.TrySetCanceled();
                    return;
                }

                RawResponse response = await webRequest.SendRequest<RawResponse>(Config.Url,
                    WebRequestDispatcher.HttpMethod.GET,
                    headers, null, default, CancellationTokenSource.Token);

                if (CancellationTokenSource.IsCancellationRequested)
                {
                    CurrentTaskCompletionSource.TrySetCanceled();
                    return;
                }
                string jsonResult = response.Content;

                if (response.IsSuccess)
                {
                    if (string.IsNullOrEmpty(jsonResult))
                    {
                        GetNewUgcTokenByPollingResponse ugcResponse = new GetNewUgcTokenByPollingResponse
                        {
                            IsTokenOutdated = true
                        };
                        CurrentTaskCompletionSource.SetResult(ugcResponse);
                        return;
                    }
                    
                    SdkTokenValidityResult tokenValidity = JsonConvert.DeserializeObject<SdkTokenValidityResult>(response.Content);
                    if (tokenValidity != null && tokenValidity.expireIn < 1)
                    {
                        GetNewUgcTokenByPollingResponse ugcResponse = new GetNewUgcTokenByPollingResponse
                        {
                            IsTokenOutdated = true
                        };
                        CurrentTaskCompletionSource.SetResult(ugcResponse);
                        return;
                    }
                }
                else
                {
                    GetNewUgcTokenByPollingResponse ugcResponse = new GetNewUgcTokenByPollingResponse
                    {
                        IsTokenOutdated = true
                    };
                    CurrentTaskCompletionSource.SetResult(ugcResponse);
                    return;
                }

                await TaskUtils.Delay(Config.IntervalTimeInSeconds);
            }

            CurrentTaskCompletionSource.TrySetException(new TimeoutException());
        }

        public override bool Compare(GetNewUgcTokenByPollingConfig _Config)
        {
            return _Config.Url == Config.Url;
        }

        public override IOperation<GetNewUgcTokenByPollingConfig, GetNewUgcTokenByPollingResponse> Clone()
        {
            return new GetNewUgcTokenByPolling(Config);
        }
    }
}
