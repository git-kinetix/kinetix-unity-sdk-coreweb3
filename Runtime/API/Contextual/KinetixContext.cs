using System.Collections.Generic;

namespace Kinetix.Internal
{
    public class KinetixContext
    {
        public bool PlayContext(string contextName)
        {
            // CONTEXTUAL EMOTES ARE NOT AVAILABLE FOR WEB 3
            return false;
        }

        public void RegisterEmoteForContext(string contextName, string emoteUuid)
        {
            // CONTEXTUAL EMOTES ARE NOT AVAILABLE FOR WEB 3
        }

        public ContextualEmote GetContextEmote(string contextName)
        {
            // CONTEXTUAL EMOTES ARE NOT AVAILABLE FOR WEB 3
            return null;
        }

        public Dictionary<string, ContextualEmote> GetContextEmotes()
        {
            // CONTEXTUAL EMOTES ARE NOT AVAILABLE FOR WEB 3
            return null;
        }
    }
}
