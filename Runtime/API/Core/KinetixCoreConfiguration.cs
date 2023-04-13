// // ----------------------------------------------------------------------------
// // <copyright file="KinetixCoreConfiguration.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

namespace Kinetix
{
    public class KinetixCoreConfiguration
    {
        // Node API Key
        public string NodeProvideAPIKey;
        // Play Animation Automatically on Animators
        public bool   PlayAutomaticallyAnimationOnAnimators = true;
        
        // Enable Analytics
        public bool   EnableAnalytics = true;
        
        // Show Logs
        public bool   ShowLogs = false;
        
        public KinetixNetworkConfiguration NetworkConfiguration;
    }
}
