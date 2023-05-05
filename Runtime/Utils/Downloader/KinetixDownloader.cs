// // ----------------------------------------------------------------------------
// // <copyright file="KinetixDownloader.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kinetix.Internal;

namespace Kinetix.Utils
{
    public static class KinetixDownloader
    {
        private const string s_ExtensionFile = ".glb";
        
        /// <summary>
        /// Download GLB and cache it in persistant data directory
        /// </summary>
        public static async Task<string> DownloadAndCacheGLB(string _UUID, string _AnimationURL)
        {
            string filename = _UUID + s_ExtensionFile;
            string filePath = Path.Combine(PathConstants.CacheAnimationsPath, filename);
            
            if (File.Exists(filePath))
            {
                return filePath;
            }
            
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

            try
            {
                KinetixDebug.Log("Start Download : " + _AnimationURL);
                WebRequestHandler.Instance.GetFile(_AnimationURL, filePath, () =>
                {
                    if (!MemoryManager.FileExists(filePath))
                    {
                        throw new Exception("File does not exists after downloading GLB");
                    }
                    
                    KinetixDebug.Log("Downloaded : " + filePath);

                    KinetixAnalytics.SendEvent("Download_Animation", _UUID, KinetixAnalytics.Page.None, KinetixAnalytics.Event_type.Click);
                    
                    tcs.SetResult(filePath);
                }, () => throw new Exception("Failed downloading GLB"));
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }

            return await tcs.Task;
        }
    }
}
