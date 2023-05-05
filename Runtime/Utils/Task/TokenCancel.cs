using System;
using System.Threading.Tasks;

namespace Kinetix.Utils
{
    public class TokenCancel
    {
        private Func<bool> internalOnCancel;
        private bool _isCanceled = false;
        public bool IsCanceled => _isCanceled;

        public void Cancel() 
        { 
            _isCanceled = true;
            internalOnCancel?.Invoke();
            Dispose();
        }
        public void Dispose() => internalOnCancel = null;
        public void Register<T>(TaskCompletionSource<T> tcs) { internalOnCancel += tcs.TrySetCanceled; }
    }    
}
