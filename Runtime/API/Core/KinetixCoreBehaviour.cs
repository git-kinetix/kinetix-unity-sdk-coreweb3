// // ----------------------------------------------------------------------------
// // <copyright file="KinetixCoreBehaviour.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using Kinetix.Internal.Cache;

namespace Kinetix.Internal
{
    internal static class KinetixCoreBehaviour
    {
        private static bool initialized;
        public static  bool Initialized => initialized;

        public static void Initialize(KinetixCoreConfiguration _Configuration, Action _OnInitialized)
        {
            KinetixCore.Account   = new KinetixAccount();
            KinetixCore.Metadata  = new KinetixMetadata();
            KinetixCore.Animation = new KinetixAnimation();
            KinetixCore.Network   = new KinetixNetwork();

            InitializeManagers(_Configuration);

            initialized = true;
            _OnInitialized?.Invoke();
        }

        private static void InitializeManagers(KinetixCoreConfiguration _Configuration)
        {
            KinetixDebug.c_ShowLog = _Configuration.ShowLogs;
            
            AssetManager.Initialize();
            FreeAnimationsManager.Initialize();
            ProviderManager.Initialize(_Configuration.NodeProvideAPIKey);
            EmotesManager.Initialize();
            LocalPlayerManager.Initialize(_Configuration.PlayAutomaticallyAnimationOnAnimators);
            MemoryManager.Initialize();
            AccountManager.Initialize();
            NetworkManager.Initialize(_Configuration.NetworkConfiguration);
            KinetixAnalytics.Initialize(_Configuration.EnableAnalytics);
        }

        public static bool IsInitialized()
        {
            return Initialized;
        }
    }
}
