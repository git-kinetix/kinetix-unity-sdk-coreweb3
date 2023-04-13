// // ----------------------------------------------------------------------------
// // <copyright file="KinetixCore.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using Kinetix.Internal;

namespace Kinetix
{
    public static class KinetixAnalytics
    {
        public static readonly IAnalyticsLogger EventLogger;
        private static bool isEnabled = true;

        public enum Page{
            EmoteWheel,
            Inventory,
            None
        }

        public enum Event_type{
            Click,
            DragDrop
        }

        static KinetixAnalytics()
        {
            EventLogger = new SegmentLogger();
        }

        /// <summary>
        ///     Send Event
        /// </summary>
        public static void SendEvent(string event_name, string idAnimation="", Page page=Page.EmoteWheel, Event_type event_type=Event_type.Click, int tile_in_wheel=-1, int page_in_wheel=-1)
        {
            if(isEnabled)
                EventLogger.SendEvent(event_name, idAnimation, page, event_type, tile_in_wheel, page_in_wheel);
        }

        public static void Initialize(bool _isEnabled)
        {
            isEnabled = _isEnabled;
        }

        public static void Disable()
        {
            isEnabled = false;
        }
    }
}
