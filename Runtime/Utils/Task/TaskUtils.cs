using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Kinetix.Utils
{
    public static class TaskUtils
    {
        public static Task Delay(float seconds)
        {
            var tcs = new TaskCompletionSource<object>();
            CoroutineUtils.Instance.StartCoroutine(DelayCoroutine(seconds, tcs));
            return tcs.Task;
        }

        private static IEnumerator DelayCoroutine(float seconds, TaskCompletionSource<object> tcs)
        {
            yield return new WaitForSeconds(seconds);
            tcs.SetResult(null);
        }
    }
}
