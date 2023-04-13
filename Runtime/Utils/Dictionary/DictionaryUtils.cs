using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kinetix.Internal
{
    public static class DictionaryUtils
    {
        public static bool TryToAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }
    }
}

