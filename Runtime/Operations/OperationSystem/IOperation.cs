using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kinetix.Internal
{
    public interface IOperation
    {
        /// <summary>
        /// Execute the CurrentTask
        /// </summary>
        /// <returns>CurrentTask</returns>
        Task Execute();

        bool IsCompleted();
    }

    public interface IOperation<TConfig, TResponse> : IOperation
        where TConfig : OperationConfig
        where TResponse : OperationResponse
    {
        /// <summary>
        /// TaskCompletionSource to raise
        /// </summary>
        TaskCompletionSource<TResponse> CurrentTaskCompletionSource { get; set; }

        /// <summary>
        /// Configuration of the operation
        /// </summary>
        TConfig                 Config                  { get; set; }
        
        /// <summary>
        /// CancellationTokenSource of the operation
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Execute the CurrentTask
        /// </summary>
        /// <returns>CurrentTask</returns>
        new Task Execute();
        
        /// <summary>
        /// Methods to define if two operations are similar
        /// </summary>
        /// <param name="_Config">Config of the operation</param>
        /// <returns>True if same operation</returns>
        bool     Compare(TConfig _Config);

        IOperation<TConfig, TResponse> Clone();
    }
}
