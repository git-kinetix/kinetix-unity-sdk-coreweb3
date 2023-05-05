


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;

namespace Kinetix.Internal
{
    internal class NetworkPoseSampler
    {
        private const    float                      LERP_SMOOTHNESS = 0.25f;
        private readonly ReadOnlyCollection<string> humans          = Sam.HUMANS;

        private List<Transform> boneTransforms;
        private Queue<KinetixNetworkedPose> targetTimestampPoseQueue;
        private KinetixNetworkedPose currentPose;

        private double startOffset = -1f;
        private double lastTimestamp = -1f;
        private double currentTimestamp = -1f;
        
        private bool isPlaying = false;

        private Guid lastAnimGuid = Guid.Empty;
        private Guid currentAnimGuid = Guid.Empty;

        private Coroutine interpolationRoutine = null;
        private BonePoseInfo[] bonePoseInfos;

        private KinetixCharacterComponent kcc;
        private ClipSampler clipSampler;
        private Animator animator;
        private KinetixNetworkConfiguration config;
        private bool startingNewAnim = true;


        internal NetworkPoseSampler(KinetixCharacterComponent parent, Animator animator, ClipSampler clipSampler)
        {
            boneTransforms = new List<Transform>();
            targetTimestampPoseQueue = new Queue<KinetixNetworkedPose>();

            for (int t = 0; t < humans.Count; t++)
            {
                HumanBodyBones humanBodyBones = (HumanBodyBones) Enum.Parse(typeof(HumanBodyBones), humans[t].Replace(" ", ""));
                Transform bone = animator.GetBoneTransform(humanBodyBones);

                if (bone != null) 
                {
                    boneTransforms.Add(bone);
                }
            }

            this.clipSampler = clipSampler;
            this.animator = animator;
            this.kcc = parent;

            config = NetworkManager.Configuration;
            bonePoseInfos = new BonePoseInfo[boneTransforms.Count];
        }

        internal void StartPose()
        {
            isPlaying = true;
            currentAnimGuid = Guid.NewGuid();
        }

        internal void StopPose()
        {
            isPlaying = false;
            lastAnimGuid = currentAnimGuid;
            currentAnimGuid = Guid.Empty;
        }

        public void ApplyPose(KinetixNetworkedPose pose, double timestamp)
        {
            isPlaying = pose != null;
            
            // If suddenly we receive a null pose, that means the anim is finished
            if (!isPlaying) 
            {
                lastAnimGuid = currentAnimGuid;
                currentAnimGuid = Guid.Empty;
                return;
            }
            
            // If we receive frames of a killed anim, ignore and return
            if (pose.guid == lastAnimGuid) return;

            currentAnimGuid = pose.guid;
            pose.timestamp = timestamp;
    
            // Start the coroutine and reset params
            if (interpolationRoutine == null) 
            {
                startOffset = 0;
                lastTimestamp = timestamp;
                currentTimestamp = timestamp;
                currentPose = null;
                targetTimestampPoseQueue = new Queue<KinetixNetworkedPose>();
                
                interpolationRoutine = kcc.StartCoroutine(InterpolateBonePosition());
            }

            // This is the queue for all anim frames and their timestamp
            targetTimestampPoseQueue.Enqueue(pose);
        }

        public KinetixNetworkedPose GetPose()
        {
            if (!isPlaying) 
            {
                startingNewAnim = true;
                return null;
            }

            if (boneTransforms == null || boneTransforms.Count == 0) 
            {
                startingNewAnim = true;
                return null;
            }

            bool posEnabled = false; 
            bool scaleEnabled = false; 

            for (int i = 0; i < boneTransforms.Count; i++) 
            {
                if (boneTransforms[i] == null) continue;


                if (!startingNewAnim) 
                {
                    if (config.SendLocalPosition) 
                    {
                        posEnabled = posEnabled || bonePoseInfos[i].localPosition == boneTransforms[i].localPosition;
                    }

                    if (config.SendLocalScale) 
                    {
                        scaleEnabled = scaleEnabled || bonePoseInfos[i].localScale == boneTransforms[i].localScale;
                    }
                }

                bonePoseInfos[i] = new BonePoseInfo(boneTransforms[i]);
            }

            startingNewAnim = false;

            return new KinetixNetworkedPose { 
                bones = bonePoseInfos, 
                guid = currentAnimGuid,
                posEnabled = posEnabled,
                scaleEnabled = scaleEnabled,
            };
        }

