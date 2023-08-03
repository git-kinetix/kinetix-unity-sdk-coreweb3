// // ----------------------------------------------------------------------------
// // <copyright file="WebRequestNew.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Kinetix.Utils
{
    internal class WebRequestDispatcher
    {
        // TODO Change this with error refacto
        public const string NO_NETWORK_ERROR = "No network"; 
        public const string ERROR_RECEIVED = "Error"; 
        public const int TIMEOUT = 240;

        public Action<float> ProgressChanged;

        public enum HttpMethod
        {
            GET,
            POST,
            PUT,
            PATCH,
            DELETE
        }

        /// Sends a request using the method (GET, POST, etc), Headers and Playload wanted.
        /// Main method for any web request
        public async Task<T> SendRequest<T>(string _Url, HttpMethod _HttpMethod, Dictionary<string, string> _Headers = null, string _Payload = null, DownloadHandler _DownloadHandler = default, CancellationToken _Ctx = new CancellationToken()) where T : IResponse, new()
        {
            var response = new T();

            // If we don't have access to the almighty internet, return an error
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                response.Error = NO_NETWORK_ERROR;

                return response;
            }

            // Just prepare the request itself
            using var request = new UnityWebRequest();
            request.timeout = TIMEOUT;
            request.url = FixURL(_Url);
            request.method = _HttpMethod.ToString();

            if (_Headers != null)
            {
                foreach (KeyValuePair<string, string> header in _Headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }
            
            // If no download handler was provided, instantiate one
            _DownloadHandler ??= new DownloadHandlerBuffer();

            request.downloadHandler = _DownloadHandler;

            if (!string.IsNullOrEmpty(_Payload))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(_Payload);
                request.uploadHandler = new UploadHandlerRaw(bytes);
            }

            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();

            while (!asyncOperation.isDone && !_Ctx.IsCancellationRequested)
            {
                await Task.Yield();
                ProgressChanged?.Invoke(request.downloadProgress);
            }

            
            response.ResponseCode = request.responseCode;

            if (_Ctx.IsCancellationRequested)
            {
                request.Abort();
                response.Error = ERROR_RECEIVED;
                return response;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                response.Error = request.error;
                return response;
            }

            response.IsSuccess = true;
            response.Parse(request);

            return response;
        }

        /// <summary>
        /// Url can't have \ in it.<br/>
        /// When you use System.Path on windows they add \<br/>
        /// So we replace them by /
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public static string FixURL(string baseUrl) => baseUrl?.Replace('\\', '/');
    }
}
