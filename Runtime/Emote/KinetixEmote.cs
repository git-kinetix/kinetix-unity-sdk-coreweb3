using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kinetix.Internal.Cache;
using Kinetix.Internal.Retargeting;
using UnityEngine;

namespace Kinetix.Internal
{
    public class KinetixEmote
    {
        public AnimationIds      Ids      { get; }
        public bool IsLocal { get { return isLocal; } }
        public AnimationMetadata Metadata { get; private set; }

        private string                  pathGLB;

        private readonly Dictionary<KinetixAvatar, EmoteRetargetedData> retargetedEmoteByAvatar;
        private readonly Dictionary<KinetixAvatar, List<string>>        lockedMemoryAvatar;
        private readonly Dictionary<KinetixAvatar, List<Action>>        OnEmoteRetargetedByAvatar;

        // We store a list of avatar if emote should be deleted but playing right now and delete after unlock
        private readonly List<KinetixAvatar> avatarsToDeleteAfterUnlock;

        private bool isLocal;

        public KinetixEmote(AnimationIds _Ids)
        {
            Ids                        = _Ids;
            retargetedEmoteByAvatar    = new Dictionary<KinetixAvatar, EmoteRetargetedData>();
            OnEmoteRetargetedByAvatar  = new Dictionary<KinetixAvatar, List<Action>>();
            lockedMemoryAvatar         = new Dictionary<KinetixAvatar, List<string>>();
            avatarsToDeleteAfterUnlock = new List<KinetixAvatar>();
        }

        /// <summary>
        /// Check if emote contains metadata
        /// </summary>
        /// <returns>True if contains metadata</returns>
        public bool HasMetadata()
        {
            return Metadata != null;
        }

        /// <summary>
        /// Set the medata for this emote
        /// </summary>
        /// <param name="_AnimationMetadata">Metadata of the emote</param>
        public void SetMetadata(AnimationMetadata _AnimationMetadata)
        {
            Metadata = _AnimationMetadata;
        }

        /// <summary>
        /// Set the medata for this emote
        /// </summary>
        /// <param name="_AnimationMetadata">Metadata of the emote</param>
        public void SetLocalMetadata(AnimationMetadata _AnimationMetadata, string localGLBPath)
        {
            Metadata = _AnimationMetadata;
            isLocal = true;
            pathGLB = localGLBPath;
        }

        /// <summary>
        /// Set only IDs and URL in the medata
        /// Mainly used for network purposes with only animation need.
        /// </summary>
        /// <param name="_Ids"></param>
        /// <param name="_AnimationURL"></param>
        public void SetShortMetadata(AnimationIds _Ids, string _AnimationURL)
        {
            Metadata = new AnimationMetadata()
            {
                Ids          = _Ids,
                AnimationURL = _AnimationURL
            };
        }

        /// <summary>
        /// Check if GLB file is used (e.g. Import Retargeting)
        /// </summary>
        /// <returns>True if use</returns>
        public bool IsFileInUse()
        {
            return retargetedEmoteByAvatar.Values.Any(retargetedData => retargetedData.ProgressStatus != EProgressStatus.COMPLETED);
        }

        public bool HasAnimationRetargeted(KinetixAvatar _Avatar)
        {
            if (!retargetedEmoteByAvatar.ContainsKey(_Avatar))
                return false;
            return retargetedEmoteByAvatar[_Avatar].ProgressStatus == EProgressStatus.COMPLETED;
        }

        /// <summary>
        /// Check if emote has a valid GLB Path in storage in order to import it
        /// </summary>
        /// <returns>True if path exists</returns>
        private bool HasValidPath()
        {
            return !string.IsNullOrEmpty(pathGLB);
        }

        /// <summary>
        /// Check if emote is retargeting for a specific avatar
        /// </summary>
        /// <param name="_KinetixAvatar"></param>
        /// <returns></returns>
        private bool IsRetargeting(KinetixAvatar _KinetixAvatar)
        {
            if (!retargetedEmoteByAvatar.ContainsKey(_KinetixAvatar))
                return false;
            return retargetedEmoteByAvatar[_KinetixAvatar].ProgressStatus != EProgressStatus.COMPLETED;
        }

        #region Async Requests

        private async Task<string> GetFilePath()
        {
            if (HasValidPath())
                return pathGLB;
            
            pathGLB = await FileOperationManager.DownloadGLBByEmote(this);
            MemoryManager.AddStorageAllocation(pathGLB);

            return pathGLB;
        }

