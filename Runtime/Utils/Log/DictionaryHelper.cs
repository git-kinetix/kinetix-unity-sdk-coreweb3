using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Internal.Utils
{
	public static class DictionaryHelper
	{
		public static TValue ValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key, TValue defaultValue = default)
		{
			if (d == null) 
				throw new ArgumentNullException("d");
			
			if (key == null) 
				return defaultValue;

			if (d.TryGetValue(key, out TValue value))
				return value;
			return defaultValue;
		}
	}
}
