using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Analytics;
using Kinetix.Internal;

namespace Kinetix.Internal
{
    internal class AmplitudeLogger : IAnalyticsLogger
    {
        public class AnalyticsManagerMB: MonoBehaviour{}
        private static AnalyticsManagerMB analyticsManagerMB;

        public static string AmplitudeURL = "https://api2.amplitude.com/2/httpapi";
        public static string AmplitudeURL_EU = "https://api.eu.amplitude.com/2/httpapi";
            
        public static string API_KEY = "bc0ca01931bf1943c7671c9de3082d8f";
        
        public void Init()
        {
            if (analyticsManagerMB == null)
            {
                GameObject gameObject = new GameObject("StaticMB");
                analyticsManagerMB = gameObject.AddComponent<AnalyticsManagerMB>();
            }
        }

        public void SendEvent(string event_name="default", string idAnimation ="", KinetixAnalytics.Page page=KinetixAnalytics.Page.EmoteWheel, KinetixAnalytics.Event_type event_type=KinetixAnalytics.Event_type.Click, int tile_in_wheel=-1, int page_in_wheel=-1)
        {
            Init();

            analyticsManagerMB.StartCoroutine(SendWebRequestEvent(AmplitudeURL, event_name, idAnimation ));
        }

        private IEnumerator SendWebRequestEvent(string url, string event_name, string idAnimation)
        {
            JsonData jsonData = new JsonData();

            jsonData.api_key = API_KEY;
            jsonData.events = new oEvents();
            jsonData.events.user_id = AnalyticsSessionInfo.userId.ToString();
            jsonData.events.event_type = event_name;
            jsonData.events.device_id = SystemInfo.deviceUniqueIdentifier;
            jsonData.events.platform = Application.platform.ToString();
            jsonData.events.app_version = KinetixConstants.version;
            jsonData.events.os_name = SystemInfo.operatingSystem;
            jsonData.events.session_id = AnalyticsSessionInfo.sessionId.ToString();

            if(idAnimation != "")
            {
                jsonData.events.event_properties.Add("idAnimation", idAnimation);
            }

            Uri             uri        = new Uri(url);
            UnityWebRequest uwr        = new UnityWebRequest(uri, "POST");
            byte[]          jsonToSend = new System.Text.UTF8Encoding().GetBytes(JsonConvert.SerializeObject(jsonData));
            uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();

            switch (uwr.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.Log("ERROR: " + uwr.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("Received: " + uwr.downloadHandler.text);
                    break;
            }
        }

        public class oEvents
        {
            public string user_id;
            public string event_type;
            public string device_id;
            public string platform;
            public string app_version;
            public string os_name;
            public string session_id;
            public string country;
            public Dictionary<string, object> user_properties = new Dictionary<string, object>();
            public Dictionary<string, object> event_properties = new Dictionary<string, object>();
        }

        public class JsonData
        {
            public string api_key;
            public oEvents events;
        }
    }    
}
