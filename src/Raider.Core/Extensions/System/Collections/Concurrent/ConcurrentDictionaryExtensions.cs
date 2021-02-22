using System;
using System.Collections.Concurrent;

namespace Raider.Extensions
{
	public static class ConcurrentDictionaryExtensions
	{
		public static TValue AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary,
			TKey key,
			Func<TKey, TValue> addValueFactory)
		{
			if (dictionary == null)
				throw new ArgumentNullException(nameof(dictionary));
			if (addValueFactory == null)
				throw new ArgumentNullException(nameof(addValueFactory));

			return dictionary.AddOrUpdate(
				key,
				addValueFactory,
				(existKey, oldValue) => addValueFactory(key));
		}

		public static TValue AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary,
			TKey key,
			TValue addValue)
		{
			if (dictionary == null)
				throw new ArgumentNullException(nameof(dictionary));

			return dictionary.AddOrUpdate(
				key,
				addValue,
				(existKey, oldValue) => addValue);
		}
	}
}
