using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kinetix.Internal.Retargeting;
using Kinetix.Utils;
using UnityEngine;

namespace Kinetix.Internal
{
    public class RetargetingService: IKinetixService
    {
        private readonly Dictionary<KinetixEmoteAvatarPair, EmoteRetargetedData> retargetedEmoteByAvatar;
        private readonly Dictionary<KinetixEmoteAvatarPair, List<Action>>        OnEmoteRetargetedByAvatar;
        private ServiceLocator serviceLocator;

        public RetargetingService(ServiceLocator _ServiceLocator)
        {
            retargetedEmoteByAvatar = new Dictionary<KinetixEmoteAvatarPair, EmoteRetargetedData>();
            OnEmoteRetargetedByAvatar = new Dictionary<KinetixEmoteAvatarPair, List<Action>>();

            serviceLocator = _ServiceLocator;
        }

        /// <summary>
        /// Check if GLB file is used (e.g. Import Retargeting)
        /// </summary>
        /// <returns>True if use</returns>
        public bool IsFileInUse()
        {
            return true;
        }

        /// <summary>
        /// If the emote / avatar pair is in cache, returns it
        /// </summary>
        /// <param name="_Emote"></param>
        /// <param name="_Avatar"></param>
        /// <returns></returns>
        public bool HasAnimationRetargeted(KinetixEmote _Emote, KinetixAvatar _Avatar)
        {
            KinetixEmoteAvatarPair pair = new KinetixEmoteAvatarPair() { Emote = _Emote, Avatar = _Avatar};

            if (!retargetedEmoteByAvatar.ContainsKey(pair))
                return false;

            bool hasAnimationRetargeted = retargetedEmoteByAvatar[pair].clipsByType.Values.ToList().Exists(retargetedEmote => retargetedEmote.HasClip());
            return hasAnimationRetargeted;
        }


        #region Notifications

        /// <summary>
        /// Allows for callback registration for when a emote / avatar combination will be retargeted
        /// </summary>
        /// <param name="_Emote"></param>
        /// <param name="_Avatar"></param>
        /// <param name="_OnSucceed"></param>
        public void RegisterCallbacksOnRetargetedByAvatar(KinetixEmote _Emote, KinetixAvatar _Avatar, Action _OnSucceed)
        {
            KinetixEmoteAvatarPair pair = new KinetixEmoteAvatarPair() { Emote = _Emote, Avatar = _Avatar};

            if (HasAnimationRetargeted(_Emote, _Avatar))
            {
                _OnSucceed?.Invoke();
                return;
            }
            
            if (!OnEmoteRetargetedByAvatar.ContainsKey(pair))
                OnEmoteRetargetedByAvatar.Add(pair, new List<Action>());

            OnEmoteRetargetedByAvatar[pair].Add(_OnSucceed);
        }

        /// <summary>
        /// Callback calling for a emote / avatar combination
        /// </summary>
        /// <param name="_Emote"></param>
        /// <param name="_Avatar"></param>
        private void NotifyCallbackOnRetargetedByAvatar(KinetixEmote _Emote, KinetixAvatar _Avatar)
        {
            KinetixEmoteAvatarPair pair = new KinetixEmoteAvatarPair() { Emote = _Emote, Avatar = _Avatar};

            if (!OnEmoteRetargetedByAvatar.ContainsKey(pair))
                return;
            
            for (int i = 0; i < OnEmoteRetargetedByAvatar[pair].Count; i++)
            {
                OnEmoteRetargetedByAvatar[pair][i]?.Invoke();
            }

            OnEmoteRetargetedByAvatar.Remove(pair);
        }

        #endregion