        private IEnumerator InterpolateBonePosition()
        {
            double ratio = 0;
            bool hasReachedEnd = false;
            float ellapsedSinceLast = 0;
    
            while (!hasReachedEnd)
            {
                // Increase timers
                currentTimestamp = currentTimestamp + Time.deltaTime;
                ellapsedSinceLast += Time.deltaTime;

                // Cumulate some upfront frames before triggering
                if (targetTimestampPoseQueue.Count < config.TargetFrameCacheCount && isPlaying) 
                {
                    
                    // If we waited the configured delay for some more frames and nothing is coming, kill the anim
                    if (currentPose != null && ellapsedSinceLast > config.MaxWaitTimeBeforeEmoteExpiration) 
                    {
                        hasReachedEnd = true;
                        isPlaying = false;
                        clipSampler.ForceBlendOut();
                        break;
                    }

                    if (currentPose == null) 
                    {
                        startOffset += Time.deltaTime;

                        yield return new WaitForEndOfFrame();
                        continue;
                    }
                }

                // Get the next frame
                if (currentPose == null || currentPose.timestamp + startOffset - lastTimestamp <= currentTimestamp - lastTimestamp) 
                {

                    // If it was the last frame, ciao bye 
                    if (targetTimestampPoseQueue.Count == 0) 
                    {
                        hasReachedEnd = true;
                        break;
                    }

                    // If we are not at the first frame of the anim, just save the previous timestamp and set the ellapsed time to 0
                    if (currentPose != null) 
                    {
                        lastTimestamp = currentPose.timestamp;
                        ellapsedSinceLast = 0;
                    }

                    currentPose = targetTimestampPoseQueue.Dequeue();

                    if (animator.enabled) 
                    {
                        clipSampler.DisableAnimator();
                    }
                }

                // Calculate interpolation ratio
                ratio = LERP_SMOOTHNESS;

                float fRatio = Mathf.Abs((float) ratio);

                // Apply interpolation for each bone
                for (int boneIndex = 0; boneIndex < currentPose.bones.Length; boneIndex++)
                {
                    if (currentPose.bones[boneIndex] != null && currentPose.bones[boneIndex].localPosition != null) 
                    {
                        
                        boneTransforms[boneIndex].localRotation = Quaternion.Lerp(boneTransforms[boneIndex].localRotation, currentPose.bones[boneIndex].localRotation, fRatio);
                        
                        if (currentPose.posEnabled || boneIndex == 0)
                            boneTransforms[boneIndex].localPosition = Vector3.Lerp(boneTransforms[boneIndex].localPosition, currentPose.bones[boneIndex].localPosition, fRatio);
                        
                        if (currentPose.scaleEnabled)
                            boneTransforms[boneIndex].localScale = Vector3.Lerp(boneTransforms[boneIndex].localScale, currentPose.bones[boneIndex].localScale, fRatio);
                    }
                }

                yield return new WaitForEndOfFrame();
            }
            

            if (currentAnimGuid != Guid.Empty) 
            {
                lastAnimGuid = currentAnimGuid;
                currentAnimGuid = Guid.Empty;
            }
            
            // As clip blending is accounted for in the networked pose, we can just enable back the animator
            animator.enabled = true;
            interpolationRoutine = null;
            targetTimestampPoseQueue = new Queue<KinetixNetworkedPose>();
        }
    }
}
