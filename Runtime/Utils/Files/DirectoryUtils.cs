// // ----------------------------------------------------------------------------
// // <copyright file="DirectoryUtils.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System.IO;

namespace Kinetix.Utils
{
    public static class FileUtils
    {
        /// <summary>
        ///     Get total size of the cache where is stored the animations
        /// </summary>
        /// <returns>Size in bytes</returns>
        public static long GetSizeOfAnimationsCache()
        {
            if (!Directory.Exists(PathConstants.CacheAnimationsPath))
                return 0;
            
            DirectoryInfo info = new DirectoryInfo(PathConstants.CacheAnimationsPath);
            return GetDirectorySize(info);
        }

        /// <summary>
        ///     Recursively get size of a directory
        /// </summary>
        /// <param name="d">Root Directory</param>
        /// <returns>Size in bytes</returns>
        private static long GetDirectorySize(DirectoryInfo d)
        {
            long size = 0;

            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }

            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += GetDirectorySize(di);
            }

            return size;
        }
    }
}
