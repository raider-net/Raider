using System;
using System.Linq;

namespace Raider.Extensions
{
	public static class IOrderedEnumerableExtensions
	{
		public static IOrderedEnumerable<TSource> ThenBySafe<TSource, TKey>(this IOrderedEnumerable<TSource> enumerable, Func<TSource, TKey> keySelector)
		{
			if (enumerable == null || keySelector == null) return enumerable;

			return enumerable.ThenBy(keySelector);
		}

		public static IOrderedEnumerable<TSource> ThenByDescendingSafe<TSource, TKey>(this IOrderedEnumerable<TSource> enumerable, Func<TSource, TKey> keySelector)
		{
			if (enumerable == null || keySelector == null) return enumerable;

			return enumerable.ThenByDescending(keySelector);
		}

		public static IOrderedEnumerable<TSource> ThenBySafe<TSource>(this IOrderedEnumerable<TSource> enumerable, string propertyName)
		{
			if (enumerable == null || propertyName == null) return enumerable;

			return enumerable.ThenBy(propertyName);
		}

		public static IOrderedEnumerable<TSource> ThenByDescendingSafe<TSource>(this IOrderedEnumerable<TSource> enumerable, string propertyName)
		{
			if (enumerable == null || propertyName == null) return enumerable;

			return enumerable.ThenByDescending(propertyName);
		}

		public static IOrderedEnumerable<TSource> ThenBy<TSource>(this IOrderedEnumerable<TSource> enumerable, string propertyName)
		{
			if (string.IsNullOrWhiteSpace(propertyName)) return enumerable;
			IOrderedQueryable<TSource> queryableSource = (IOrderedQueryable<TSource>)enumerable.AsQueryable();
			return new IOrderedEnumerableNoOrderWrapper<TSource>(queryableSource.ThenBy(propertyName));
		}

		public static IOrderedEnumerable<TSource> ThenByDescending<TSource>(this IOrderedEnumerable<TSource> enumerable, string propertyName)
		{
			if (string.IsNullOrWhiteSpace(propertyName)) return enumerable;
			IOrderedQueryable<TSource> queryableSource = (IOrderedQueryable<TSource>)enumerable.AsQueryable();
			return new IOrderedEnumerableNoOrderWrapper<TSource>(queryableSource.ThenByDescending(propertyName));
		}
	}
}
