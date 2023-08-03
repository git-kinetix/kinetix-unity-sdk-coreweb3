// // ----------------------------------------------------------------------------
// // <copyright file="KinetixClipDictionary.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kinetix
{
	/// <summary>
	/// A dictionary outliner for the <see cref="KinetixClip"/>.
	/// </summary>
	/// <typeparam name="TKey">Key of the dictionary</typeparam>
	/// <typeparam name="TValue">Value of the dictionary</typeparam>
	/// <remarks>
	/// The accessor is reaonly. To write in the dictionary, please refer to methods in <see cref="KinetixClip"/>
	/// </remarks>
	public class KinetixClipDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
	{
		private readonly Dictionary<TKey, TValue> dictionary;

        public KinetixClipDictionary()
        {
            dictionary = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Get a value in the dictionary.
        /// </summary>
        /// <remarks>
        /// Setting the dictionary directly is prohibited. Use a method in <see cref="KinetixClip"/> to set the value
        /// </remarks>
        public TValue this[TKey key]
        {
            get
			{
				if (!dictionary.ContainsKey(key))
					throw new KeyNotFoundException($"The key '{key}' was not present in the dictionary.");

				return dictionary[key];
			}

            internal set => dictionary[key] = value;
        }

        public Dictionary<TKey, TValue> GetDictionary() => dictionary;

		//----------------------------//
		// Dictionary methods replica //
		//----------------------------//
		#region Dictionary methods replica
		public bool TryGetValue(TKey key, out TValue value) => dictionary.TryGetValue(key, out value);

		public Dictionary<TKey, TValue>.KeyCollection Keys => dictionary.Keys;

		public Dictionary<TKey, TValue>.ValueCollection Values => dictionary.Values;

		public int Count => dictionary.Count;

        public void Clear() => dictionary.Clear();

        public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

        public bool ContainsValue(TValue value) => dictionary.ContainsValue(value);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => dictionary.GetEnumerator();
		#endregion
		//----------------------------//

		//----------------------------//
		//            Cast            //
		//----------------------------//
		public static explicit operator Dictionary<TKey, TValue>(KinetixClipDictionary<TKey, TValue> dictionary) => dictionary.dictionary;
		//----------------------------//
	}
}
