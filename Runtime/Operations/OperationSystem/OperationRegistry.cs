using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Kinetix.Internal
{
    public class OperationRegistry
    {
        private readonly List<IOperationBatch> operationsBatch;

        public OperationRegistry()
        {
            operationsBatch = new List<IOperationBatch>();
        }

        public void Register<TConfig, TResponse>(IOperation<TConfig, TResponse> _Operation, CancellationTokenSource _CancellationTokenSource)
            where TConfig : OperationConfig
            where TResponse : OperationResponse
        {
            SetBatchOperation(_Operation, _CancellationTokenSource);
        }

        public void Unregister<TConfig, TResponse>(IOperation<TConfig, TResponse> _Operation)
            where TConfig : OperationConfig
            where TResponse : OperationResponse
        {
            RemoveOperationBatch(_Operation);
        }

        public IOperationBatch<TConfig, TResponse> GetOperationBatch<TConfig, TResponse>(
            IOperation<TConfig, TResponse> _Operation)
            where TConfig : OperationConfig
            where TResponse : OperationResponse
        {
            return GetRegisteredOperationBatch(_Operation);
        }

        private void SetBatchOperation<TConfig, TResponse>(IOperation<TConfig, TResponse> _Operation, CancellationTokenSource _CancellationTokenSource)
            where TConfig : OperationConfig
            where TResponse : OperationResponse
        {
            if (!IsOperationRegistered(_Operation))
            {
                OperationBatch<TConfig, TResponse> opBatch = CreateOperationBatch(_Operation);
            }

            GetRegisteredOperationBatch(_Operation).RegisterOperation(_Operation, _CancellationTokenSource);
        }

        private OperationBatch<TConfig, TResponse> CreateOperationBatch<TConfig, TResponse>(IOperation<TConfig, TResponse> _Operation)
            where TConfig : OperationConfig
            where TResponse : OperationResponse
        {
            IOperation<TConfig, TResponse>  operationClone = _Operation.Clone();
            TaskCompletionSource<TResponse> tcs           = new TaskCompletionSource<TResponse>();
            operationClone.CurrentTaskCompletionSource = tcs;
            
            OperationBatch<TConfig, TResponse> operationBatch = new OperationBatch<TConfig, TResponse>(operationClone);
            operationBatch.OnCompleted += () => Unregister(operationBatch.GetOperation());

            operationsBatch.Add(operationBatch);
            return operationBatch;
        }

        private void RemoveOperationBatch<TConfig, TResponse>(IOperation<TConfig, TResponse> _Operation)
            where TConfig : OperationConfig
            where TResponse : OperationResponse
        {
            OperationBatch<TConfig, TResponse> operationBatch = GetRegisteredOperationBatch(_Operation);
            operationBatch?.Dispose();
            operationsBatch?.Remove(operationBatch);
        }
        
        #region UTILS

        /// <summary>
        /// Detect if an operation is already registered in the Operation Composites
        /// </summary>
        /// <param name="_Operation">Operation to compare</param>
        /// <typeparam name="TConfig">Config Type</typeparam>
        /// <typeparam name="TResponse">Response Type</typeparam>
        /// <returns>True if an operation similar is already registered</returns>
        private bool IsOperationRegistered<TConfig, TResponse>(IOperation<TConfig, TResponse> _Operation)
            where TConfig : OperationConfig
            where TResponse : OperationResponse
        {
            return operationsBatch.Any(operationComposite =>
            {
                IOperationBatch<TConfig, TResponse> op = operationComposite as IOperationBatch<TConfig, TResponse>;
                return op != null && op.GetOperation().Compare(_Operation.Config);
            });
        }

        /// <summary>
        /// Get the operation composite based on the operation already registered
        /// </summary>
        /// <param name="operation">OperationComposite to return</param>
        /// <typeparam name="TConfig">Config Type</typeparam>
        /// <typeparam name="TResponse">Response Type</typeparam>
        /// <returns>The OperationComposite based on the similar operation</returns>
        private OperationBatch<TConfig, TResponse> GetRegisteredOperationBatch<TConfig, TResponse>(
            IOperation<TConfig, TResponse> operation)
            where TConfig : OperationConfig
            where TResponse : OperationResponse
        {
            return (OperationBatch<TConfig, TResponse>)operationsBatch.SingleOrDefault(operationBatch =>
            {
                IOperationBatch<TConfig, TResponse> op = operationBatch as IOperationBatch<TConfig, TResponse>;
                return op != null && op.GetOperation().Compare(operation.Config);
            });
        }

        #endregion
    }
}
