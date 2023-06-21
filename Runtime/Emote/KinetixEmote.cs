using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kinetix.Internal.Retargeting;
using UnityEngine;

namespace Kinetix.Internal
{
    public class KinetixEmote
    {
        public AnimationIds      Ids      { get; }
        public AnimationMetadata Metadata { get; private set; }

        private string pathGLB;

        private readonly Dictionary<KinetixAvatar, EmoteRetargetedData> retargetedEmoteByAvatar;
        private readonly Dictionary<KinetixAvatar, List<Action>>        OnEmoteRetargetedByAvatar;
        private          List<string>                                   locks;

        public KinetixEmote(AnimationIds _Ids)
        {
            Ids                       = _Ids;
            retargetedEmoteByAvatar   = new Dictionary<KinetixAvatar, EmoteRetargetedData>();
            OnEmoteRetargetedByAvatar = new Dictionary<KinetixAvatar, List<Action>>();
            locks                     = new List<string>();
        }

        public void Lock(string lockId)
        {
            if (string.IsNullOrEmpty(lockId))
                return;

            if (locks.Contains(lockId))
                return;

            KinetixDebug.Log("[LOCK] Animation : " + Ids + " by " + lockId);

            locks.Add(lockId);
        }

        public void Unlock(string lockId, KinetixAvatar avatar)
        {
            if (!locks.Contains(lockId))
            {
                KinetixDebug.LogWarning("Tried to unlock emote " + Ids +
                                        " but it wasn't locked anymore and should have been disposed already.");
                return;
            }

            locks.Remove(lockId);

            KinetixDebug.Log("[UNLOCK] Animation : " + Ids + ". Locks left: " + locks.Count);

            if (locks.Count == 0)
                Unload(avatar);
        }

        public void ForceUnload(KinetixAvatar avatar)
        {
            KinetixDebug.Log("[FORCE UNLOAD] Animation : " + Ids);
            Unload(avatar);
        }

