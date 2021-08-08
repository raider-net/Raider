using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace Raider.Extensions
{
	public static class IEnumerableExtensions
	{
		public static string ConvertToString(this IEnumerable list, string delimiter = ", ")
		{
			if (list == null) return null;
			StringBuilder stringBuilder = new StringBuilder();
			bool needDelimiter = false;
			foreach (object current in list)
			{
				if (needDelimiter)
					stringBuilder.Append(delimiter);

				if (current != null)
				{
					stringBuilder.Append(current.ToString());
				}
				else
				{
					stringBuilder.Append("null");
				}
				needDelimiter = true;

			}
			return stringBuilder.ToString();
		}

		public static bool IsEqualsTo<T>(this IEnumerable<T> list, IEnumerable<T> enumerable, IEqualityComparer<T>? comparer = null)
		{
			if (list == null && enumerable == null)
			{
				return true;
			}
			else if (list == null || enumerable == null)
			{
				return false;
			}
			else if (list.Count() != enumerable.Count())
			{
				return false;
			}
			else
			{
				return comparer == null
					? list.SequenceEqual(enumerable)
					: list.SequenceEqual(enumerable, comparer);
			}
		}

		public static T Get<T>(this IEnumerable<T> list, int index) where T : class
		{
			if (list == null || index < 0 || list.Count() < index)
			{
				return default(T);
			}
			try
			{
				return list.ElementAtOrDefault(index);
			}
			catch (System.Exception)
			{
				return default(T);
			}
		}

		public static T Get<T>(this IEnumerable<T> list, int index, T defaultValue)
		{
			if (list == null || index < 0 || list.Count() < index)
			{
				return defaultValue;
			}
			try
			{
				return list.ElementAtOrDefault(index);
			}
			catch (System.Exception)
			{
				return defaultValue;
			}
		}

		public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
		{
			var col = new ObservableCollection<T>();
			foreach (var cur in enumerable)
			{
				col.Add(cur);
			}
			return col;
		}

		public static string ConvertToString<T>(this IEnumerable<T> source, Func<T, string> selector, string delimiter = ",")
		{
			StringBuilder b = new StringBuilder();
			bool needDelimiter = false;

			foreach (var item in source)
			{
				if (needDelimiter)
					b.Append(delimiter);

				b.Append(selector(item));
				needDelimiter = true;
			}

			return b.ToString();
		}

		public static IOrderedEnumerable<TSource> OrderBySafe<TSource, TKey>(this IEnumerable<TSource> enumerable, Func<TSource, TKey> keySelector)
		{
			if (enumerable == null || keySelector == null) return new IOrderedEnumerableNoOrderWrapper<TSource>(enumerable);

			return enumerable.OrderBy(keySelector);
		}

		public static IOrderedEnumerable<TSource> OrderByDescendingSafe<TSource, TKey>(this IEnumerable<TSource> enumerable, Func<TSource, TKey> keySelector)
		{
			if (enumerable == null || keySelector == null) return new IOrderedEnumerableNoOrderWrapper<TSource>(enumerable);

			return enumerable.OrderByDescending(keySelector);
		}

		public static IOrderedEnumerable<TSource> OrderBySafe<TSource>(this IEnumerable<TSource> enumerable, string propertyName)
		{
			if (enumerable == null || propertyName == null) return new IOrderedEnumerableNoOrderWrapper<TSource>(enumerable);

			return enumerable.OrderBy(propertyName);
		}

		public static IOrderedEnumerable<TSource> OrderByDescendingSafe<TSource>(this IEnumerable<TSource> enumerable, string propertyName)
		{
			if (enumerable == null || propertyName == null) return new IOrderedEnumerableNoOrderWrapper<TSource>(enumerable);

			return enumerable.OrderByDescending(propertyName);
		}

		public static IOrderedEnumerable<TSource> OrderBy<TSource>(this IEnumerable<TSource> enumerable, string propertyName)
		{
			if (string.IsNullOrWhiteSpace(propertyName)) return new IOrderedEnumerableNoOrderWrapper<TSource>(enumerable);
			IQueryable<TSource> queryableSource = enumerable.AsQueryable();
			return new IOrderedEnumerableNoOrderWrapper<TSource>(queryableSource.OrderBy(propertyName));
		}

		public static IOrderedEnumerable<TSource> OrderByDescending<TSource>(this IEnumerable<TSource> enumerable, string propertyName)
		{
			if (string.IsNullOrWhiteSpace(propertyName)) return new IOrderedEnumerableNoOrderWrapper<TSource>(enumerable);
			IQueryable<TSource> queryableSource = enumerable.AsQueryable();
			return new IOrderedEnumerableNoOrderWrapper<TSource>(queryableSource.OrderByDescending(propertyName));
		}

		public static IEnumerable<TSource> SkipSafe<TSource>(this IEnumerable<TSource> enumerable, int count)
		{
			if (enumerable == null || count < 1) return enumerable;

			return enumerable.Skip(count);
		}

		public static IEnumerable<TSource> TakeSafe<TSource>(this IEnumerable<TSource> enumerable, int count)
		{
			if (enumerable == null || count < 1) return enumerable;

			return enumerable.Take(count);
		}

		/// <summary>
		/// Prevedie IEnumerable<T> na DataTable
		/// </summary>
		/// <param name="tableName">Nazov dataTable</param>
		/// <param name="columnDisplayText">Dictionary<propertyNme, displayName></param>
		/// <returns></returns>
		public static DataTable ToDataTable<T>(this IEnumerable<T> data, string tableName = null, Dictionary<string, string> columnDisplayTexts = null)
		{
			if (data == null || data.Count() == 0)
			{
				return null;
			}
			Type tType = data.GetType().GetGenericArguments()[0];
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(tType);
			if (properties == null) return null;
			string tabName = !string.IsNullOrWhiteSpace(tableName) ? tableName : tType.ToFriendlyName();
			DataTable table = new DataTable(tabName);

			if (columnDisplayTexts == null || columnDisplayTexts.Count == 0)
			{
				foreach (PropertyDescriptor prop in properties)
				{
					table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
				}

				foreach (T item in data)
				{
					DataRow row = table.NewRow();
					foreach (PropertyDescriptor prop in properties)
					{
						row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
					}
					table.Rows.Add(row);
				}
			}
			else
			{
				foreach (KeyValuePair<string, string> visibleColumn in columnDisplayTexts)
				{
					PropertyDescriptor prop = properties.Find(visibleColumn.Key, false);
					if (prop != null)
					{
						if (string.IsNullOrWhiteSpace(visibleColumn.Value))
						{
							table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
						}
						else
						{
							table.Columns.Add(visibleColumn.Value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
						}
					}
				}

				foreach (T item in data)
				{
					DataRow row = table.NewRow();
					foreach (KeyValuePair<string, string> visibleColumn in columnDisplayTexts)
					{
						PropertyDescriptor prop = properties.Find(visibleColumn.Key, false);
						if (prop != null)
						{
							if (string.IsNullOrWhiteSpace(visibleColumn.Value))
							{
								row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
							}
							else
							{
								row[visibleColumn.Value] = prop.GetValue(item) ?? DBNull.Value;
							}
						}
					}
					table.Rows.Add(row);
				}
			}
			return table;
		}

		public static bool HasDuplicates<T>(this IEnumerable<T> source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			var checkBuffer = new HashSet<T>();
			foreach (var t in source)
			{
				if (checkBuffer.Add(t))
					continue;

				return true;
			}

			return false;
		}

		public static bool HasDuplicates<T>(this IEnumerable<T> source, out T? firstDuplicate)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			var checkBuffer = new HashSet<T>();
			foreach (var t in source)
			{
				if (checkBuffer.Add(t))
					continue;

				firstDuplicate = t;
				return true;
			}

			firstDuplicate = default;
			return false;
		}

		public static List<T> GetDuplicates<T>(this IEnumerable<T> source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			var result = new List<T>();
			var checkBuffer = new HashSet<T>();
			foreach (var t in source)
			{
				if (checkBuffer.Add(t))
					continue;

				result.Add(t);
			}

			return result;
		}

		public static List<T> GetDuplicates<T, TCompare>(this IEnumerable<T> source, Func<T?, TCompare?> equalityTransformation)
		{
			if (equalityTransformation == null)
				return GetDuplicates(source);

			if (source == null)
				throw new ArgumentNullException(nameof(source));

			var result = new List<T>();
			var checkBuffer = new HashSet<TCompare?>();
			foreach (var t in source)
			{
				if (checkBuffer.Add(equalityTransformation(t)))
					continue;

				result.Add(t);
			}

			return result;
		}
	}

	public class IOrderedEnumerableNoOrderWrapper<T> : IOrderedEnumerable<T>
	{
		private IEnumerable<T> source;
		public IOrderedEnumerableNoOrderWrapper(IEnumerable<T> source)
		{
			this.source = source;
		}

		public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
		{
			return new IOrderedEnumerableNoOrderWrapper<T>(source);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return source.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return source.GetEnumerator();
		}
	}
}
