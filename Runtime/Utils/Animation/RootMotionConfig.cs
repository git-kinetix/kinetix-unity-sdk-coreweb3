// // ----------------------------------------------------------------------------
// // <copyright file="RootMotionConfig.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

namespace Kinetix
{
    public class RootMotionConfig
    {
        public bool ApplyHipsYPos = false;
        public bool ApplyHipsXAndZPos = false;
        public bool BackToInitialPose = false;
        public bool BakeIntoPoseXZ = false;
        public bool BakeIntoPoseY = false;

        public RootMotionConfig(bool ApplyHipsYPos = false, bool ApplyHipsXAndZPos = false, bool BackToInitialPose = false, bool BakeIntoPoseXZ = false, bool BakeIntoPoseY = false)
        {
            this.ApplyHipsYPos = ApplyHipsYPos;
            this.ApplyHipsXAndZPos = ApplyHipsXAndZPos;
            this.BackToInitialPose = BackToInitialPose;
            this.BakeIntoPoseXZ = BakeIntoPoseXZ;
            this.BakeIntoPoseY = BakeIntoPoseXZ;
        }
    }
}
