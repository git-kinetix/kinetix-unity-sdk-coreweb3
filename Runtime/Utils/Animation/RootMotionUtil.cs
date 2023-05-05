// // ----------------------------------------------------------------------------
// // <copyright file="RootMotionUtil.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using UnityEngine;

namespace Kinetix.Internal
{
    internal class RootMotionUtil
    {
        public RootMotionConfig Config;

        private Transform hips;
        private Transform root;
        
        private Vector3 hipsOriginalPosition;
        private Vector3 rootOriginalPosition;

        private Vector3 calculatedRootPosition = Vector3.zero;
        private Vector3 lastHipsPosition = Vector3.zero;
        

        internal RootMotionUtil(Transform hips, Transform root, RootMotionConfig config)
        {
            this.hips = hips;
            this.root = root;
            this.Config = config;

            SaveOffsets();
        }

        // TODO Add in doc to have an avatar with no rotation above armature
        internal void ProcessRootMotionAfterAnimSampling()
        {
            Quaternion armatureRotation = hips.parent.localRotation;

            // Handling root post-processing
            Vector3 newRootPosition = Vector3.zero;
            calculatedRootPosition = root.localRotation * ((armatureRotation * hips.localPosition) - lastHipsPosition);	

            if (!Config.BakeIntoPoseXZ && Config.ApplyHipsXAndZPos) {
                newRootPosition.x = calculatedRootPosition.x;
                newRootPosition.z = calculatedRootPosition.z;
            }

            if (!Config.BakeIntoPoseY && Config.ApplyHipsYPos) { 
                newRootPosition.y = calculatedRootPosition.y;
            }

            root.localPosition += newRootPosition;
            
            lastHipsPosition = armatureRotation * hips.localPosition;


            // Handling hips post-processing
            Vector3 newHipsPos = armatureRotation * hips.localPosition;

            
            if (Config.ApplyHipsXAndZPos) {
                newHipsPos.x = hipsOriginalPosition.x;
                
                newHipsPos.z = hipsOriginalPosition.z;
            }

            if (Config.ApplyHipsYPos) {
                newHipsPos.y = hipsOriginalPosition.y;
            }
            

            hips.localPosition = Quaternion.Inverse(armatureRotation) * newHipsPos;
        }


        internal void SaveOffsets()
        {       
            hipsOriginalPosition = hips.parent.localRotation * hips.localPosition;
            lastHipsPosition = hipsOriginalPosition;

            rootOriginalPosition = root.position;
        }



        internal void RevertToOffsets()
        {
            // Save current hips position before moving the root
            Vector3 hipsPos = hips.position;


            // Revert root position
            Vector3 newRootPos = root.position;

            if (Config.BackToInitialPose && Config.ApplyHipsXAndZPos) {
                newRootPos.x = rootOriginalPosition.x;
                newRootPos.z = rootOriginalPosition.z;
            }

            if (Config.ApplyHipsYPos) {
                newRootPos.y = rootOriginalPosition.y;
            }

            root.position = newRootPos;

            // Revert hips position
            Vector3 calcHipsPos = hipsPos;
            Vector3 newHipsPos = hips.position;

            if (Config.BackToInitialPose && Config.ApplyHipsXAndZPos) {
                newHipsPos.x = calcHipsPos.x;
                newHipsPos.z = calcHipsPos.z;
            }

            if (Config.ApplyHipsYPos) {
                newHipsPos.y = calcHipsPos.y;
            }

            hips.position = hipsPos;
            lastHipsPosition = hips.localPosition;            
        }
    }
}
