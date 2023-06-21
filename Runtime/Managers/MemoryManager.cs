// // ----------------------------------------------------------------------------
// // <copyright file="PersistentDataManager.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System.IO;
using Kinetix.Internal.Cache;
using Kinetix.Utils;
using UnityEngine;

namespace Kinetix.Internal
{
    internal static class MemoryManager
    {
        private static float MaxStorageSizeInMB = 50f;
        private static float MaxRAMSizeinMB     = 50f;

        private const float MinStorageSizeInMB = 50f;
        private const float MinRamSizeInMB     = 50f;

        private static double currentRAM;
        private static double currentStorage;

        public static void Initialize()
        {
            MaxStorageSizeInMB = MinStorageSizeInMB;
            MaxRAMSizeinMB     = MinRamSizeInMB;

            CleanStorageObsoleteAnimations();
            currentStorage = ByteConverter.ConvertBytesToMegabytes(FileUtils.GetSizeOfAnimationsCache());
            currentRAM     = 0.0f;
        }

        #region RAM

        public static void CheckRAM(string _ExcludeUUID = null, KinetixAvatar _ExcludeAvatar = null)
        {
            if (HasRAMExceedMemoryLimit())
                CleanRAM(_ExcludeUUID, _ExcludeAvatar);
        }
        
        public static bool HasRAMExceedMemoryLimit()
        {
            return GetRAMSize() > MaxRAMSizeinMB;
        }

        public static double GetRAMSize()
        {
            return ByteConverter.ConvertBytesToMegabytes((long)currentRAM);
        }

        public static void AddRamAllocation(float _RamAllocationInBytes)
        {
            currentRAM += _RamAllocationInBytes;
            KinetixDebug.Log("RAM Size : " + GetRAMSize().ToString("F1") + "MB");
        }

        public static void RemoveRamAllocation(float _RamAllocationInBytes)
        {
            currentRAM -= _RamAllocationInBytes;
            KinetixDebug.Log("RAM Size : " + GetRAMSize().ToString("F1") + "MB");
        }

        private static void CleanRAM(string _ExcludeUUID = null, KinetixAvatar _ExcludeAvatar = null)
        {
            CleanRAMNonLocalPlayerAnimation(_ExcludeUUID, _ExcludeAvatar);
        }
        
        private static void CleanRAMNonLocalPlayerAnimation(string _ExcludeUUID, KinetixAvatar _ExcludeAvatar)
        {
            KinetixEmote[] emotes = EmotesManager.GetAllEmotes();
            for (int i = 0; i < emotes.Length; i++)
            {
                if (!HasRAMExceedMemoryLimit())
                    return;

                // Clear emote clip for all avatars except exclude avatar and local player avatar if UUID match
                // Otherwise clean all avatars except local player avatar
                EmotesManager.ForceClearEmote(emotes[i], emotes[i].Ids.UUID == _ExcludeUUID ? new[] { LocalPlayerManager.KAvatar, _ExcludeAvatar } : new[] { LocalPlayerManager.KAvatar });
            }
        }
        
        private static void CleanRAMLocalPlayerAnimation(string _ExcludeUUID, KinetixAvatar _ExcludeAvatar)
        {
            KinetixEmote[] emotes = EmotesManager.GetAllEmotes();
            for (int i = 0; i < emotes.Length; i++)
            {
                if (!HasRAMExceedMemoryLimit())
                    return;
                EmotesManager.ForceClearEmote(emotes[i], emotes[i].Ids.UUID == _ExcludeUUID ? new[] {  _ExcludeAvatar } : new KinetixAvatar[] { } );

            }
        }
        
        public static void DeleteFileInRaM(Object _obj)
        {
            Object.Destroy(_obj);
        }

        #endregion

        #region STORAGE

        public static void CheckStorage()
        {
            if (HasStorageExceedMemoryLimit())
                CleanStorage();
        }
        
        public static bool FileExists(string _Path)
        {
            return File.Exists(_Path);
        }
        
        public static bool HasStorageExceedMemoryLimit()
        {
            return GetStorageSize() > MaxStorageSizeInMB;
        }
        
        private static double GetStorageSize()
        {
            return ByteConverter.ConvertBytesToMegabytes(FileUtils.GetSizeOfAnimationsCache());
        }
        
        public static void AddStorageAllocation(string _Path, bool _Log = true)
        {
            if (File.Exists(_Path))
            {
                currentStorage += new FileInfo(_Path).Length;
                if (_Log)
                    KinetixDebug.Log("Storage Size : " + GetStorageSize().ToString("F1") + "MB");
            }
        }

        public static void RemoveStorageAllocation(string _Path, bool _Log = true)
        {
            if (File.Exists(_Path))
            {
                currentStorage -= new FileInfo(_Path).Length;
                if (_Log)
                    KinetixDebug.Log("Storage Size : " + GetStorageSize().ToString("F1") + "MB");
            }
        }
        
        private static void CleanStorage()
        {
            if (!HasStorageExceedMemoryLimit())
                return;

            CleanStorageNonLocalPlayerAnimation();
            CleanStorageLocalPlayerAnimation();
        }
        
        private static void CleanStorageObsoleteAnimations()
        {
            // Clear first non use files
            if (!Directory.Exists(PathConstants.CacheAnimationsPath))
                return;
            
            DirectoryInfo di    = new DirectoryInfo(PathConstants.CacheAnimationsPath);
            FileInfo[]    files = di.GetFiles();
            foreach (FileInfo file in files)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
                DeleteFileInStorage(Path.Combine(fileNameWithoutExtension), false);
            }
        }

        private static void CleanStorageNonLocalPlayerAnimation()
        {
            KinetixEmote[] emotes = EmotesManager.GetAllEmotes();
            for (int i = 0; i < emotes.Length; i++)
            {
                if (!HasStorageExceedMemoryLimit())
                    return;

                if (emotes[i].IsFileInUse())
                    continue;

                if (!LocalPlayerManager.IsEmoteUsedByPlayer(emotes[i].Ids))
                {
                    DeleteFileInStorage(emotes[i].Ids.UUID);
                }
            }
        }
        
        private static void CleanStorageLocalPlayerAnimation()
        {
            KinetixEmote[] emotes = EmotesManager.GetAllEmotes();
            for (int i = 0; i < emotes.Length; i++)
            {
                if (!HasStorageExceedMemoryLimit())
                    return;
                
                if (emotes[i].IsFileInUse())
                    continue;

                if (LocalPlayerManager.IsEmoteUsedByPlayer(emotes[i].Ids))
                {
                    DeleteFileInStorage(emotes[i].Ids.UUID);
                }
            }
        }
        
        public static void DeleteFileInStorage(string _UUID, bool _Log = true)
        {
            string path = Path.Combine(PathConstants.CacheAnimationsPath, (_UUID + ".glb"));

            if (File.Exists(path))
            {
                RemoveStorageAllocation(path, _Log);
                File.Delete(path);
            }
        }
        
        #endregion
    }
}
