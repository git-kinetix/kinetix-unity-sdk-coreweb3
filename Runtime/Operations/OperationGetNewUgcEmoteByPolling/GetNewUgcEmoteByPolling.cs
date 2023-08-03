using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kinetix.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Kinetix.Internal
{
    public class GetNewUgcEmoteByPolling : Operation<GetNewUgcEmoteByPollingConfig, GetNewUgcEmoteByPollingResponse>
    {
        public GetNewUgcEmoteByPolling(GetNewUgcEmoteByPollingConfig forNewUgcEmoteConfig) : base(forNewUgcEmoteConfig)
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
                
                RawResponse response = await webRequest.SendRequest<RawResponse>(Config.Url, WebRequestDispatcher.HttpMethod.GET,
                    headers, null, default, CancellationTokenSource.Token);

                if (CancellationTokenSource.IsCancellationRequested)
                {
                    CurrentTaskCompletionSource.TrySetCanceled();
                    return;
                }
                
                string json = response.Content;
                if (response.IsSuccess && json != string.Empty)
                {

                    // Then try getting the emotes again
                    SdkApiUserAsset[] collection = JsonConvert.DeserializeObject<SdkApiUserAsset[]>(json);

                    if (collection.Length > KinetixCoreBehaviour.ManagerLocator.Get<AccountManager>().LoggedAccount.Emotes.ToArray().Length)
                    {
                        AnimationMetadata newEmoteMetadata = collection[collection.Length - 1].ToAnimationMetadata();
                        GetNewUgcEmoteByPollingResponse result = new GetNewUgcEmoteByPollingResponse()
                        {
                            newAnimationMetadata = newEmoteMetadata
                        };

                        CurrentTaskCompletionSource.TrySetResult(result);
                        return;
                    }
                }

                await TaskUtils.Delay(Config.IntervalTimeInSeconds);
            }

            CurrentTaskCompletionSource.TrySetException(new TimeoutException());
        }

        public override bool Compare(GetNewUgcEmoteByPollingConfig forNewUgcEmoteByPollingConfig)
        {
            return Config.Url == forNewUgcEmoteByPollingConfig.Url;
        }

        public override IOperation<GetNewUgcEmoteByPollingConfig, GetNewUgcEmoteByPollingResponse> Clone()
        {
            return new GetNewUgcEmoteByPolling(Config);
        }
    }
}
