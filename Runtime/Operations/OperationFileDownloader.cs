using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kinetix.Utils;
using UnityEngine;

namespace Kinetix.Internal
{
    public class OperationFileDownloader : OperationAsync<string>
    {
        private readonly KinetixEmote kinetixEmote;
        private          string       path;
        
        public OperationFileDownloader(KinetixEmote _KinetixEmote)
        {
            kinetixEmote = _KinetixEmote;
        }
        
        public override async Task<string> Execute()
        {
            if (ProgressStatus == EProgressStatus.NONE)
            {
                if (kinetixEmote == null)
                    return "";
                if (!kinetixEmote.HasMetadata())
                    return "";
                if (string.IsNullOrEmpty(kinetixEmote.Metadata.AnimationURL))
                    return "";
                
                Task<string> task = KinetixDownloader.DownloadAndCacheGLB(kinetixEmote.Ids.UUID, kinetixEmote.Metadata.AnimationURL);

                ProgressStatus = EProgressStatus.PENDING;
                Task           = task;

                path           = await task;
                ProgressStatus = EProgressStatus.COMPLETED;
                return path;
            }

            if (ProgressStatus != EProgressStatus.COMPLETED)
            {
                path = await Task;
                return path;
            }
            
            return path;
        }
    }
}

