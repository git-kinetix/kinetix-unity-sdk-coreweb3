// // ----------------------------------------------------------------------------
// // <copyright file="ConstPath.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System.IO;
using UnityEngine;

namespace Kinetix.Utils
{
    public static class PathConstants
    {
        public static string CacheAnimationsPath => Path.Combine(Application.persistentDataPath, "Kinetix", "Animations");
    }
}