        /// <summary>
        /// Get an KinetixClip retargeted for a specific avatar
        /// </summary>
        /// <param name="_Avatar">The Avatar</param>
        /// <param name="_Priority">The priority in the retargeting queue</param>
        /// <param name="_Force">Force the retargeting even if we exceed memory.
        /// /!\ Be really cautious with this parameter as we keep a stable memory management.
        /// It is only use to for the emote played by the local player.
        /// </param>
        /// <returns>The KinetixClip for the specific Avatar</returns>
        public async Task<TResponseType> GetRetargetedClipByAvatar<TResponseType, THandler>(KinetixEmote _Emote, KinetixAvatar _Avatar, SequencerPriority _Priority, bool _Force) where THandler : ARetargetExport<TResponseType>, new()
        {
            if (!_Force && serviceLocator.Get<MemoryService>().HasStorageExceedMemoryLimit())
                throw new Exception("Not enough storage space available to retarget : " + _Emote.Ids.UUID);

            if (!_Force && serviceLocator.Get<MemoryService>().HasRAMExceedMemoryLimit())
                throw new Exception("Not enough RAM space to retarget : " + _Emote.Ids.UUID);

            KinetixEmoteAvatarPair pair = new KinetixEmoteAvatarPair() { Emote = _Emote, Avatar = _Avatar};
            EmoteRetargetingClipResult<TResponseType> castedClipResult = null;


            if (retargetedEmoteByAvatar.ContainsKey(pair) && retargetedEmoteByAvatar[pair].clipsByType.ContainsKey(typeof(TResponseType)))
            {
                castedClipResult = (EmoteRetargetingClipResult<TResponseType>) retargetedEmoteByAvatar[pair].clipsByType[typeof(TResponseType)];

                await castedClipResult.Task.Task;

                return (TResponseType) castedClipResult.Clip;
            }
            
            // In addition to tyhe CancellationTokenSource of the operation, we add a SequencerCancel, used by the retargeting system itself
            SequencerCancel sequencerCancelToken = new SequencerCancel();
            CancellationTokenSource cancellationTokenFileDownload = new CancellationTokenSource();

            // First get the GLB file path to give it to the retargeter
            string path = string.Empty;
            
            if (!retargetedEmoteByAvatar.ContainsKey(pair))
            {
                retargetedEmoteByAvatar.Add(pair, new EmoteRetargetedData()
                {
                    CancellationTokenFileDownload = cancellationTokenFileDownload,
                    SequencerCancelToken = sequencerCancelToken
                });
            }

            if (!retargetedEmoteByAvatar[pair].clipsByType.ContainsKey(typeof(TResponseType)))
            {
                TaskCompletionSource<EmoteRetargetingResponse<TResponseType>> tcsObj = new TaskCompletionSource<EmoteRetargetingResponse<TResponseType>>();
                retargetedEmoteByAvatar[pair].clipsByType[typeof(TResponseType)] = new EmoteRetargetingClipResult<TResponseType>(EProgressStatus.PENDING, tcsObj);
            }


            try
            {
                path = await GetFilePath(_Emote, cancellationTokenFileDownload);
            }
            catch (OperationCanceledException e)
            {
                _Emote.PathGLB = string.Empty;

                if (retargetedEmoteByAvatar.ContainsKey(pair))
                    retargetedEmoteByAvatar.Remove(pair);
                throw e;
            }
            catch (Exception e)
            {
                // Independent of the wanted return type, if we can't find the GLB file 
                // we want to empty the cache as it is a global problem with the emote
                _Emote.PathGLB = string.Empty;

                if (retargetedEmoteByAvatar.ContainsKey(pair))
                    retargetedEmoteByAvatar.Remove(pair);
                    
                throw e;
            }
            
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("GLB is not available");
            }

            // If while the file path operation was executed we have a previous operation result to get, give that result instead of launching a new op
            if (retargetedEmoteByAvatar[pair].clipsByType.ContainsKey(typeof(TResponseType)))
            {
                castedClipResult = (EmoteRetargetingClipResult<TResponseType>) retargetedEmoteByAvatar[pair].clipsByType[typeof(TResponseType)];
            }
            
