// // ----------------------------------------------------------------------------
// // <copyright file="KinetixCore.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using Kinetix.Internal;

namespace Kinetix
{
    public static class KinetixCore
    {
        public static event Action OnInitialized;

        /// <summary>
        /// Initialize Kinetix Core SDK.
        /// </summary>
        public static void Initialize(KinetixCoreConfiguration _Configuration)
        {
            KinetixCoreBehaviour.Initialize(_Configuration, OnInitialized);
        }

        /// <summary>
        /// Check state of the Kinetix Core SDK.
        /// </summary>
        /// <returns>Is sdk initialized</returns>
        public static bool IsInitialized()
        {
            return KinetixCoreBehaviour.IsInitialized();
        }

        #region References

        public static KinetixAccount   Account   { get; internal set; }
        public static KinetixMetadata  Metadata  { get; internal set; }
        public static KinetixAnimation Animation { get; internal set; }
        public static KinetixNetwork   Network   { get; internal set; }
        public static KinetixUGC   UGC   { get; internal set; }

        #endregion
    }
}