        private void Unload(KinetixAvatar _Avatar)
        {
            ClearAvatar(_Avatar);
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
        /// Check if GLB file is used (e.g. Import Retargeting)
        /// </summary>
        /// <returns>True if use</returns>
        public bool IsFileInUse()
        {
            return retargetedEmoteByAvatar.Values.Any(retargetedData =>
                retargetedData.ProgressStatus != EProgressStatus.COMPLETED);
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
            if (string.IsNullOrEmpty(pathGLB))
                return false;
            return File.Exists(pathGLB);
        }

        #region Async Requests

        private async Task<string> GetFilePath(SequencerCancel cancelToken)
        {
            if (HasValidPath())
                return pathGLB;

            try
            {
                pathGLB = await FileOperationManager.DownloadGLBByEmote(this, cancelToken);
            }
            catch (OperationCanceledException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
            
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
        public async Task<AnimationClip> GetRetargetedAnimationClipByAvatar(KinetixAvatar _Avatar,
            SequencerPriority                                                             _Priority, bool _Force)
        {
            if (!_Force && MemoryManager.HasStorageExceedMemoryLimit())
                throw new Exception("Not enough storage space available to retarget : " + Ids.UUID);

            if (!_Force && MemoryManager.HasRAMExceedMemoryLimit())
                throw new Exception("Not enough RAM space to retarget : " + Ids.UUID);

            if (!HasMetadata())
                throw new Exception("No metadata found for emote : " + Ids.UUID);

            if (!retargetedEmoteByAvatar.ContainsKey(_Avatar))
            {
                retargetedEmoteByAvatar.Add(_Avatar, new EmoteRetargetedData()
                {
                    ProgressStatus = EProgressStatus.NONE
                });
            }

            TaskCompletionSource<AnimationClip> tcs = new TaskCompletionSource<AnimationClip>();

            switch (retargetedEmoteByAvatar[_Avatar].ProgressStatus)
            {
                case EProgressStatus.COMPLETED:
                    return retargetedEmoteByAvatar[_Avatar].AnimationClipLegacyRetargeted;
                case EProgressStatus.PENDING:
                    RegisterCallbacksOnRetargetedByAvatar(_Avatar,
                        () => { tcs.SetResult(retargetedEmoteByAvatar[_Avatar].AnimationClipLegacyRetargeted); });
                    break;
                case EProgressStatus.NONE:
                {
                    if (MemoryManager.HasStorageExceedMemoryLimit())
                        throw new Exception("Not enough storage space available to retarget : " + Ids.UUID);

                    if (MemoryManager.HasRAMExceedMemoryLimit())
                        throw new Exception("Not enough RAM space to retarget : " + Ids.UUID);
                    
                    SequencerCancel cancelToken = new SequencerCancel();
                    retargetedEmoteByAvatar[_Avatar].CancelToken    = cancelToken;
                    retargetedEmoteByAvatar[_Avatar].ProgressStatus = EProgressStatus.PENDING;

                    string path;
                    try
                    {
                        path = await GetFilePath(cancelToken);
                    }
                    catch (OperationCanceledException e)
                    {
                        throw e;
                    }
                    catch (Exception e)
                    {
                        pathGLB = string.Empty;
                        if (retargetedEmoteByAvatar.ContainsKey(_Avatar))
                            retargetedEmoteByAvatar[_Avatar].ProgressStatus = EProgressStatus.NONE;
                        throw e;
                    }
                    
                    if (string.IsNullOrEmpty(path))
                    {
                        retargetedEmoteByAvatar[_Avatar].ProgressStatus = EProgressStatus.NONE;
                        throw new Exception("GLB is not available");
                    }

                    bool useWebRequest = false;
                    
                    if (cancelToken.canceled)
                    {
                        retargetedEmoteByAvatar[_Avatar].ProgressStatus = EProgressStatus.NONE;
                        throw new Exception($"Retargeting for emote {Ids.UUID} was cancelled");
                    }

                    RetargetingManager.GetRetargetedAnimationClip<AnimationClip, AnimationClipExport>(
                        _Avatar.Avatar, _Avatar.Root, path, _Priority, cancelToken, (clip, estimationSize) =>
                        {
                            if (!retargetedEmoteByAvatar.ContainsKey(_Avatar))
                            {
                                // We delete clip as we dont need it anymore
                                retargetedEmoteByAvatar[_Avatar].ProgressStatus = EProgressStatus.NONE;
                                MemoryManager.DeleteFileInRaM(clip);
                                tcs.SetException(
                                    new Exception("AnimationClip is not required anymore after retargeting"));
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
                                    tcs.SetException(
                                        new Exception("Not enough RAM space to retarget : " + Ids.UUID));
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
            if (_Avatar == null)
                return;
            
            if (!retargetedEmoteByAvatar.ContainsKey(_Avatar))
                return;

            if (OnEmoteRetargetedByAvatar.ContainsKey(_Avatar))
                OnEmoteRetargetedByAvatar.Remove(_Avatar);

            if (retargetedEmoteByAvatar[_Avatar].ProgressStatus != EProgressStatus.COMPLETED)
            {
                if (retargetedEmoteByAvatar[_Avatar].CancelToken != null &&
                    !retargetedEmoteByAvatar[_Avatar].CancelToken.canceled)
                {
                    retargetedEmoteByAvatar[_Avatar].ProgressStatus = EProgressStatus.NONE;
                    retargetedEmoteByAvatar[_Avatar].CancelToken.Cancel();
                }
            }
            else
            {
                EmoteRetargetedData retargetedData = retargetedEmoteByAvatar[_Avatar];
                MemoryManager.RemoveRamAllocation(retargetedData.SizeInBytes);
                MemoryManager.DeleteFileInRaM(retargetedData.AnimationClipLegacyRetargeted);
            }

            retargetedEmoteByAvatar.Remove(_Avatar);
            pathGLB = null;
            MemoryManager.DeleteFileInStorage(Ids.UUID);
        }

        public void ClearAllAvatars(KinetixAvatar[] _AvoidAvatars = null)
        {
            List<KinetixAvatar> avoidAvatars = new List<KinetixAvatar>();
            if (_AvoidAvatars != null)
            {
                avoidAvatars = _AvoidAvatars.ToList();
            }

            Dictionary<KinetixAvatar, EmoteRetargetedData> retargetedEmoteByAvatarCopy =
                new Dictionary<KinetixAvatar, EmoteRetargetedData>(retargetedEmoteByAvatar);
            foreach (KeyValuePair<KinetixAvatar, EmoteRetargetedData> kvp in retargetedEmoteByAvatarCopy)
            {
                if (!avoidAvatars.Exists(avatar => avatar.Equals(kvp.Key)))
                    ClearAvatar(kvp.Key);
            }
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
