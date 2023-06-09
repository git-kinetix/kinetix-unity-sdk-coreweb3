using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Analytics;
using Kinetix.Internal;

namespace Kinetix.Internal
{
    public interface IAnalyticsLogger
    {
        void Init();
        void SendEvent(string event_name="default", string idAnimation ="", KinetixAnalytics.Page page=KinetixAnalytics.Page.EmoteWheel, KinetixAnalytics.Event_type event_type=KinetixAnalytics.Event_type.Click, int tile_in_wheel=-1);
    }    
}
