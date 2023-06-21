// // ----------------------------------------------------------------------------
// // <copyright file="WebRequestHandler.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kinetix.Internal;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace Kinetix.Utils
{
    public class WebRequestHandler : MonoBehaviour
    {
        private static WebRequestHandler instance;
        private Dictionary<string, int> currentPollsCallsLeft;
        
        public const string ERROR_RECEIVED = "ERROR"; 

        
        public static WebRequestHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("[Kinetix] WebRequestHandler")
                    {
                        hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector
                    };
                    instance = obj.AddComponent<WebRequestHandler>();
                    DontDestroyOnLoad(obj);
                }

                return instance;
            }
        }

        public class JsonWebResponse
        {
            public long responseCode;
        }
        
        public async Task<T> GetAsync<T>(string endPoint, KeyValuePair<string, string>[] query)
            where T : JsonWebResponse, new()
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            return await GetAsync(endPoint, query, tcs);
        }
        
        private async Task<T> GetAsync<T>(string endPoint, KeyValuePair<string, string>[] query, TaskCompletionSource<T> _Tcs,float _Delay = 0.0f, int _Tries = 0)
            where T : JsonWebResponse, new()
        {
            await TaskUtils.Delay(_Delay);

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

            Uri             uri = new Uri(endPoint + "?" + queryString);
            UnityWebRequest www = UnityWebRequest.Get(uri);

            www.SetRequestHeader("accept", "application/json");
            UnityWebRequestAsyncOperation asyncOp = www.SendWebRequest();


            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            asyncOp.completed += (op) =>
            {
                tcs.SetResult(true);
            };

            await tcs.Task;

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError ||
                www.result == UnityWebRequest.Result.DataProcessingError)
            {
                _Tries++;
                if (_Tries == 3)
                {
                    www.Dispose();
                    _Tcs.SetException(new Exception("Web Request timed out"));
                }
                else
                {
                    float maxDelay = Mathf.Clamp(_Delay + 1.0f, 0.0f, 5.0f) + (Random.value * 0.25f);
                    www.Dispose();
                    await GetAsync(endPoint, query, _Tcs, maxDelay, _Tries);
                }
            }
            else
            {
#if DEV_KINETIX
                Debug.Log("Response : " + www.downloadHandler.text);
#endif
                T resultObject = JsonUtility.FromJson<T>(www.downloadHandler.text);
                resultObject.responseCode = www.responseCode;
                www.Dispose();
                _Tcs.SetResult(resultObject);
            }
            
            return await _Tcs.Task;
        }

        public async Task<bool> PostAsyncRaw(string endPoint, KeyValuePair<string, string>[] headers, string jsonBody)
        {
            var    tcs         = new TaskCompletionSource<bool>();
            Uri uri = new Uri(endPoint);

            using (UnityWebRequest www = new UnityWebRequest(uri, "POST"))
            {
                if (jsonBody != string.Empty)
                {
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                    www.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
                }
    
                www.SetRequestHeader("accept", "application/json");
                www.SetRequestHeader("Content-Type", "application/json");

                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> keyValuePair in headers)
                    {
                        www.SetRequestHeader(keyValuePair.Key, keyValuePair.Value);
                    }
                }

                www.SendWebRequest();
                
                while (!www.isDone)
                {
                    await Task.Yield();
                }

                if (www.result == UnityWebRequest.Result.ConnectionError ||
                    www.result == UnityWebRequest.Result.ProtocolError ||
                    www.result == UnityWebRequest.Result.DataProcessingError)
                {
                    tcs.SetException(new Exception(www.error));
                }
                else
                {
                    tcs.SetResult(true);
                }
            }
            return await tcs.Task;
        }
        

        public async Task<string> GetAsyncRaw(string endPoint, KeyValuePair<string, string>[] headers, KeyValuePair<string, string>[] query, string errorFallbackText = "")
        {
            var    tcs         = new TaskCompletionSource<string>();
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

            if (queryString != string.Empty) {
                queryString = "?" + queryString;
            }

            Uri uri = new Uri(endPoint + queryString);
            using (UnityWebRequest www = UnityWebRequest.Get(uri))
            {
                www.SetRequestHeader("accept", "application/json");

                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> keyValuePair in headers)
                    {
                        www.SetRequestHeader(keyValuePair.Key, keyValuePair.Value);
                    }
                }

                www.SendWebRequest();
                
                while (!www.isDone)
                {
                    await Task.Yield();
                }

                if (www.result == UnityWebRequest.Result.ConnectionError ||
                    www.result == UnityWebRequest.Result.ProtocolError ||
                    www.result == UnityWebRequest.Result.DataProcessingError)
                {
                    tcs.SetResult(errorFallbackText);
                }
                else
                {
                    tcs.SetResult(www.downloadHandler.text);
                }
            }
            return await tcs.Task;
        }

        public async Task<string> GetAsyncRaw(string endPoint, KeyValuePair<string, string>[] query)
        {
            return await GetAsyncRaw(endPoint, null, query);
        }

        public void GetFile(string _URL, string _FilePath, SequencerCancel _CancelToken, Action _OnSuccess, Action _OnFailure)
        {
            if (string.IsNullOrWhiteSpace(_URL))
            {
                KinetixDebug.LogException(new ArgumentException($"'{nameof(_URL)}' can't be null, empty or whitespace.", nameof(_URL)));
                _OnFailure?.Invoke();
                return;
            }

            StartCoroutine(GetFileAsync(_URL, _FilePath, _CancelToken, _OnSuccess, _OnFailure));
        }

        private IEnumerator GetFileAsync(string _URL, string _FilePath, SequencerCancel _CancelToken, Action _OnSuccess, Action _OnFailure)
        {
            _URL = FixURL(_URL);
            Uri uri = new Uri(_URL);

            using (UnityWebRequest www = UnityWebRequest.Get(uri))
            {
                DownloadHandlerFile downloadHandlerFile = new DownloadHandlerFile(_FilePath);
                downloadHandlerFile.removeFileOnAbort = true;
                www.downloadHandler                   = downloadHandlerFile;

                www.SendWebRequest();

                while (!www.isDone)
                {
                    if (_CancelToken != null && _CancelToken.canceled)
                    {
                        www.Abort();
                        www.Dispose();
                        _OnFailure?.Invoke();
                        yield break;
                    }
                    yield return null;
                }

                if (www.result == UnityWebRequest.Result.ConnectionError ||
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    _OnFailure?.Invoke();
                }
                else
                {
                    _OnSuccess?.Invoke();
                }
            }
        }

        public void GetTexture(string _URL, Action<Texture2D> _OnSuccess, Action<Exception> _OnFailure, TokenCancel cancelToken = null)
        {
            if (string.IsNullOrWhiteSpace(_URL))
                _OnFailure.Invoke(new ArgumentException($"'{nameof(_URL)}' can't be null, empty or whitespace.", nameof(_URL)));

            StartCoroutine(GetTextureAsync(_URL, _OnSuccess, _OnFailure, cancelToken));
        }

        private IEnumerator GetTextureAsync(string _URL, Action<Texture2D> _OnSuccess, Action<Exception> _OnFailure, TokenCancel cancelToken = null)
        {
            _URL = FixURL(_URL);
            Uri uri = new Uri(_URL);

            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri))
            {
                www.SendWebRequest();
                while (!www.isDone)
                {
                    if(cancelToken != null && cancelToken.IsCanceled)
                    {
                        www.Abort();
                        www.Dispose();
                        _OnFailure?.Invoke(new TaskCanceledException("Cancelled download icon"));
                        yield break;
                    }
                    yield return null;
                }

                if (www.result == UnityWebRequest.Result.ConnectionError ||
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    _OnFailure?.Invoke(new Exception("Web Request failed to download icon"));
                }
                else
                {
                    Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    _OnSuccess?.Invoke(texture);
                }


            }
        }

        public async Task<string> PollUrl(string url, Func<string, bool> completionCondition, int nbRequests = 10, float requestTimeout = 300)
        {
            return await PollUrl(url, null, completionCondition, nbRequests, requestTimeout);
        }

        public async Task<string> PollUrl(string url, KeyValuePair<string, string>[] headers, Func<string, bool> completionCondition, int nbRequests = 10, float requestTimeout = 300)
        {
            currentPollsCallsLeft ??= new Dictionary<string, int>();

            if (currentPollsCallsLeft.ContainsKey(url)) {
                currentPollsCallsLeft[url] = nbRequests;
                return null;
            }

            currentPollsCallsLeft[url] = nbRequests;

            for (int i = currentPollsCallsLeft[url]; i > 0 ; i = currentPollsCallsLeft[url])
            {
                currentPollsCallsLeft[url] --;
                
                try
                {
                    var task = GetAsyncRaw(url, headers, null, ERROR_RECEIVED);
                    
                    
                    if (await Task.WhenAny(task, TaskUtils.Delay((int)(requestTimeout))) == task && !string.IsNullOrEmpty(task.Result) && completionCondition(task.Result))
                    {
                        currentPollsCallsLeft.Remove(url);
                        return task.Result;
                    }
                }
                catch (Exception e)
                {
                    KinetixDebug.LogError($"Error during request {i}: {e.Message}");
                }
                
                // Wait for timeout more and more
                await TaskUtils.Delay(requestTimeout / nbRequests);
            }


            currentPollsCallsLeft.Remove(url);

            return null;
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
