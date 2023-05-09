using System.Collections;
using System.Collections.Generic;
using Kinetix;
using UnityEngine;

namespace Kinetix.Internal.Utils
{
    public static class ListHelper
    {
        public static void AggregateAndDistinct(this List<KinetixEmote> _EmotesBase, KinetixEmote[] _EmotesToAdd)
        {
            for (int i = 0; i < _EmotesToAdd.Length; i++)
            {
                if (_EmotesToAdd[i] != null && !_EmotesBase.Exists(emote => emote.Ids.UUID == _EmotesToAdd[i].Ids.UUID))
                    _EmotesBase.Add(_EmotesToAdd[i]);
            }
        }
    }
}
