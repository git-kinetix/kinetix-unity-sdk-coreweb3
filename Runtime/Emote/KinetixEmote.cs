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
	public class KinetixEmote
	{
		public AnimationIds      Ids      { get; }
		public AnimationMetadata Metadata { get; private set; }

        public string PathGLB;


        public KinetixEmote(AnimationIds _Ids)
        {
            Ids                       = _Ids;
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
        /// Check if emote has a valid GLB Path in storage in order to import it
        /// </summary>
        /// <returns>True if path exists</returns>
        public bool HasValidPath()
        {            
            if (string.IsNullOrEmpty(PathGLB))
                return false;
                
            return File.Exists(PathGLB);
        }

        public bool IsFileInUse()
        {
            return KinetixCoreBehaviour.ServiceLocator.Get<RetargetingService>().IsFileInUse();
        }

        public void ClearAvatar(KinetixAvatar _Avatar)
        {
            KinetixCoreBehaviour.ServiceLocator.Get<RetargetingService>().ClearAvatar(this, _Avatar);
        }

        public void ClearAllAvatars(KinetixAvatar[] _AvoidAvatars = null)
        {
            KinetixCoreBehaviour.ServiceLocator.Get<RetargetingService>().ClearAllAvatars(_AvoidAvatars);
        }

        public bool HasAnimationRetargeted(KinetixAvatar _Avatar)
        {
            return KinetixCoreBehaviour.ServiceLocator.Get<RetargetingService>().HasAnimationRetargeted(this, _Avatar);
        }
    }
}
