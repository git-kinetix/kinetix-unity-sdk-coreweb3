using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Kinetix.Internal
{
    public class OperationOrchestrator
    {
        private readonly OperationRegistry                    registry;
        private readonly Dictionary<Type, OperationSequencer> operationsSequencer;

        public OperationOrchestrator()
        {
            registry            = new OperationRegistry();
            operationsSequencer = new Dictionary<Type, OperationSequencer>();
        }

        public Task<TResponse> RequestExecution<TConfig, TResponse>(IOperation<TConfig, TResponse> _Operation, CancellationTokenSource _CancellationTokenSource)
            where TConfig : OperationConfig
            where TResponse : OperationResponse
        {
            if (_CancellationTokenSource.IsCancellationRequested)
            {
                TaskCompletionSource<TResponse> tcs = new TaskCompletionSource<TResponse>();
                tcs.SetCanceled();
                return tcs.Task;
            }
            
            registry.Register(_Operation, _CancellationTokenSource);
            IOperationBatch<TConfig, TResponse> operationBatch = registry.GetOperationBatch(_Operation);
            Enqueue(operationBatch);
            return _Operation.CurrentTaskCompletionSource.Task;

        }

        private void Enqueue<TConfig, TResponse>(IOperationBatch<TConfig, TResponse> _Operation)
            where TConfig : OperationConfig
            where TResponse : OperationResponse
        {
            Type operationBatchType = _Operation.GetType();

            if (!operationsSequencer.ContainsKey(operationBatchType))
                operationsSequencer.Add(operationBatchType, new OperationSequencer());
            
            operationsSequencer[operationBatchType].Enqueue(_Operation);
        }
    }
}
