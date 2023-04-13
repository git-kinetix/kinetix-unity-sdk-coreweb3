using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kinetix.Internal
{
    public class KinetixAvatar
    {
        public Avatar    Avatar;
        public Transform Root;

        public EExportType ExportType;
        
        #region Equals
        public override bool Equals(object obj)
        {
            if ((obj == null) || GetType() != obj.GetType())
                return false;
            KinetixAvatar kAvatar = (KinetixAvatar)obj;
            return (Avatar.GetInstanceID() == kAvatar.Avatar.GetInstanceID());
        }

        protected bool Equals(KinetixAvatar other)
        {
            if (other == null)
                return false;
            return Avatar.GetInstanceID() == other.Avatar.GetInstanceID() ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Avatar != null ? Avatar.GetHashCode() : 0);
                return hashCode;
            }
        }
        #endregion
    }
}

