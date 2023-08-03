namespace Kinetix.Internal
{
    public static class OperationManagerShortcut
    {
        private static OperationManager op;

        public static OperationManager Get()
        {
            op ??= new OperationManager();

            return op;
        }
    }
}
