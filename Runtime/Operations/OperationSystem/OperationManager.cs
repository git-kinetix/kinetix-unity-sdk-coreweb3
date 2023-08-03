using System.Threading;
using System.Threading.Tasks;

namespace Kinetix.Internal
{
    public class OperationManager
    {
        private readonly OperationOrchestrator orchestrator;

        public OperationManager()
        {
            orchestrator = new OperationOrchestrator();
        }

        /// <summary>
        /// Request execute of an operation, create or redirect to a similar operation
        /// </summary>
        /// <param name="_Operation">Operation</param>
        /// <typeparam name="TConfig">Config Type</typeparam>
        /// <typeparam name="TResponse">Response Type</typeparam>
        /// <returns>The response in the type of TResponse</returns>
        public Task<TResponse> RequestExecution<TConfig, TResponse>(IOperation<TConfig, TResponse> _Operation,
            CancellationTokenSource _CancellationTokenSource = null)
            where TConfig : OperationConfig
            where TResponse : OperationResponse
        {
            _CancellationTokenSource ??= new CancellationTokenSource();

            return orchestrator.RequestExecution(_Operation, _CancellationTokenSource);
        }
    }
}
