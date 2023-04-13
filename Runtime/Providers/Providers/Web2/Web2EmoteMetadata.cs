
namespace Kinetix.Internal 
{
    [System.Serializable]
    public class Web2EmoteMetadata
    {
        public string uuid;
        public string name;
        public string origin;
        public float duration;

        public AnimationMetadata ToAnimationMetadata()
        {
            AnimationMetadata animationMetadata = new AnimationMetadata
            {
                    Ids          = new AnimationIds(uuid + uuid, uuid, uuid),
                    Name         = name,
                    Description  = string.Empty,
                    AnimationURL = string.Empty,
                    IconeURL     = string.Empty,

                    EmoteRarity = ERarity.NONE,       
            };

            return animationMetadata;
        }
    }
}
