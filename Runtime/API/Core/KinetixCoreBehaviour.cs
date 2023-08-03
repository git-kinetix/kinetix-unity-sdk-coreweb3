// // ----------------------------------------------------------------------------
// // <copyright file="KinetixCoreBehaviour.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using Kinetix.Internal.Cache;
using UnityEngine;

namespace Kinetix.Internal
{
    internal static class KinetixCoreBehaviour
    {
        private static bool initialized;
        public static  bool Initialized => initialized;

        public static ServiceLocator ServiceLocator => serviceLocator;
        private static ServiceLocator serviceLocator;

        public static ManagerLocator ManagerLocator => managerLocator;
        private static ManagerLocator managerLocator;

        public static void Initialize(KinetixCoreConfiguration _Configuration, Action _OnInitialized)
        {    
            InitializeServices(_Configuration);

            InitializeManagers(_Configuration);

            KinetixCore.Account   = new KinetixAccount();
            KinetixCore.Metadata  = new KinetixMetadata();
            KinetixCore.Animation = new KinetixAnimation();
            KinetixCore.Network   = new KinetixNetwork();
            KinetixCore.UGC       = new KinetixUGC();
            KinetixCore.Context   = new KinetixContext();
            
            KinetixAnalytics.Initialize(_Configuration.EnableAnalytics);

            initialized = true;
            _OnInitialized?.Invoke();
        }

        private static void InitializeServices(KinetixCoreConfiguration _Configuration)
        {
            serviceLocator = new ServiceLocator();
            
            serviceLocator.Register<EmotesService>(new EmotesService(serviceLocator, _Configuration));
            serviceLocator.Register<LockService>(new LockService());
            serviceLocator.Register<MemoryService>(new MemoryService());
            serviceLocator.Register<AssetService>(new AssetService());
            serviceLocator.Register<RetargetingService>(new RetargetingService(serviceLocator));
            serviceLocator.Register<ProviderService>(new ProviderService(_Configuration));

            serviceLocator.Get<LockService>().OnRequestEmoteUnload += serviceLocator.Get<RetargetingService>().ClearAvatar;
        }

        private static void InitializeManagers(KinetixCoreConfiguration _Configuration)
        {
            KinetixDebug.c_ShowLog = _Configuration.ShowLogs;
            
            managerLocator = new ManagerLocator();
            
            managerLocator.Register<LocalPlayerManager>(new LocalPlayerManager(serviceLocator, _Configuration));
            managerLocator.Register<AccountManager>(new AccountManager(serviceLocator, _Configuration));
            managerLocator.Register<UGCManager>(new UGCManager(serviceLocator, _Configuration));
            managerLocator.Register<ContextManager>(new ContextManager(serviceLocator, _Configuration));
            managerLocator.Register<NetworkManager>(new NetworkManager(serviceLocator, _Configuration));
        }

        public static bool IsInitialized()
        {
            return Initialized;
        }
    }
}
