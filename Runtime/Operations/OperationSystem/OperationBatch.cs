using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Kinetix.Internal
{
    public class OperationBatch<TConfig, TResponse> : IOperationBatch<TConfig, TResponse>
        where TConfig : OperationConfig
        where TResponse : OperationResponse
    {
        public          Action                         OnCompleted;
        public readonly IOperation<TConfig, TResponse> MainOperation;

        private readonly CompositeTokenSource                 CompositeTokenSource;
        private readonly List<IOperation<TConfig, TResponse>> registeredOperations;

        public OperationBatch(IOperation<TConfig, TResponse> mainOperationClone)
        {
            CompositeTokenSource = new CompositeTokenSource();

            MainOperation                         = mainOperationClone;
            MainOperation.CancellationTokenSource = CompositeTokenSource.compositeTokenSource;

            registeredOperations = new List<IOperation<TConfig, TResponse>>();
        }

        public void RegisterOperation(IOperation<TConfig, TResponse> _Operation,
            CancellationTokenSource                                  _CancellationTokenSource)
        {
            CompositeTokenSource.AddTokenSource(_CancellationTokenSource);

            TaskCompletionSource<TResponse> tcs = new TaskCompletionSource<TResponse>();
            _Operation.CurrentTaskCompletionSource = tcs;
            _Operation.CancellationTokenSource     = _CancellationTokenSource;


            registeredOperations.Add(_Operation);


            MirrorTaskCompletionSource(MainOperation.CurrentTaskCompletionSource,
                _Operation.CurrentTaskCompletionSource, _CancellationTokenSource);
        }

        private async void MirrorTaskCompletionSource(TaskCompletionSource<TResponse> source1,
            TaskCompletionSource<TResponse> source2, CancellationTokenSource _CancellationTokenSource)
        {
            try
            {
                while (!source1.Task.IsCompleted && _CancellationTokenSource is { Token: { IsCancellationRequested: false } })
                    await Task.Yield();
                MirrorTCSResult(source1, source2, _CancellationTokenSource);

            }
            catch (ObjectDisposedException)
            {
                source2.TrySetCanceled();
            }
        }

        private void MirrorTCSResult(TaskCompletionSource<TResponse> source1,
            TaskCompletionSource<TResponse> source2, CancellationTokenSource _CancellationTokenSource)
        {
            if (source1.Task.IsFaulted)
            {
                SetOnCompleted();
                source2.TrySetException(source1.Task.Exception);
                return;
            }
            if (source1.Task.IsCanceled)
            {
                SetOnCompleted();
                source2.TrySetCanceled();
                return;
            }
            if (source1.Task.IsCompleted)
            {
                SetOnCompleted();
                source2.TrySetResult(source1.Task.Result);
                return;
            }
            
            if (_CancellationTokenSource.IsCancellationRequested)
            {
                if (CompositeTokenSource.Token.IsCancellationRequested)
                    OnCompleted?.Invoke();
                source2.TrySetCanceled();
            }
        }

        private void SetOnCompleted()
        {
            if (registeredOperations.Count(op => op.CurrentTaskCompletionSource.Task.IsCompleted) == registeredOperations.Count - 1)
                OnCompleted?.Invoke();
        }
        
        public void Dispose()
        {
            CompositeTokenSource?.Dispose();
            if (registeredOperations.Count(op => op.CurrentTaskCompletionSource.Task.IsCompleted) != registeredOperations.Count - 1)
                registeredOperations?.ForEach(op => op.CurrentTaskCompletionSource.TrySetCanceled());
        }
        
        public IOperation<TConfig, TResponse> GetOperation()
        {
            return MainOperation;
        }

        IOperation IOperationBatch.GetOperation()
        {
            return GetOperation();
        }
    }
}
