

namespace Kinetix.Internal
{
    public abstract class AKinetixManager: IKinetixManager
    {
        protected ServiceLocator serviceLocator;
        
        public AKinetixManager(ServiceLocator _ServiceLocator, KinetixCoreConfiguration _Config)
        {
            serviceLocator = _ServiceLocator;
            
            Initialize(_Config);
        }

        protected abstract void Initialize(KinetixCoreConfiguration _Config);
    }
}
