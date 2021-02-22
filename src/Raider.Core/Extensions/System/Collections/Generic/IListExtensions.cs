using System;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Extensions
{
	public static class IListExtensions
	{
		public static T Get<T>(this IList<T> list, int index) where T : class
		{
			if (list == null || index < 0 || list.Count < index)
			{
				return default(T);
			}
			try
			{
				return list[index];
			}
			catch (System.Exception)
			{
				return default(T);
			}
		}

		public static T Get<T>(this IList<T> list, int index, T defaultValue)
		{
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}
			if (index < 0 || list.Count < index)
			{
				return defaultValue;
			}
			try
			{
				return list[index];
			}
			catch (System.Exception)
			{
				return defaultValue;
			}
		}

		public static IList<T> AddUniqueItem<T>(this IList<T> list, T item)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));

			if (item != null && !list.Contains(item))
				list.Add(item);

			return list;
		}

		public static IList<T> AddUniqueItem<T>(this IList<T> list, T item, IEqualityComparer<T> comparer)
		{
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}

			if (!list.Contains(item, comparer) && item != null)
			{
				list.Add(item);
			}

			return list;
		}

		public static IList<T> InsertItem<T>(this IList<T> list, int index, T item)
		{
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}

			if (index >= list.Count)
			{
				list.Add(item);
			}
			else if (index < 0)
			{
				list.Insert(0, item);
			}
			else
			{
				list.Insert(index, item);
			}

			return list;
		}

		public static List<T> CopyContainsRange<T>(this IList<T> list, IEnumerable<T> collection)
		{
			if (list == null || collection == null)
			{
				return null;
			}

			//List<T> result = new List<T>();
			//foreach (T item in collection)
			//{
			//    if (list.Contains(item))
			//    {
			//        result.Add(item);
			//    }
			//}
			List<T> result = new List<T>(collection.Where(x => list.Contains(x)));
			return result;
		}

		public static List<T> CopyContainsRange<T>(this IList<T> list, IEnumerable<T> collection, IEqualityComparer<T> comparer)
		{
			if (list == null || collection == null)
			{
				return null;
			}

			//List<T> result = new List<T>();
			//foreach (T item in collection)
			//{
			//    if (list.Contains(item))
			//    {
			//        result.Add(item);
			//    }
			//}
			List<T> result = new List<T>(collection.Where(x => list.Contains(x, comparer)));
			return result;
		}

		public static bool ContainsAll<T>(this IList<T> list, IEnumerable<T> collection)
		{
			if (list == null || collection == null)
			{
				return false;
			}

			int containsCount = 0;
			foreach (T item in collection)
			{
				if (!list.Contains(item))
				{
					return false;
				}
				else
				{
					containsCount++;
				}
			}
			return 0 < containsCount;
		}

		public static bool ContainsAll<T>(this IList<T> list, IEnumerable<T> collection, IEqualityComparer<T> comparer)
		{
			if (list == null || collection == null)
			{
				return false;
			}

			int containsCount = 0;
			foreach (T item in collection)
			{
				if (!list.Contains(item, comparer))
				{
					return false;
				}
				else
				{
					containsCount++;
				}
			}
			return 0 < containsCount;
		}

		public static bool ContainsAtLeastOne<T>(this IList<T> list, IEnumerable<T> collection)
		{
			if (list == null || collection == null)
			{
				return false;
			}

			foreach (T item in collection)
			{
				if (list.Contains(item))
				{
					return true;
				}
			}
			return false;
		}

		public static bool ContainsAtLeastOne<T>(this IList<T> list, IEnumerable<T> collection, IEqualityComparer<T> comparer)
		{
			if (list == null || collection == null)
			{
				return false;
			}

			foreach (T item in collection)
			{
				if (list.Contains(item, comparer))
				{
					return true;
				}
			}
			return false;
		}

		public static bool ContainsAtLeastOne<T>(this IList<T> list, params T[] collection)
		{
			if (list == null || collection == null)
			{
				return false;
			}

			foreach (T item in collection)
			{
				if (list.Contains(item))
				{
					return true;
				}
			}
			return false;
		}

		public static bool ContainsAtLeastOne<T>(this IList<T> list, IEqualityComparer<T> comparer, params T[] collection)
		{
			if (list == null || collection == null)
			{
				return false;
			}

			foreach (T item in collection)
			{
				if (list.Contains(item, comparer))
				{
					return true;
				}
			}
			return false;
		}

		public static bool AddUniqueRange<T>(this IList<T> list, IEnumerable<T> collection)
		{
			if (list == null || collection == null)
			{
				return false;
			}

			int addedCount = 0;
			foreach (T item in collection)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
					addedCount++;
				}
			}
			return 0 < addedCount;
		}

		public static List<T> AddUniqueRange<T>(this List<T> list, IEnumerable<T> collection)
		{
			if (list == null || collection == null)
			{
				return list;
			}

			int addedCount = 0;
			foreach (T item in collection)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
					addedCount++;
				}
			}
			return list;
		}

		public static bool AddUniqueRange<T>(this IList<T> list, IEnumerable<T> collection, IEqualityComparer<T> comparer)
		{
			if (list == null || collection == null)
			{
				return false;
			}

			int addedCount = 0;
			foreach (T item in collection)
			{
				if (!list.Contains(item, comparer))
				{
					list.Add(item);
					addedCount++;
				}
			}
			return 0 < addedCount;
		}

		public static List<T> AddUniqueRange<T>(this List<T> list, IEnumerable<T> collection, IEqualityComparer<T> comparer)
		{
			if (list == null || collection == null)
			{
				return list;
			}

			int addedCount = 0;
			foreach (T item in collection)
			{
				if (!list.Contains(item, comparer))
				{
					list.Add(item);
					addedCount++;
				}
			}
			return list;
		}

		public static int RemoveContainsRange<T>(this IList<T> list, IEnumerable<T> collection)
		{
			if (list == null || collection == null)
			{
				return 0;
			}

			int removedCount = 0;
			List<T> clonedContainsCollection = CopyContainsRange(list, collection);
			foreach (T item in clonedContainsCollection)
			{
				list.Remove(item);
				removedCount++;
			}
			return removedCount;
		}

		public static int RemoveContainsRange<T>(this IList<T> list, IEnumerable<T> collection, IEqualityComparer<T> comparer)
		{
			if (list == null || collection == null)
			{
				return 0;
			}

			int removedCount = 0;
			List<T> clonedContainsCollection = CopyContainsRange(list, collection, comparer);
			foreach (T item in clonedContainsCollection)
			{
				list.Remove(item);
				removedCount++;
			}
			return removedCount;
		}

		public static IList<T> GetNullIfEmpty<T>(this IList<T> list)
		{
			if (list == null || list.Count == 0)
			{
				return null;
			}
			return list;
		}
	}
}

