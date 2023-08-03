using System.Collections.Generic;
using System.Threading.Tasks;
using Kinetix.Utils;

namespace Kinetix.Internal
{
    public class GetRawAPIResult : Operation<GetRawAPIResultConfig, GetRawAPIResultResponse>
    {
        public GetRawAPIResult(GetRawAPIResultConfig _Config): base(_Config) {}

        public override async Task Execute()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Content-type", "application/json" },
                { "Accept", "application/json" },
                { "x-api-key", Config.ApiKey }
            };

            WebRequestDispatcher request = new WebRequestDispatcher();

            RawResponse response = await request.SendRequest<RawResponse>(Config.Url, WebRequestDispatcher.HttpMethod.GET, headers);

            string json = response.Content;

            GetRawAPIResultResponse apiResponse = new GetRawAPIResultResponse
            {
                json = json
            };

            CurrentTaskCompletionSource.SetResult(apiResponse);
        }

        public override bool Compare(GetRawAPIResultConfig _Config)
        {
            return Config.Url.Equals(_Config.Url);
        }

        public override IOperation<GetRawAPIResultConfig, GetRawAPIResultResponse> Clone()
        {
            return new GetRawAPIResult(Config);
        }
    }
}
