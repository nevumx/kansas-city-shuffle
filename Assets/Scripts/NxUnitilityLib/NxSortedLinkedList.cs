using System;
using System.Collections;
using System.Collections.Generic;

namespace Nx
{
	[Serializable]
	public class NxSortedLinkedList<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
	{
		private LinkedList<T> _list = new LinkedList<T>();

		private Func<T, int> _sortingFunction;

		private NxSortedLinkedList() {}

		public NxSortedLinkedList(Func<T, int> sortBy)
		{
			_sortingFunction = sortBy;
		}

		int ICollection.Count
		{
			get
			{
				return _list.Count;
			}
		}

		int ICollection<T>.Count
		{
			get
			{
				return _list.Count;
			}
		}

		public LinkedListNode<T> First
		{
			get
			{
				return _list.First;
			}
		}

		bool ICollection<T>.IsReadOnly
		{
			get
			{
				return ((ICollection<T>) _list).IsReadOnly;
			}
		}

		bool ICollection.IsSynchronized
		{
			get
			{
				return ((ICollection) _list).IsSynchronized;
			}
		}

		public LinkedListNode<T> Last
		{
			get
			{
				return _list.Last;
			}
		}

		object ICollection.SyncRoot
		{
			get
			{
				return ((ICollection) _list).SyncRoot;
			}
		}

		public void Clear()
		{
			_list.Clear();
		}

		public bool Contains(T value)
		{
			return _list.Contains(value);
		}

		void ICollection.CopyTo(System.Array array, int index)
		{
			((ICollection)_list).CopyTo(array, index);
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			((ICollection<T>)_list).CopyTo(array, arrayIndex);
		}

		public LinkedListNode<T> Find (T value)
		{
			return _list.Find(value);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator) _list.GetEnumerator();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			return (IEnumerator<T>) _list.GetEnumerator();
		}

		public LinkedListNode<T> AddSorted(T value)
		{
			for (LinkedListNode<T> node = _list.First; node != null; node = node.Next)
			{
				if (_sortingFunction(value) - _sortingFunction(node.Value) < 0)
				{
					return _list.AddBefore(node, value);
				}
			}
			return _list.AddLast(value);
		}

		public void AddSorted(LinkedListNode<T> newNode)
		{
			for (LinkedListNode<T> node = _list.First; node != null; node = node.Next)
			{
				if (_sortingFunction(newNode.Value) - _sortingFunction(node.Value) < 0)
				{
					_list.AddBefore(node, newNode);
					return;
				}
			}
			_list.AddLast(newNode);
		}

		public void Add(T value)
		{
			AddSorted(value);
		}

		public void Remove(LinkedListNode<T> node)
		{
			_list.Remove(node);
		}

		public bool Remove(T value)
		{
			return _list.Remove(value);
		}

		public void RemoveFirst()
		{
			_list.RemoveFirst();
		}

		public void RemoveLast()
		{
			_list.RemoveLast();
		}
	}
}