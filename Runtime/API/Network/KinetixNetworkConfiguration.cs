
namespace Kinetix
{
    public class KinetixNetworkConfiguration
    {
        public float MaxWaitTimeBeforeEmoteExpiration;

        public int TargetFrameCacheCount;

        public bool SendLocalPosition = false;
        
        public bool SendLocalScale = false;

        public static KinetixNetworkConfiguration GetDefault()
        {
            return new KinetixNetworkConfiguration {
                MaxWaitTimeBeforeEmoteExpiration = 2f,
                TargetFrameCacheCount = 10
            };
        }
    }
}
