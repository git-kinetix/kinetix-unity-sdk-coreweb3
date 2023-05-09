using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Analytics;
using Kinetix.Internal;

namespace Kinetix.Internal
{
    internal class SegmentLogger : IAnalyticsLogger
    {
        public class AnalyticsManagerMB: MonoBehaviour{}
        private static AnalyticsManagerMB analyticsManagerMB;

        private const string SegmentURL = "https://api.segment.io/v1/track";
        private const string WRITE_KEY  = "81TtulYt7ViNrarGnaL6vGsq2fCwZD0G";
        
        public void Init()
        {
            if (analyticsManagerMB == null)
            {
                GameObject gameObject = new GameObject("KinetixAnalytics");
                gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                analyticsManagerMB   = gameObject.AddComponent<AnalyticsManagerMB>();
            }
        }

        public void SendEvent(string event_name="default", string idAnimation ="", KinetixAnalytics.Page page=KinetixAnalytics.Page.EmoteWheel, KinetixAnalytics.Event_type event_type=KinetixAnalytics.Event_type.Click, int tile_in_wheel=-1, int page_in_wheel=-1)
        {
            Init();
            analyticsManagerMB.StartCoroutine(SendWebRequestEvent(SegmentURL, event_name, idAnimation, page, event_type, tile_in_wheel, page_in_wheel ));
        }

        private IEnumerator SendWebRequestEvent(string uri, string event_name, string idAnimation, KinetixAnalytics.Page page, KinetixAnalytics.Event_type event_type, int tile_in_wheel, int page_in_wheel)
        {
            Dictionary<string, object> jsonData = new Dictionary<string, object>();

            jsonData.Add("userId", AnalyticsSessionInfo.userId.ToString() );
            jsonData.Add("event", GetTrackingName(event_name, page, event_type) );
            jsonData.Add("properties", GetEventProperties( idAnimation, tile_in_wheel, page_in_wheel ));
            jsonData.Add("context", GetContext() );

            using (UnityWebRequest uwr = new UnityWebRequest(uri, "POST"))
            {
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(JsonConvert.SerializeObject(jsonData));
                uwr.uploadHandler   = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                uwr.SetRequestHeader("Content-Type", "application/json");
                uwr.SetRequestHeader("Authorization", "Basic " + System.Convert.ToBase64String( System.Text.Encoding.ASCII.GetBytes(WRITE_KEY)));

                yield return uwr.SendWebRequest();

                switch (uwr.result)
                {                
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                    case UnityWebRequest.Result.ProtocolError:
                        break;
                    case UnityWebRequest.Result.Success:
                        break;
                }
            }
        }

        Dictionary<string, object> GetEventProperties( string idAnimation, int tile_in_wheel, int page_in_wheel  )
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            if(idAnimation != "")
                properties.Add("idAnimation", idAnimation);
            if( tile_in_wheel != -1 ) 
                properties.Add("tile", tile_in_wheel);
            if( page_in_wheel != -1 ) 
                properties.Add("page", page_in_wheel);

            return properties;
        }

        Dictionary<string, object> GetContext()
        {
            Dictionary<string, object> context = new Dictionary<string, object>();   

            Dictionary<string, object> app = new Dictionary<string, object>();   
            app.Add("name", Application.productName );
            app.Add("version", KinetixConstants.version );
            context.Add("app", app);

            Dictionary<string, object> device = new Dictionary<string, object>();   
            // device.Add("id", SystemInfo.deviceUniqueIdentifier );
            device.Add("model", SystemInfo.deviceModel );
            device.Add("type", SystemInfo.deviceType.ToString() );
            device.Add("manufacturer", Application.platform.ToString() );
            // device.Add("deviceName", SystemInfo.deviceName );
            // device.Add("graphicsDeviceVendor", SystemInfo.graphicsDeviceVendor );
            context.Add("device", device);

            Dictionary<string, object> os = new Dictionary<string, object>();   
            os.Add("name", SystemInfo.operatingSystemFamily.ToString() );
            os.Add("version", SystemInfo.operatingSystem );
            context.Add("os", os);

            return context;
        }

        string GetTrackingName(string event_text="", KinetixAnalytics.Page page=KinetixAnalytics.Page.EmoteWheel, KinetixAnalytics.Event_type event_type=KinetixAnalytics.Event_type.Click)
        {
            string Environment;

            #if !UNITY_EDITOR && !DEV_KINETIX
                Environment = "Prod";
            #else
                Environment = "PreProd";
            #endif
            
            string Product = "SDK";
            string Page = page.ToString();
            string Tracking = "Unity";
            string Event_Type = event_type.ToString();
            string Placement = "INGAME";
            string Event_Text = event_text;

            return Environment+"_"+Product+"_"+Page+"_"+Tracking+"_"+Event_Type+"_"+Placement+"_"+Event_Text;
        }


    }    
}
