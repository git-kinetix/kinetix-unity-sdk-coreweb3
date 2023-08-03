using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kinetix.Internal
{
	public class KinetixAvatar
	{
		public AvatarData Avatar;
		public Transform Root;

		public EExportType ExportType;

		#region Equals
		public override bool Equals(object obj)
		{
			if ((obj == null) || GetType() != obj.GetType())
				return false;
			KinetixAvatar kAvatar = (KinetixAvatar)obj;
			return (Avatar.id == kAvatar.Avatar.id);
		}

		protected bool Equals(KinetixAvatar other)
		{
			if (other == null)
				return false;
			return Avatar.id == other.Avatar.id;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = Avatar.id;
				return hashCode;
			}
		}
		#endregion
	}
}

