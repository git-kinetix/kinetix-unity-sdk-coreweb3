using System.Threading;
using System.Threading.Tasks;

namespace Kinetix.Internal
{
    public abstract class Operation<TConfig, TResponse> : IOperation<TConfig, TResponse>
        where TConfig : OperationConfig
        where TResponse : OperationResponse
    {
        protected Operation(TConfig config)
        {
            Config = config;
        }

        /// <summary>
        /// TaskCompletionSource to raise
        /// </summary>
        public TaskCompletionSource<TResponse> CurrentTaskCompletionSource { get; set; }

        /// <summary>
        /// Configuration of the operation
        /// </summary>
        public TConfig Config { get; set; }

        /// <summary>
        /// CancellationTokenSource of the operation
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Execute the CurrentTask
        /// </summary>
        /// <returns>CurrentTask</returns>
        public abstract Task Execute();

        public bool IsCompleted()
        {
            return CurrentTaskCompletionSource.Task.IsCompleted;
        }

        /// <summary>
        /// Override IOperation Execution for template abstraction
        /// </summary>
        async Task IOperation.Execute()
        {
            await Execute();
        }

        /// <summary>
        /// Methods to define if two operations are similar
        /// </summary>
        /// <param name="_Config">Config of the operation</param>
        /// <returns>True if same operation</returns>
        public abstract bool Compare(TConfig _Config);

        public abstract IOperation<TConfig, TResponse> Clone();
    }
}
