using UnityEngine;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Nx
{
	public static class NxUtils
	{
		private static readonly int CARD_TEXT_SIZE = 429;

		public static void ForEach<T>(this T[] array, Action<T> action)
		{
			array.IfIsNotNullThen(a => Array.ForEach(a, action));
		}

		public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
		{
			for (IEnumerator<T> i = list.GetEnumerator(); i.MoveNext();)
			{
				action(i.Current);
			}
		}

		public static bool Exists<T>(this IEnumerable<T> container, Predicate<T> predicate)
		{
			for (IEnumerator<T> enumerator = container.GetEnumerator(); enumerator.MoveNext();)
			{
				if (predicate(enumerator.Current))
				{
					return true;
				}
			}
			return false;
		}

		public static T[] AllSuchThat<T>(this IEnumerable<T> container, Predicate<T> predicate)
		{
			var matchingList = new LinkedList<T>();
			for (IEnumerator<T> enumerator = container.GetEnumerator(); enumerator.MoveNext();)
			{
				if (predicate(enumerator.Current))
				{
					matchingList.AddLast(enumerator.Current);
				}
			}
			return matchingList.ToArray();
		}

		public static int[] AllIndexesSuchThat<T>(this ReadOnlyCollection<T> list, Predicate<T> predicate)
		{
			var matchingList = new LinkedList<int>();
			for (int i = 0, iMax = list.Count; i <iMax; ++i)
			{
				if (predicate(list[i]))
				{
					matchingList.AddLast(i);
				}
			}
			return matchingList.ToArray();
		}

		public static int[] AllIndexesSuchThat<T>(this ReadOnlyCollection<T> list, Func<T, int, bool> itemAndIndexIsValid)
		{
			var matchingList = new LinkedList<int>();
			for (int i = 0, iMax = list.Count; i <iMax; ++i)
			{
				if (itemAndIndexIsValid(list[i], i))
				{
					matchingList.AddLast(i);
				}
			}
			return matchingList.ToArray();
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
				return default(T);
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

		public static int BestIndex<T>(this ReadOnlyCollection<T> container, Func<T,T,bool> AIsBetterThanB)
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

		public static T[] AllBest<T>(this IEnumerable<T> container, Func<T,T,bool> AIsBetterThanB, Func<T,T,bool> AIsSameAsB)
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

		public static int[] AllBestIndexes<T>(this ReadOnlyCollection<T> list, Func<T,T,bool> AIsBetterThanB, Func<T,T,bool> AIsSameAsB)
		{
			if (list.Count <= 0)
			{
				return new int[0];
			}
			var bestIndexes = new LinkedList<int>(new int[] {0});
			for (int i = 1, iMax = list.Count; i < iMax; ++i)
			{
				if (AIsBetterThanB(list[i], list[bestIndexes.First.Value]))
				{
					bestIndexes.Clear();
					bestIndexes.AddLast(i);
				}
				else if (AIsSameAsB(list[i], list[bestIndexes.First.Value]))
				{
					bestIndexes.AddLast(i);
				}
			}
			return bestIndexes.ToArray();
		}

		public static void IfIsNotNullThen(this System.Object obj, Action action)
		{
			if (obj != null)
			{
				action();
			}
		}

		public static void IfIsNullThen(this System.Object obj, Action action)
		{
			if (obj == null)
			{
				action();
			}
		}

		public static void IfIsNotNullThen<T>(this T obj, Action<T> action)
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

		public static bool IsEmpty<T>(this List<T> list)
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

		public static T Last<T>(this List<T> list)
		{
			int lastIndex = list.LastIndex();
			return lastIndex < 0 ? default(T) : list[lastIndex];
		}

		public static T Last<T>(this ReadOnlyCollection<T> collection)
		{
			int lastIndex = collection.LastIndex();
			return lastIndex < 0 ? default(T) : collection[lastIndex];
		}

		public static int LastIndex<T>(this List<T> list)
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

		public static string ToRichText(this string str, int size = 40, string color = "black")
		{
			return "<size=" + size + "><color=" + color + ">" + str + "</color></size>";
		}

		public static string FormattedForCardText(this Card card)
		{
			return card.ToString().ToRichText(size: CARD_TEXT_SIZE, color: card.Suit == Card.CardSuit.SPADES
											 || card.Suit == Card.CardSuit.CLUBS ? "black" : "red");
		}

		public static void Log(object message)
		{
#if NX_DEBUG
			Debug.Log(message);
#endif
		}

		public static void LogWarning(object message)
		{
#if NX_DEBUG
			Debug.LogWarning(message);
#endif
		}

		public static void LogError(object message)
		{
#if NX_DEBUG
			Debug.LogError(message);
#endif
		}
	}
}