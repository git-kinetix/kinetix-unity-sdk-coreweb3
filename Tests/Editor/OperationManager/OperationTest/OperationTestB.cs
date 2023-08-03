using System;
using System.Threading.Tasks;
using Kinetix.Internal;

namespace Kinetix.Tests
{
    public class OperationTestB : Operation<OperationTestConfig, OperationTestResponse>
    {
        public OperationTestB(OperationTestConfig _Config) : base(_Config)
        {
        }

        public override async Task Execute()
        {
            await Task.Delay(Config.delayExecutionInMs);

            if (Config.willCancel)
            {
                CurrentTaskCompletionSource.SetCanceled();
                return;
            }

            if (Config.willThrow)
            {
                CurrentTaskCompletionSource.SetException(new Exception());
                return;
            }

            OperationTestResponse opResponse = new OperationTestResponse()
            {
                operationId = Config.RandomId
            };
            
            CurrentTaskCompletionSource.SetResult(opResponse);
        }

        public override bool Compare(OperationTestConfig _Config)
        {
            return _Config.RandomId.Equals(Config.RandomId);
        }

        public override IOperation<OperationTestConfig, OperationTestResponse> Clone()
        {
            return new OperationTestA(Config);
        }
    }
}

