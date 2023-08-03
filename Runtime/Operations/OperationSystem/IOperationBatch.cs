namespace Kinetix.Internal
{
    public interface IOperationBatch
    {
        
        IOperation GetOperation();
    }

    public interface IOperationBatch<TConfig, TResponse> : IOperationBatch 
        where TConfig : OperationConfig
        where TResponse : OperationResponse
    {
        new IOperation<TConfig, TResponse> GetOperation();
    }
}
