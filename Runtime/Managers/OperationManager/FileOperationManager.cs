using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kinetix.Internal;
using Kinetix.Utils;
using UnityEngine;

public static class FileOperationManager
{
    private static Dictionary<AnimationIds, OperationFileDownloader>                  operationsFileDownloader;
    private static Queue<(OperationFileDownloader, TaskCompletionSource<string>)> queue;
    
    public static async Task<string> DownloadGLBByEmote(KinetixEmote _Emote)
    {
        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
        operationsFileDownloader ??= new Dictionary<AnimationIds, OperationFileDownloader>();
        queue                    ??= new Queue<(OperationFileDownloader, TaskCompletionSource<string>)>();

        if (!operationsFileDownloader.ContainsKey(_Emote.Ids))
        {
            OperationFileDownloader fileDownloader = new OperationFileDownloader(_Emote);
            operationsFileDownloader.Add(_Emote.Ids, fileDownloader);
            queue.Enqueue((fileDownloader, tcs));

            if (queue.Count == 1)
                DequeueOperations();
        }
        return await tcs.Task;
    }

    private static void ClearEmote(AnimationIds emoteIds)
    {
        if (operationsFileDownloader != null && operationsFileDownloader.ContainsKey(emoteIds)) {
            operationsFileDownloader.Remove(emoteIds);
        }
    }

    private static async void DequeueOperations()
    {
        (OperationFileDownloader operation, TaskCompletionSource<string> tcs) = queue.Peek();
        
        string path = await operation.Execute();

        ClearEmote(operation.kinetixEmote.Ids);
        
        tcs.SetResult(path);
        queue.Dequeue();

        if (queue.Count > 0)
        {
            await TaskUtils.Delay(0.0f);
            DequeueOperations();
        }
    }
}
