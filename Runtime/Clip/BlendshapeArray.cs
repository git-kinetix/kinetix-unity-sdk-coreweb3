// // ----------------------------------------------------------------------------
// // <copyright file="KinetixPose.cs" company="Kinetix SAS">
// // Kinetix Unity SDK - Copyright (C) 2022 Kinetix SAS
// // </copyright>
// // ----------------------------------------------------------------------------

using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kinetix
{
    public class BlendshapeArray : ICloneable, ICollection, IEnumerable, IStructuralComparable, IStructuralEquatable
	{
        public const int COUNT = (int)ARKitBlendshapes.Count;
        private float[] m_array = new float[COUNT];

        public BlendshapeArray() {}
        public BlendshapeArray(IEnumerable<KeyValuePair<ARKitBlendshapes, float>> array)
		{
			foreach (var item in array)
			{
				m_array[(int)item.Key] = item.Value;
			}
		}
        public BlendshapeArray(IEnumerable<float> array)
        {
			var enumerator = array.GetEnumerator();
			for (int i = 0; i < COUNT; i++)
			{
				if (!enumerator.MoveNext())
				{
					Debug.LogWarning($"Given enumerator is smaller than {nameof(ARKitBlendshapes)}.{nameof(ARKitBlendshapes.Count)}");
					break;
				}
				m_array[i] = enumerator.Current;
			}
        }

        public float this[ARKitBlendshapes index] { get => m_array[(int)index]; set => m_array[(int)index] = value; }

		public int Count => (int)ARKitBlendshapes.Count;

		public bool IsSynchronized => m_array.IsSynchronized;

		public object SyncRoot => m_array.SyncRoot;

		public float[] ToArray() => (float[])m_array.Clone();

		public object Clone()
		{
			return new BlendshapeArray(m_array);
		}

		public int CompareTo(object other, IComparer comparer)
		{
			return ((IStructuralComparable)m_array).CompareTo(other, comparer);
		}

		public bool Contains(object value)
		{
			return ((IList)m_array).Contains(value);
		}

		public void CopyTo(Array array, int index)
		{
			m_array.CopyTo(array, index);
		}

		public bool Equals(object other, IEqualityComparer comparer)
		{
			return ((IStructuralEquatable)m_array).Equals(other, comparer);
		}

		public IEnumerator GetEnumerator()
		{
			return m_array.GetEnumerator();
		}

		public int GetHashCode(IEqualityComparer comparer)
		{
			return ((IStructuralEquatable)m_array).GetHashCode(comparer);
		}

		public int IndexOf(object value)
		{
			return ((IList)m_array).IndexOf(value);
		}
	}
}
