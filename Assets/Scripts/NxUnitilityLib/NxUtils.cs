using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Nx
{
	public static class NxUtils
	{
		public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
		{
			foreach (T item in list)
			{
				action(item);
			}
		}

		public static bool StartsWith<T>(this T[] array, T[] other) where T : IEquatable<T>
		{
			if (other.Length > array.Length)
			{
				return false;
			}

			for (int i = 0, iMax = other.Length; i < iMax; ++i)
			{
				if (!array[i].Equals(other[i]))
				{
					return false;
				}
			}
			return true;
		}

		public static T Best<T>(this IEnumerable<T> container, Func<T,T,bool> AIsBetterThanB)
		{
			IEnumerator<T> enumerator = container.GetEnumerator();
			T best;
			if (enumerator.MoveNext())
			{
				best = enumerator.Current;
			}
			else
			{
				return default;
			}
			for (; enumerator.MoveNext();)
			{
				if (AIsBetterThanB(enumerator.Current, best))
				{
					best = enumerator.Current;
				}
			}
			return best;
		}

		public static int BestIndex<T>(this ReadOnlyCollection<T> container, Func<T, T, bool> AIsBetterThanB)
		{
			int bestIndex = 0;
			for (int i = 1, iMax = container.Count; i < iMax; ++i)
			{
				if (AIsBetterThanB(container[i], container[bestIndex]))
				{
					bestIndex = i;
				}
			}
			return bestIndex;
		}

		public static T[] AllBest<T>(this IEnumerable<T> container, Func<T, T, bool> AIsBetterThanB, Func<T, T, bool> AIsSameAsB)
		{
			IEnumerator<T> enumerator = container.GetEnumerator();
			var best = new LinkedList<T>();
			if (enumerator.MoveNext())
			{
				best.AddLast(enumerator.Current);
			}
			else
			{
				return new T[0];
			}
			while (enumerator.MoveNext())
			{
				if (AIsBetterThanB(enumerator.Current, best.First.Value))
				{
					best.Clear();
					best.AddLast(enumerator.Current);
				}
				else if (AIsSameAsB(enumerator.Current, best.First.Value))
				{
					best.AddLast(enumerator.Current);
				}
			}
			return best.ToArray();
		}

		public static void IfIsNotNullThen(this object obj, Action action)
		{
			if (obj != null)
			{
				action();
			}
		}

		public static void IfIsNullThen(this object obj, Action action)
		{
			if (obj == null)
			{
				action();
			}
		}

		public static void IfIsNotNullThen<T>(this T obj, Action<T> action) where T : class
		{
			if (obj != null)
			{
				action(obj);
			}
		}

		public static bool IsEmpty<T>(this ReadOnlyCollection<T> collection)
		{
			return collection.Count <= 0;
		}

		public static bool IsEmpty<T>(this IList<T> list)
		{
			return list.Count <= 0;
		}

		public static bool IsEmpty<T>(this Stack<T> stack)
		{
			return stack.Count == 0;
		}

		public static bool IsNullOrEmpty<T>(this T[] array)
		{
			return array == null || array.Length <= 0;
		}

		public static void Raise(this Action action)
		{
			action.IfIsNotNullThen(action);
		}

		public static void Raise<T>(this Action<T> action, T arg)
		{
			action.IfIsNotNullThen(a => a(arg));
		}

		public static T Last<T>(this IList<T> list)
		{
			int lastIndex = list.LastIndex();
			return lastIndex < 0 ? default : list[lastIndex];
		}

		public static T Last<T>(this ReadOnlyCollection<T> collection)
		{
			int lastIndex = collection.LastIndex();
			return lastIndex < 0 ? default : collection[lastIndex];
		}

		public static int LastIndex<T>(this IList<T> list)
		{
			return list.Count - 1;
		}

		public static int LastIndex<T>(this ReadOnlyCollection<T> collection)
		{
			return collection.Count - 1;
		}

		public static bool IndexIsValid<T>(this ICollection<T> collection, int index)
		{
			return index >= 0 && index < collection.Count;
		}

		public static bool InsertionIndexIsValid<T>(this ICollection<T> collection, int index)
		{
			return index >= 0 && index <= collection.Count;
		}

		public class FalseEqualityComparer<T> : IEqualityComparer<T>
		{
			public bool Equals(T a, T b)
			{
				return false;
			}

			public int GetHashCode(T obj)
			{
				return obj.GetHashCode();
			}
		}

		public static void ResetLocal(this Transform transform, bool position = true, bool scale = true, bool rotation = true)
		{
			if (position)
			{
				transform.localPosition = Vector3.zero;
			}
			if (scale)
			{
				transform.localScale = Vector3.one;
			}
			if (rotation)
			{
				transform.localRotation = Quaternion.identity;
			}
		}

		public static void SetAlpha(this Graphic graphic, float newAlpha)
		{
			Color newColor = graphic.color;
			newColor.a = newAlpha;
			graphic.color = newColor;
		}

		public static void SetAlpha(this TextMesh textMesh, float newAlpha)
		{
			Color newColor = textMesh.color;
			newColor.a = newAlpha;
			textMesh.color = newColor;
		}
	}

	[Serializable]
	public class FloatEvent : UnityEvent<float> {}

	[Serializable]
	public struct MeshMaterialSwapInfo
	{
		[SerializeField]	private	MeshRenderer	_meshRenderer;
		[SerializeField]	private	Material		_swapMaterial;

							public	MeshRenderer	MeshRenderer	{ get { return _meshRenderer; } }
							public	Material		SwapMaterial	{ get { return _swapMaterial; } }
	}
}