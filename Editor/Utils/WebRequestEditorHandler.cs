using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Kinetix.Internal
{
    internal static class WebRequestEditorHandler
    {
        internal static async Task<bool> GetFileAsync(string url, string filePath)
        {
            var tcs = new TaskCompletionSource<bool>();
            Uri uri = new Uri(url);

            using (UnityWebRequest www = UnityWebRequest.Get(uri))
            {
                DownloadHandlerFile downloadHandlerFile = new DownloadHandlerFile(filePath);
                downloadHandlerFile.removeFileOnAbort = true;
                www.downloadHandler = downloadHandlerFile;

                www.SendWebRequest();

                while (!www.isDone)
                {
                    await Task.Yield();
                }

                if (www.result == UnityWebRequest.Result.ConnectionError ||
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    tcs.SetResult(false);
                }
                else
                {
                    tcs.SetResult(true);
                }
            }

            return await tcs.Task;
        }

        internal static async Task<string> GetAsyncRaw(string endPoint, KeyValuePair<string, string>[] query = null)
        {
            var tcs = new TaskCompletionSource<string>();
            string queryString = string.Empty;

            if (query != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in query)
                {
                    if (queryString.Length > 0)
                        queryString += "&";

                    queryString += keyValuePair.Key + "=" + UnityWebRequest.EscapeURL(keyValuePair.Value);
                }
            }
            else
            {
                queryString = "";
            }

            Uri uri = new Uri(endPoint + "?" + queryString);
            using (UnityWebRequest www = UnityWebRequest.Get(uri))
            {
                www.SetRequestHeader("accept", "application/json");

                www.SendWebRequest();
                
                while (!www.isDone)
                {
                    await Task.Yield();
                }


                if (www.result == UnityWebRequest.Result.ConnectionError ||
                    www.result == UnityWebRequest.Result.ProtocolError ||
                    www.result == UnityWebRequest.Result.DataProcessingError)
                {
                    tcs.SetResult(www.error);
                }
                else
                {
                    tcs.SetResult(www.downloadHandler.text);
                }
            }

            return await tcs.Task;
        }
    }    
}
