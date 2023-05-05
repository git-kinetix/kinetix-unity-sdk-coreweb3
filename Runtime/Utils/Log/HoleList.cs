using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kinetix.Internal.Utils
{
	/// <summary>
	/// List with static indexes.<br/>
	/// Use <see cref="LatestIndex"/> after a <see cref="Add"/> to access the index of the item you've added
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class HoleList<T> : IList<T>, IEnumerable<(T value, int index)>
	{
		/// <summary>
		/// Index of last <see cref="Add"/>
		/// </summary>
		public int LatestIndex { get; private set; }

		SortedSet<int> _nullIndexes = new SortedSet<int>();
		List<bool> _null = new List<bool>();
		List<T> _list = new List<T>();

		public T this[int id]
		{
			get => _list[id];
			set => _list[id] = value;
		}

		public int Count => _list.Count - _nullIndexes.Count;

		public bool IsReadOnly => false;

		public void Add(T item)
		{
			if (_nullIndexes.Count > 0)
			{
				int p = LatestIndex = _nullIndexes.Min;
				_null[p] = false;
				_list[p] = item;
				_nullIndexes.Remove(p);
				return;
			}

			LatestIndex = _list.Count;
			_null.Add(false);
			_list.Add(item);
		}

		public void Clear()
		{
			_nullIndexes.Clear();
			_list.Clear();
		}

		public void RemoveAt(int index)
		{
			_null[index] = true;
			_list[index] = default;
			_nullIndexes.Add(index);

			int nullI = _nullIndexes.Count - 1;
			while (index == _list.Count - 1 && index >= 0 && _null[index])
			{
				_nullIndexes.Remove(index);
				_list.RemoveAt(index);
				_null.RemoveAt(index);
				index--;
			}
		}

		public int IndexOf(T item) => _list.IndexOf(item);

		public bool Contains(T item) => _list.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) => throw new NotSupportedException();

		public void Insert(int index, T item) => throw new NotSupportedException();

		public bool Remove(T item) => throw new NotSupportedException();

		public IEnumerable<(T value, int index)> CastEnumerable() => this;

		IEnumerator IEnumerable.GetEnumerator() => _list.Where(l => l != null).GetEnumerator();
		IEnumerator<T> IEnumerable<T>.GetEnumerator() => _list.Where(l => l != null).GetEnumerator();
		IEnumerator<(T value, int index)> IEnumerable<(T value, int index)>.GetEnumerator()
		{
			int count = _list.Count;
			for (int i = 0; i < count; i++)
			{
				if (!_null[i])
					yield return (_list[i], i);
			}
		}

		public override string ToString()
		{
			string toReturn = "";
			int count = _list.Count;
			for (int i = 0; i < count; i++)
			{
				if (!_null[i])
				{
					toReturn += $"{i}:{_list[i]}, ";
				}
			}

			return toReturn.Substring(0, Math.Max(0, toReturn.Length - 2));
		}
	}
}
