using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Kinetix.Internal
{
    public class EmoteRetargetedData
    {
        public Dictionary<Type, EmoteRetargetingClipResult> clipsByType;
        public SequencerCancel SequencerCancelToken;
        public float           SizeInBytes;
        public CancellationTokenSource CancellationTokenFileDownload;
        public EProgressStatus ProgressStatus;

        public EmoteRetargetedData()
        {
            clipsByType = new Dictionary<Type, EmoteRetargetingClipResult>();
        }
    }
}

