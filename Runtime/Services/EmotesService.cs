using System.Collections.Generic;
using System.Linq;

namespace Kinetix.Internal
{
    public class EmotesService: IKinetixService
    {
        private Dictionary<string, KinetixEmote> kinetixEmotes;
        private Queue<AvatarEmotePair> toClearQueue;
        protected ServiceLocator serviceLocator;

        public EmotesService(ServiceLocator _ServiceLocator, KinetixCoreConfiguration _Config) 
        {
            kinetixEmotes = new Dictionary<string, KinetixEmote>();
            toClearQueue = new Queue<AvatarEmotePair>();
            serviceLocator = _ServiceLocator;
        }

        public struct AvatarEmotePair
        {
            public KinetixAvatar Avatar;
            public AnimationIds EmoteIds;
        }

        public KinetixEmote GetEmote(AnimationIds _AnimationIds)
        {
            if (!kinetixEmotes.ContainsKey(_AnimationIds.UUID))
                kinetixEmotes.Add(_AnimationIds.UUID, new KinetixEmote(_AnimationIds));                

            return kinetixEmotes[_AnimationIds.UUID];
        }

        public KinetixEmote[] GetEmotes(AnimationIds[] _AnimationIds)
        {
            KinetixEmote[] kinetixEmotesTmp = new KinetixEmote[_AnimationIds.Length];
            for (int i = 0; i < _AnimationIds.Length; i++)
            {
                kinetixEmotesTmp[i] = GetEmote(_AnimationIds[i]);
            }

            return kinetixEmotesTmp;
        }

        public KinetixEmote[] GetAllEmotes()
        {
            return kinetixEmotes?.Values.ToArray();
        }
    }
}
