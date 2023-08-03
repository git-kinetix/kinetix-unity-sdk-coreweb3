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
    public class MemoryService: IKinetixService
    {
        private float MaxStorageSizeInMB = 50f;
        private float MaxRAMSizeinMB     = 50f;

        private const float MinStorageSizeInMB = 50f;
        private const float MinRamSizeInMB     = 50f;

        private double currentRAM;
        private double currentStorage;

        public MemoryService()
        {
            MaxStorageSizeInMB = MinStorageSizeInMB;
            MaxRAMSizeinMB     = MinRamSizeInMB;

            CleanStorageObsoleteAnimations();
            currentStorage = ByteConverter.ConvertBytesToMegabytes(FileUtils.GetSizeOfAnimationsCache());
            currentRAM     = 0.0f;
        }

        #region RAM

        public void CheckRAM(string _ExcludeUUID = null, KinetixAvatar _ExcludeAvatar = null)
        {
            if (HasRAMExceedMemoryLimit())
                CleanRAM(_ExcludeUUID, _ExcludeAvatar);
        }
        
        public bool HasRAMExceedMemoryLimit()
        {
            return GetRAMSize() > MaxRAMSizeinMB;
        }

        public double GetRAMSize()
        {
            return ByteConverter.ConvertBytesToMegabytes((long) currentRAM);
        }

        public void AddRamAllocation(float _RamAllocationInBytes)
        {
            currentRAM += _RamAllocationInBytes;
            KinetixDebug.Log("RAM Size : " + GetRAMSize().ToString("F1") + "MB");
        }

        public void RemoveRamAllocation(float _RamAllocationInBytes)
        {
            currentRAM -= _RamAllocationInBytes;
            KinetixDebug.Log("RAM Size : " + GetRAMSize().ToString("F1") + "MB");
        }

        private void CleanRAM(string _ExcludeUUID = null, KinetixAvatar _ExcludeAvatar = null)
        {
            CleanRAMNonLocalPlayerAnimation(_ExcludeUUID, _ExcludeAvatar);
        }
        
        private void CleanRAMNonLocalPlayerAnimation(string _ExcludeUUID, KinetixAvatar _ExcludeAvatar)
        {
            KinetixEmote[] emotes = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetAllEmotes();
            for (int i = 0; i < emotes.Length; i++)
            {
                if (!HasRAMExceedMemoryLimit())
                    return;

                // Clear emote clip for all avatars except exclude avatar and local player avatar if UUID match
                // Otherwise clean all avatars except local player avatar
                ForceClearEmote(emotes[i], emotes[i].Ids.UUID == _ExcludeUUID ? new[] { KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().KAvatar, _ExcludeAvatar } : new[] { KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().KAvatar });
            }
        }
        
        private void CleanRAMLocalPlayerAnimation(string _ExcludeUUID, KinetixAvatar _ExcludeAvatar)
        {
            if (!HasRAMExceedMemoryLimit())
                    return;
        
            KinetixEmote[] emotes = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetAllEmotes();

            for (int i = 0; i < emotes.Length; i++)
            {
                ForceClearEmote(emotes[i], emotes[i].Ids.UUID == _ExcludeUUID ? new[] {  _ExcludeAvatar } : new KinetixAvatar[] { } );
            }
        }

        private void ForceClearEmote(KinetixEmote _kinetixEmote, KinetixAvatar[] _AvoidAvatars = null)
        {
            _kinetixEmote.ClearAllAvatars(_AvoidAvatars);
        }
        
        public void DeleteFileInRaM(Object _obj)
        {
            Object.Destroy(_obj);
        }

        #endregion

        #region STORAGE

        public void CheckStorage()
        {
            if (HasStorageExceedMemoryLimit())
                CleanStorage();
        }
        
        public bool FileExists(string _Path)
        {
            return File.Exists(_Path);
        }
        
        public bool HasStorageExceedMemoryLimit()
        {
            return GetStorageSize() > MaxStorageSizeInMB;
        }
        
        private double GetStorageSize()
        {
            return ByteConverter.ConvertBytesToMegabytes(FileUtils.GetSizeOfAnimationsCache());
        }
        
        public void AddStorageAllocation(string _Path, bool _Log = true)
        {
            if (File.Exists(_Path))
            {
                currentStorage += new FileInfo(_Path).Length;
                if (_Log)
                    KinetixDebug.Log("Storage Size : " + GetStorageSize().ToString("F1") + "MB");
            }
        }

        public void RemoveStorageAllocation(string _Path, bool _Log = true)
        {
            if (File.Exists(_Path))
            {
                currentStorage -= new FileInfo(_Path).Length;
            }
        }
        
        private void CleanStorage()
        {
            if (!HasStorageExceedMemoryLimit())
                return;

            CleanStorageNonLocalPlayerAnimation();
            CleanStorageLocalPlayerAnimation();
        }
        
        private void CleanStorageObsoleteAnimations()
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

        private void CleanStorageNonLocalPlayerAnimation()
        {
            KinetixEmote[] emotes = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetAllEmotes();
            for (int i = 0; i < emotes.Length; i++)
            {
                if (!HasStorageExceedMemoryLimit())
                    return;

                if (emotes[i].IsFileInUse())
                    continue;

                if (!KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().IsEmoteUsedByPlayer(emotes[i].Ids))
                {
                    DeleteFileInStorage(emotes[i].Ids.UUID);
                }
            }
        }
        
        private void CleanStorageLocalPlayerAnimation()
        {
            KinetixEmote[] emotes = KinetixCoreBehaviour.ServiceLocator.Get<EmotesService>().GetAllEmotes();
            for (int i = 0; i < emotes.Length; i++)
            {
                if (!HasStorageExceedMemoryLimit())
                    return;
                
                if (emotes[i].IsFileInUse())
                    continue;

                if (KinetixCoreBehaviour.ManagerLocator.Get<LocalPlayerManager>().IsEmoteUsedByPlayer(emotes[i].Ids))
                {
                    DeleteFileInStorage(emotes[i].Ids.UUID);
                }
            }
        }
        
        public void DeleteFileInStorage(string _UUID, bool _Log = true)
        {
            string path = Path.Combine(PathConstants.CacheAnimationsPath, (_UUID + ".glb"));

            if (File.Exists(path))
            {
                RemoveStorageAllocation(path, _Log);
                File.Delete(path);
                
                if (_Log)
                    KinetixDebug.Log("Storage Size : " + (GetStorageSize().ToString("F1") + "MB"));
            }
        }
        
        #endregion
    }
}
