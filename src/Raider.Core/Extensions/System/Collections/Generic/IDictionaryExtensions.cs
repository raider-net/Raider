using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raider.Extensions
{
	public static class IDictionaryExtensions
	{
		/// <summary>
		/// Get value from dictionary by key
		/// </summary>
		public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			return Get(dictionary, key, default(TValue));
		}

		/// <summary>
		/// Get value from dictionary by key
		/// </summary>
		public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
		{
			if (dictionary == null) return default(TValue);
			if (dictionary.TryGetValue(key, out TValue value))
			{
				return value;
			}
			else
			{
				return defaultValue;
			}
		}

		public static TValue AddOrGet<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
		{
			if (dict == null)
				throw new ArgumentNullException(nameof(dict));

			if (!dict.TryGetValue(key, out TValue val))
			{
				val = value;
				dict.Add(key, val);
			}

			return val;
		}

		public static TValue AddOrGet<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> valueFactory)
		{
			if (dict == null)
				throw new ArgumentNullException(nameof(dict));
			if (valueFactory == null)
				throw new ArgumentNullException(nameof(valueFactory));

			if (!dict.TryGetValue(key, out TValue val))
			{
				val = valueFactory(key);
				dict.Add(key, val);
			}

			return val;
		}

		public static bool AddUniqueKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
		{
			if (dict == null)
				throw new ArgumentNullException(nameof(dict));

			if (!dict.ContainsKey(key))
			{
				dict.Add(key, value);
				return true;
			}
			else
			{
				return false;
			}
		}

		public static IDictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue item)
		{
			if (dict == null)
				throw new ArgumentNullException(nameof(dict));

			if (item != null)
			{
				dict[key] = item;
			}

			return dict;
		}

		public static IDictionary<TKey, TValue> AddItem<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value, bool checkForNulls = false)
		{
			if (dict == null)
				throw new ArgumentNullException(nameof(dict));

			if (key == null)
				throw new ArgumentNullException(nameof(key));

			if (checkForNulls && value == null)
			{
				return dict;
			}

			dict[key] = value;

			return dict;
		}

		public static List<TKey> AddRangeUniqueKeys<TKey, TValue>(this IDictionary<TKey, TValue> dict, IDictionary<TKey, TValue> dictionary)
		{
			if (dict == null || dictionary == null)
			{
				return null;
			}

			List<TKey> result = new List<TKey>();
			foreach (KeyValuePair<TKey, TValue> item in dictionary)
			{
				if (!dict.ContainsKey(item.Key))
				{
					dict.Add(item.Key, item.Value);
					result.Add(item.Key);
				}
			}
			return result;
		}

		public static List<TKey> AddOrReplaceRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IDictionary<TKey, TValue> dictionary)
		{
			if (dict == null || dictionary == null)
				return null;

			List<TKey> result = new List<TKey>();
			foreach (KeyValuePair<TKey, TValue> item in dictionary)
			{
				if (dict.ContainsKey(item.Key))
				{
					dict[item.Key] = item.Value;
				}
				else
				{
					dict.Add(item.Key, item.Value);
					result.Add(item.Key);
				}
			}
			return result;
		}

		public static bool RemoveIfKeyExists<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
		{
			if (dict == null)
				return false;

			if (dict.ContainsKey(key))
			{
				dict.Remove(key);
				return true;
			}
			else
			{
				return false;
			}
		}

		public static string ConcatKeysAndValues<TKey, TValue>(this IDictionary<TKey, TValue> dict, string keyValueDelimiter, string keyValuePairDelimiter)
		{
			if (dict == null)
			{
				return null;
			}

			StringBuilder sb = new StringBuilder();
			int idx = 0;
			foreach (KeyValuePair<TKey, TValue> kvp in dict)
			{
				string endDelimiter = string.Empty;
				if (idx < (dict.Count - 1)) //last
				{
					endDelimiter = keyValuePairDelimiter;
				}
				sb.AppendFormat("{0}{1}{2}{3}", kvp.Key, keyValueDelimiter, kvp.Value, endDelimiter);
			}

			return sb.ToString();
		}

		public static bool TryGetValue<K, V>(this IDictionary<K, object> dictionary, K key, out V value)
		{
			if (dictionary == null)
			{
				throw new ArgumentNullException(nameof(dictionary));
			}
			if (dictionary.TryGetValue(key, out object tmp))
			{
				if (!(tmp is V))
				{
					value = default(V);
					return false;
				}
				value = (V)tmp;
				return true;
			}
			else
			{
				value = default(V);
				return false;
			}
		}

		public static IDictionary<string, string> ToJson(this IDictionary<string, object> jsonObj)
		{
			return jsonObj?.ToDictionary(x => x.Key, y => y.Value as string ?? System.Text.Json.JsonSerializer.Serialize(y.Value))
				?? new Dictionary<string, string>();
		}

		/// <summary>
		/// Get key and value by TValue type
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool TryGetValue<TKey, TValue>(this IDictionary<TKey, object> dictionary, out TKey key, out TValue value)
		{
			if (dictionary == null)
				throw new ArgumentNullException(nameof(dictionary));

			key = default(TKey);
			value = default(TValue);

			foreach (var item in dictionary)
			{
				if (item.Value is TValue)
				{
					key = item.Key;
					value = (TValue)item.Value;
					return true;
				}
			}

			return false;
		}

		public static TValue Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue, TValue> merge)
		{
			if (dictionary == null)
				throw new ArgumentNullException(nameof(dictionary));

			if (merge == null)
				throw new ArgumentNullException(nameof(merge));

			TValue result = default(TValue);

			if (dictionary.TryGetValue(key, out TValue oldValue))
			{
				result = merge(oldValue);
				dictionary[key] = result;
			}
			else
			{
				result = merge(default(TValue));
				dictionary[key] = result;
			}

			return result;
		}
	}
}