        /// <summary>
        /// Get an AnimationClip retargeted for a specific avatar
        /// </summary>
        /// <param name="_Avatar">The Avatar</param>
        /// <param name="_Priority">The priority in the retargeting queue</param>
        /// <param name="_Force">Force the retargeting even if we exceed memory.
        /// /!\ Be really cautious with this parameter as we keep a stable memory management.
        /// It is only use to for the emote played by the local player.
        /// </param>
        /// <returns>The AnimationClip for the specific Avatar</returns>
        public async Task<AnimationClip> GetRetargetedAnimationClipByAvatar(KinetixAvatar _Avatar, SequencerPriority _Priority, bool _Force)
        {
            TaskCompletionSource<AnimationClip> tcs = new TaskCompletionSource<AnimationClip>();

            if (!_Force && MemoryManager.HasStorageExceedMemoryLimit())
            {
                tcs.SetException(new Exception("Not enough storage space available to retarget : " + Ids.UUID));
                return await tcs.Task;
            }
            
            if (!_Force && MemoryManager.HasRAMExceedMemoryLimit())
            {
                tcs.SetException(new Exception("Not enough RAM space to retarget : " + Ids.UUID));
                return await tcs.Task;
            }
            
            if (!HasMetadata())
            {
                try
                {
                    KinetixDebug.LogWarning("No metadata found while retargeting AnimationClip");
                    SetMetadata(await MetadataOperationManager.DownloadMetadataByAnimationIds(Ids));
                }
                catch (Exception e)
                {
                    KinetixDebug.LogException(e);
                    return null;
                }
            }

            if (avatarsToDeleteAfterUnlock != null && avatarsToDeleteAfterUnlock.Contains(_Avatar))
                avatarsToDeleteAfterUnlock.Remove(_Avatar);

            if (!retargetedEmoteByAvatar.ContainsKey(_Avatar))
            {
                retargetedEmoteByAvatar.Add(_Avatar, new EmoteRetargetedData()
                {
                    ProgressStatus = EProgressStatus.NONE
                });
            }

            switch (retargetedEmoteByAvatar[_Avatar].ProgressStatus)
            {
                case EProgressStatus.COMPLETED:
                    tcs.SetResult(retargetedEmoteByAvatar[_Avatar].AnimationClipLegacyRetargeted);
                    break;
                case EProgressStatus.PENDING:
                    RegisterCallbacksOnRetargetedByAvatar(_Avatar, () => { tcs.SetResult(retargetedEmoteByAvatar[_Avatar].AnimationClipLegacyRetargeted); });
                    break;
                case EProgressStatus.NONE:
                {
                    if (MemoryManager.HasStorageExceedMemoryLimit())
                    {
                        tcs.SetException(new Exception("Not enough storage space available to retarget : " + Ids.UUID));
                        return await tcs.Task;
                    }
                    
                    if (MemoryManager.HasRAMExceedMemoryLimit())
                    {
                        tcs.SetException(new Exception("Not enough RAM space to retarget : " + Ids.UUID));
                        return await tcs.Task;
                    }

                    try
                    {
                        retargetedEmoteByAvatar[_Avatar].ProgressStatus = EProgressStatus.PENDING;

                        string path = await GetFilePath();

                        if (string.IsNullOrEmpty(path))
                            tcs.SetException(new Exception("GLB is not available"));
                        else
                        {
                            bool useWebRequest = false;
                            
#if (UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR
                            useWebRequest = isLocal;
#endif
                            
                            SequencerCancel cancelToken = new SequencerCancel();
                            retargetedEmoteByAvatar[_Avatar].CancelToken = cancelToken;
                            RetargetingManager.GetRetargetedAnimationClip<AnimationClip, AnimationClipExport>(_Avatar.Avatar, _Avatar.Root, path, _Priority, cancelToken, (clip, estimationSize) =>
                            {
                                
                                if (!_Force && !retargetedEmoteByAvatar.ContainsKey(_Avatar))
                                {
                                    // We delete clip as we dont need it anymore
                                    MemoryManager.DeleteFileInRaM(clip);
                                    CheckAvatarsLeft();
                                    tcs.SetException(new Exception("AnimationClip is not required anymore after retargeting"));
                                }
                                else
                                {
                                    retargetedEmoteByAvatar[_Avatar].ProgressStatus = EProgressStatus.COMPLETED;

                                    KinetixDebug.Log("Retargeted : " + pathGLB);

                                    MemoryManager.CheckStorage();
                                    MemoryManager.CheckRAM(Ids.UUID, _Avatar);

                                    
                                    if (MemoryManager.HasRAMExceedMemoryLimit())
                                    {
                                        ClearAvatar(_Avatar);
                                        tcs.SetException(new Exception("Not enough RAM space to retarget : " + Ids.UUID));
                                        return;
                                    }

                                    retargetedEmoteByAvatar[_Avatar].AnimationClipLegacyRetargeted = clip;
                                    retargetedEmoteByAvatar[_Avatar].SizeInBytes                   = estimationSize;
                                    retargetedEmoteByAvatar[_Avatar].CancelToken                   = null;
                                    MemoryManager.AddRamAllocation(estimationSize);
                                    NotifyCallbackOnRetargetedByAvatar(_Avatar);

                                    tcs.SetResult(clip);
                                }
                            }, useWebRequest: useWebRequest);
                        }
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                        return null;
                    }

                    break;
                }
            }

            return await tcs.Task;
        }

