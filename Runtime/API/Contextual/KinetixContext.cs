using System.Collections.Generic;

namespace Kinetix.Internal
{
    public class KinetixContext
    {
        public bool PlayContext(string _ContextName)
        {
            // CONTEXTUAL EMOTES ARE NOT AVAILABLE FOR WEB 3
            return false;
        }

        public void RegisterEmoteForContext(string _ContextName, string _EmoteUuid)
        {
            // CONTEXTUAL EMOTES ARE NOT AVAILABLE FOR WEB 3
        }

        public void UnregisterEmoteForContext(string _ContextName)
        {            
            // CONTEXTUAL EMOTES ARE NOT AVAILABLE FOR WEB 3
        }

        public ContextualEmote GetContextEmote(string _ContextName)
        {
            // CONTEXTUAL EMOTES ARE NOT AVAILABLE FOR WEB 3
            return null;
        }

        public Dictionary<string, ContextualEmote> GetContextEmotes()
        {
            // CONTEXTUAL EMOTES ARE NOT AVAILABLE FOR WEB 3
            return null;
        }

        public bool IsContextEmoteAvailable(string _ContextName)
        {
            // CONTEXTUAL EMOTES ARE NOT AVAILABLE FOR WEB 3
            return false;
        }
    }
}
