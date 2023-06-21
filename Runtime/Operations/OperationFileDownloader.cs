using System;
using System.Threading.Tasks;
using Kinetix.Utils;
using UnityEngine;

namespace Kinetix.Internal
{
    public class OperationFileDownloader : OperationAsync<string>
    {
        public  KinetixEmote    kinetixEmote;
        private string          path;
        public  SequencerCancel CancelToken;

        public OperationFileDownloader(KinetixEmote _KinetixEmote, SequencerCancel _CancelToken)
        {
            kinetixEmote = _KinetixEmote;
            CancelToken  = _CancelToken;
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

                try
                {
                    Task<string> task = KinetixDownloader.DownloadAndCacheGLB(kinetixEmote.Ids.UUID, kinetixEmote.Metadata.AnimationURL, CancelToken);
                    ProgressStatus = EProgressStatus.PENDING;
                    Task           = task;

                    path = await task;
                    ProgressStatus = EProgressStatus.COMPLETED;
                    return path;
                }
                catch (Exception e)
                {
                    ProgressStatus = EProgressStatus.NONE;
                    throw e;
                }
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
