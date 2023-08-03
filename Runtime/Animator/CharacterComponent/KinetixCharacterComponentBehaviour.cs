// // ----------------------------------------------------------------------------
// // <copyright file="KinetixCharacterComponentBehaviour.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Kinetix
{
    /// <summary>
    /// Behaviour of the <see cref="KinetixCharacterComponent"/>
    /// </summary>
    public sealed class KinetixCharacterComponentBehaviour : MonoBehaviour
	{
		internal event Action OnUpdate;

        /// <summary>
        /// Character attached to the behaviour
        /// </summary>
		public KinetixCharacterComponent Character => _kcc;
		internal KinetixCharacterComponent _kcc;
        /// <summary>
        /// True if the character is local
        /// </summary>
		public bool IsLocal  => _kcc is KinetixCharacterComponentLocal;
		/// <summary>
		/// True if it's a remote character
		/// </summary>
		public bool IsRemote => _kcc is KinetixCharacterComponentRemote;

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        private void OnDestroy()
        {
            OnUpdate = null;
        }
    }
}
