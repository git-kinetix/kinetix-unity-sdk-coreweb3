using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Kinetix.Utils
{
    public static class WebRequestFileDownloadExtension
    {
        internal static async Task<FileResponse> GetFile(this WebRequestDispatcher _WebRequest, string _Url, string _Path, CancellationToken _Ctx = new CancellationToken(), int _Timeout = WebRequestDispatcher.TIMEOUT)
        {
            DownloadHandlerFile downloadHandler = new DownloadHandlerFile(_Path);
            downloadHandler.removeFileOnAbort = true;

            return await _WebRequest.SendRequest<FileResponse>(_Url, WebRequestDispatcher.HttpMethod.GET, _DownloadHandler: downloadHandler, _Ctx: _Ctx);
        }
    }    
}
