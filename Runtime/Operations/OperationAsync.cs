using System.Threading.Tasks;

namespace Kinetix.Internal
{
    public abstract class OperationAsync<T>
    {
        public EProgressStatus ProgressStatus;
        public Task<T>         Task;

        protected OperationAsync()
        {
            ProgressStatus = EProgressStatus.NONE;
        }

        public abstract Task<T> Execute();
    }
}