            if (castedClipResult != null && castedClipResult.Clip != null)
            {
                return (TResponseType) castedClipResult.Clip;
            }

            try
            {
                // Now we can create the retargeting operation itself, that will smartly handle the retargeting of the emote
                EmoteRetargetingResponse<TResponseType> response = await RequestOperationExecution<TResponseType, THandler>(pair, _Priority, path, sequencerCancelToken);
                
                if (castedClipResult == null && retargetedEmoteByAvatar[pair].clipsByType.ContainsKey(typeof(TResponseType)))
                {
                    castedClipResult = (EmoteRetargetingClipResult<TResponseType>) retargetedEmoteByAvatar[pair].clipsByType[typeof(TResponseType)];
                }

                // If the current call is the first to resolve the operation
                if (castedClipResult != null && castedClipResult.Clip == null)
                {
                    // Once the operation is finished we cache the result
                    castedClipResult.Clip = response.RetargetedClip;
                    castedClipResult.Status = EProgressStatus.COMPLETED;

                    retargetedEmoteByAvatar[pair].clipsByType[typeof(TResponseType)] = castedClipResult;
                    retargetedEmoteByAvatar[pair].SizeInBytes = response.EstimatedClipSize;

                    serviceLocator.Get<MemoryService>().AddRamAllocation(response.EstimatedClipSize);    
                }

                // And invoke callbacks in case they were awaited by UI before the core was initialized
                NotifyCallbackOnRetargetedByAvatar(_Emote, _Avatar);

                // Qualify the task as done
                return castedClipResult.Clip;
            }
            catch (OperationCanceledException e)
            {
                if (retargetedEmoteByAvatar.ContainsKey(pair) && retargetedEmoteByAvatar[pair].clipsByType.Count == 0)
                    retargetedEmoteByAvatar.Remove(pair);

                throw e;
            }
            catch (Exception e)
            {
                if (retargetedEmoteByAvatar.ContainsKey(pair))
                    retargetedEmoteByAvatar.Remove(pair);
                    
                throw e;
            }    
        }

        /// <summary>
        /// Requests the operation itself
        /// </summary>
        /// <param name="_Emote"></param>
        /// <param name="_Avatar"></param>
        /// <param name="_Priority"></param>
        /// <param name="path"></param>
        /// <param name="cancelToken"></param>
        /// <typeparam name="TResponseType"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <returns></returns>
        private async Task<EmoteRetargetingResponse<TResponseType>> RequestOperationExecution<TResponseType, THandler>(KinetixEmoteAvatarPair _EmoteAvatarPair, SequencerPriority _Priority, string path, SequencerCancel cancelToken) where THandler : ARetargetExport<TResponseType>, new()
        {
            EmoteRetargetingConfig emoteRetargetingConfig = new EmoteRetargetingConfig(_EmoteAvatarPair.Emote, _EmoteAvatarPair.Avatar, _Priority, path, cancelToken);
            EmoteRetargeting<TResponseType, THandler> emoteRetargeting = new EmoteRetargeting<TResponseType, THandler>(emoteRetargetingConfig);
        
            retargetedEmoteByAvatar[_EmoteAvatarPair].CancellationTokenFileDownload = emoteRetargeting.CancellationTokenSource;

            EmoteRetargetingResponse<TResponseType> response = await OperationManagerShortcut.Get().RequestExecution<EmoteRetargetingConfig, EmoteRetargetingResponse<TResponseType>>(emoteRetargeting);
        
            EmoteRetargetingClipResult<TResponseType> castedClipResult = (EmoteRetargetingClipResult<TResponseType>) retargetedEmoteByAvatar[_EmoteAvatarPair].clipsByType[typeof(TResponseType)];
            castedClipResult.Task.SetResult(response);

            return response;
        }