        #endregion

        #region Notifications

        public void RegisterCallbacksOnRetargetedByAvatar(KinetixAvatar _Avatar, Action _OnSucceed)
        {
            if (!OnEmoteRetargetedByAvatar.ContainsKey(_Avatar))
                OnEmoteRetargetedByAvatar.Add(_Avatar, new List<Action>());
            OnEmoteRetargetedByAvatar[_Avatar].Add(_OnSucceed);
        }

        private void NotifyCallbackOnRetargetedByAvatar(KinetixAvatar _Avatar)
        {
            if (!OnEmoteRetargetedByAvatar.ContainsKey(_Avatar))
                return;


            for (int i = 0; i < OnEmoteRetargetedByAvatar[_Avatar].Count; i++)
            {
                OnEmoteRetargetedByAvatar[_Avatar][i]?.Invoke();
            }

            OnEmoteRetargetedByAvatar.Remove(_Avatar);
        }

        #endregion

        #region Memory

        public float GetSize()
        {
            float totalSize = 0.0f;
            foreach (EmoteRetargetedData dataCache in retargetedEmoteByAvatar.Values)
            {
                totalSize += dataCache.SizeInBytes;
            }

            return totalSize;
        }

        public void ClearAvatar(KinetixAvatar _Avatar)
        {

            if (IsRetargeting(_Avatar))
            {
                if (retargetedEmoteByAvatar[_Avatar].CancelToken != null)
                    retargetedEmoteByAvatar[_Avatar].CancelToken.canceled = true;
                return;
            }
            
            if (OnEmoteRetargetedByAvatar.ContainsKey(_Avatar))
                OnEmoteRetargetedByAvatar.Remove(_Avatar);

            if (retargetedEmoteByAvatar.ContainsKey(_Avatar))
            {
                EmoteRetargetedData retargetedData = retargetedEmoteByAvatar[_Avatar];
                retargetedEmoteByAvatar.Remove(_Avatar);

                MemoryManager.RemoveRamAllocation(retargetedData.SizeInBytes);
                MemoryManager.DeleteFileInRaM(retargetedData.AnimationClipLegacyRetargeted);
            }

            CheckAvatarsLeft();
        }

        private void CheckAvatarsLeft()
        {
            if (retargetedEmoteByAvatar.Count > 0)
                return;
            
            if (isLocal) return;
            
            MemoryManager.DeleteFileInStorage(Ids.UUID);
            RemoveDownloadFile();
        }

        public void ClearAllAvatars(KinetixAvatar[] _AvoidAvatars = null)
        {
            List<KinetixAvatar> avoidAvatars = new List<KinetixAvatar>();
            if (_AvoidAvatars != null)
            {
                avoidAvatars = _AvoidAvatars.ToList();
            }

            Dictionary<KinetixAvatar, EmoteRetargetedData> retargetedEmoteByAvatarCopy = new Dictionary<KinetixAvatar, EmoteRetargetedData>(retargetedEmoteByAvatar);
            foreach (KeyValuePair<KinetixAvatar, EmoteRetargetedData> kvp in retargetedEmoteByAvatarCopy)
            {
                if (!avoidAvatars.Exists(avatar => avatar.Equals(kvp.Key)))
                    ClearAvatar(kvp.Key);
            }
        }

        public void RemoveDownloadFile()
        {
            if (isLocal) return;
            
            pathGLB                  = string.Empty;
        }

        public void Destroy()
        {
            foreach (EmoteRetargetedData retargetedData in retargetedEmoteByAvatar.Values)
            {
                UnityEngine.Object.DestroyImmediate(retargetedData.AnimationClipLegacyRetargeted);
            }
        }

        #endregion
    }
}
