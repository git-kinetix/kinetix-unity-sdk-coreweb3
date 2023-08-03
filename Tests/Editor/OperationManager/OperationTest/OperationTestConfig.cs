using Kinetix.Internal;

namespace Kinetix.Tests
{
    public class OperationTestConfig : OperationConfig
    {
        public readonly string RandomId;
        public          bool   willCancel;
        public          bool   willThrow;
        public          int    delayExecutionInMs;

        public OperationTestConfig(string _RandomId)
        {
            RandomId = _RandomId;
        }
    }
}
