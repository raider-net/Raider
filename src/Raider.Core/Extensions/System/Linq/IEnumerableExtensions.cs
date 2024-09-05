using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Raider.Extensions
{
	public static class IEnumerableExtensionsssss
	{
		public static IEnumerable<TSource> OrderSafe<TSource, TKey>(this IEnumerable<TSource> query, Func<TSource, TKey> keySelector, ListSortDirection direction = ListSortDirection.Ascending)
		{
			if (query == null || keySelector == null) return query;

			return direction == ListSortDirection.Ascending
				? query.OrderBy(keySelector)
				: query.OrderByDescending(keySelector);
		}

		public static IOrderedEnumerable<TSource> OrderBySafe<TSource, TKey>(this IEnumerable<TSource> query, Func<TSource, TKey> keySelector)
		{
			if (query == null || keySelector == null) return (IOrderedEnumerable<TSource>)query;

			return query.OrderBy(keySelector);
		}

		public static IOrderedEnumerable<TSource> OrderByDescendingSafe<TSource, TKey>(this IEnumerable<TSource> query, Func<TSource, TKey> keySelector)
		{
			if (query == null || keySelector == null) return (IOrderedEnumerable<TSource>)query;

			return query.OrderByDescending(keySelector);
		}

		public static IOrderedEnumerable<TSource> OrderBySafe<TSource, TKey>(this IEnumerable<TSource> query, string propertyName)
		{
			if (query == null) return (IOrderedEnumerable<TSource>)query;

			return query.OrderBy(propertyName);
		}

		public static IOrderedEnumerable<TSource> OrderByDescendingSafe<TSource, TKey>(this IEnumerable<TSource> query, string propertyName)
		{
			if (query == null) return (IOrderedEnumerable<TSource>)query;

			return query.OrderByDescending(propertyName);
		}

		public static IEnumerable<TSource> SkipSafe<TSource>(this IEnumerable<TSource> query, int count)
		{
			if (query == null || count < 1) return query;

			return query.Skip(count);
		}

		public static IEnumerable<TSource> TakeSafe<TSource>(this IEnumerable<TSource> query, int count)
		{
			if (query == null || count < 1) return query;

			return query.Take(count);
		}

		public static IEnumerable<TSource> WhereSafe<TSource>(this IEnumerable<TSource> query, Func<TSource, bool> whereExpression)
		{
			if (query == null || whereExpression == null) return query;

			return query.Where(whereExpression);
		}

		//public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
		//	Func<TSource, TKey> keySelector)
		//{
		//	return source.DistinctBy(keySelector, null);
		//}

		//public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
		//	Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
		//{
		//	if (source == null) throw new ArgumentNullException(nameof(source));
		//	if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

		//	return _(); IEnumerable<TSource> _()
		//	{
		//		var knownKeys = new HashSet<TKey>(comparer);
		//		foreach (var element in source)
		//		{
		//			if (knownKeys.Add(keySelector(element)))
		//				yield return element;
		//		}
		//	}
		//}
	}
}
