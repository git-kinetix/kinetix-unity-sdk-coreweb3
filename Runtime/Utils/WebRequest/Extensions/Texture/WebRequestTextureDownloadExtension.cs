using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Kinetix.Utils
{
    public static class WebRequestTextureDownloadExtension
    {
        internal static async Task<TextureResponse> GetTexture(this WebRequestDispatcher _WebRequest, string _Url, CancellationToken _Ctx = new CancellationToken(), int _Timeout = WebRequestDispatcher.TIMEOUT)
        {
            return await _WebRequest.SendRequest<TextureResponse>(_Url, WebRequestDispatcher.HttpMethod.GET, _DownloadHandler: new DownloadHandlerTexture(),
                _Ctx: _Ctx);
        }
    }
}
