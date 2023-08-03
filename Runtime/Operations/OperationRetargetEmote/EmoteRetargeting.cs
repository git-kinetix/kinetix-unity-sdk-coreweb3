using System;
using System.Threading;
using System.Threading.Tasks;
using Kinetix.Internal.Retargeting;
using UnityEngine;

namespace Kinetix.Internal
{
    public class EmoteRetargeting<TResponseType, TExporter> : Operation<EmoteRetargetingConfig, EmoteRetargetingResponse<TResponseType>> where TExporter : ARetargetExport<TResponseType>, new()
    {
        public EmoteRetargeting(EmoteRetargetingConfig _Config): base(_Config) {}

        public override async Task Execute()
        {
            if (CancellationTokenSource.IsCancellationRequested || Config.CancellationSequencer.canceled)
            {
                CurrentTaskCompletionSource.TrySetCanceled();
                return;
            }

            CheckRetargeting();
            
            bool useWebRequest = false;
            
            RetargetingManager.GetRetargetedAnimationClip<TResponseType, TExporter>(
                            Config.Avatar.Avatar, Config.Path, Config.Priority, Config.CancellationSequencer, (clip, estimationSize) =>
            {
                if (CancellationTokenSource.IsCancellationRequested || Config.CancellationSequencer.canceled)
                {
                    
                    CurrentTaskCompletionSource.TrySetCanceled();
                    return;
                }
                
                EmoteRetargetingResponse<TResponseType> emoteRetargetingResponse = new EmoteRetargetingResponse<TResponseType>
                {
                    RetargetedClip = clip,
                    EstimatedClipSize = estimationSize
                };

                CurrentTaskCompletionSource.SetResult(emoteRetargetingResponse);
                    
                KinetixDebug.Log("Retargeted : " + Config.Emote.PathGLB);
            }, useWebRequest: useWebRequest);


            try
            {
                await CurrentTaskCompletionSource.Task;
            }
            catch (Exception)
            {
                
            }
        }

        private async void CheckRetargeting()
        {
            while (!CurrentTaskCompletionSource.Task.IsCompleted)
            {
                await Task.Yield();
                if (CancellationTokenSource.IsCancellationRequested || Config.CancellationSequencer.canceled)
                    CurrentTaskCompletionSource.TrySetCanceled();
            }
        }
        
        public override bool Compare(EmoteRetargetingConfig _Config)
        {
            return Config.Path.Equals(_Config.Path);
        }

        public override IOperation<EmoteRetargetingConfig, EmoteRetargetingResponse<TResponseType>> Clone()
        {
            return new EmoteRetargeting<TResponseType, TExporter>(Config);
        }
    }
}