        /// <summary>
        /// Checks if the emote's GLB file is valid and, if needed, asks for it's downloading
        /// </summary>
        /// <param name="_Emote"></param>
        /// <returns></returns>
        public async Task<string> GetFilePath(KinetixEmote _Emote, CancellationTokenSource cancellationToken)
        {
            if (_Emote.HasValidPath())
                return _Emote.PathGLB;

            try
            {
                string filename = _Emote.Ids.UUID + ".glb";
                string filePath = Path.Combine(PathConstants.CacheAnimationsPath, filename);

                if (!_Emote.HasMetadata())
                    throw new Exception("No metadata found for emote : " + _Emote.Ids.UUID);

                FileDownloaderConfig fileDownloadOperationConfig = new FileDownloaderConfig(_Emote.Metadata.AnimationURL, filePath);
                FileDownloader fileDownloadOperation = new FileDownloader(fileDownloadOperationConfig);

                FileDownloaderResponse response = await OperationManagerShortcut.Get().RequestExecution<FileDownloaderConfig, FileDownloaderResponse>(fileDownloadOperation, cancellationToken);

                _Emote.PathGLB = response.path;
            }
            catch (OperationCanceledException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
            
            serviceLocator.Get<MemoryService>().AddStorageAllocation(_Emote.PathGLB);
            return _Emote.PathGLB;
        }


        #region Memory

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_EmoteAvatarPair"></param>
        public void ClearAvatar(KinetixEmoteAvatarPair _EmoteAvatarPair)
        {
            ClearAvatar(_EmoteAvatarPair.Emote, _EmoteAvatarPair.Avatar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_Emote"></param>
        /// <param name="_Avatar"></param>
        public void ClearAvatar(KinetixEmote _Emote, KinetixAvatar _Avatar)
        {
            if (_Avatar == null)
                return;

            KinetixEmoteAvatarPair pair = new KinetixEmoteAvatarPair() { Emote = _Emote, Avatar = _Avatar};
            
            if (!retargetedEmoteByAvatar.ContainsKey(pair))
                return;

            if (OnEmoteRetargetedByAvatar.ContainsKey(pair))
                OnEmoteRetargetedByAvatar.Remove(pair);

            retargetedEmoteByAvatar[pair].CancellationTokenFileDownload?.Cancel();
            retargetedEmoteByAvatar[pair].SequencerCancelToken?.Cancel();
            
            EmoteRetargetedData retargetedData = retargetedEmoteByAvatar[pair];
            foreach (EmoteRetargetingClipResult emoteRetargetingClipResult in retargetedData.clipsByType.Values)
            {
                emoteRetargetingClipResult.Dispose();
            }
            
            serviceLocator.Get<MemoryService>().RemoveRamAllocation(retargetedData.SizeInBytes);
            serviceLocator.Get<MemoryService>().DeleteFileInStorage(pair.Emote.Ids.UUID);

            KinetixDebug.Log("[UNLOADED] Animation : " + pair.Emote.Ids);

            retargetedEmoteByAvatar.Remove(pair);
            _Emote.PathGLB = null;
        }

        public void ClearAllAvatars(KinetixAvatar[] _AvoidAvatars = null)
        {
            List<KinetixAvatar> avoidAvatars = new List<KinetixAvatar>();
            if (_AvoidAvatars != null)
            {
                avoidAvatars = _AvoidAvatars.ToList();
            }

            Dictionary<KinetixEmoteAvatarPair, EmoteRetargetedData> retargetedEmoteByAvatarCopy =
                new Dictionary<KinetixEmoteAvatarPair, EmoteRetargetedData>(retargetedEmoteByAvatar);
            foreach (KeyValuePair<KinetixEmoteAvatarPair, EmoteRetargetedData> kvp in retargetedEmoteByAvatarCopy)
            {
                if (!avoidAvatars.Exists(avatar => avatar.Equals(kvp.Key)))
                    ClearAvatar(kvp.Key.Emote, kvp.Key.Avatar);
            }
        }
        
        #endregion
    }
}
