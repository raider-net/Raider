using System;
using System.Collections.Generic;

namespace Raider.Collections
{
	public interface IListBuilder<TBuilder, T>
		where TBuilder : IListBuilder<TBuilder, T>
	{
		/// <summary>
		/// Sets the list to build.
		/// </summary>
		TBuilder Object(List<T> list);

		/// <summary>
		/// Returns entire list object.
		/// </summary>
		List<T> ToObject();

		/// <summary>
		///  Adds an object to the end of the list.
		/// </summary>
		/// <param name="item">The object to be added to the end of the list. The value can be null for reference types.</param>
		TBuilder Add(T item);

		/// <summary>
		/// Adds the elements of the specified collection to the end of the list.
		/// </summary>
		/// <param name="collection">The collection whose elements should be added to the end of the list.
		/// The collection itself cannot be null, but it can contain elements that are null,
		/// if type T is a reference type.</param>
		TBuilder AddRange(IEnumerable<T> collection);

		/// <summary>
		/// Removes all elements from the list.
		/// </summary>
		TBuilder Clear();

		/// <summary>
		/// Inserts an element into the list at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which item should be inserted.</param>
		/// <param name="item">The object to insert. The value can be null for reference types.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is greater than list.Count.</exception>
		TBuilder Insert(int index, T item);

		/// <summary>
		/// Inserts the elements of a collection into the list at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the new elements should be inserted.</param>
		/// <param name="collection">The collection whose elements should be inserted into the list.
		/// The collection itself cannot be null, but it can contain elements that are null,
		/// if type T is a reference type.</param>
		/// <exception cref="System.ArgumentNullException">collection is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is greater than list.Count.</exception>
		TBuilder InsertRange(int index, IEnumerable<T> collection);

		/// <summary>
		/// Removes the first occurrence of a specific object from the list.
		/// </summary>
		/// <param name="item">The object to remove from the list. The value can be null for reference types.</param>
		/// <param name="removed">true if item is successfully removed; otherwise, false. <paramref name="removed"/> is
		/// false if item was not found in the list.</param>
		TBuilder Remove(T item, out bool removed);

		/// <summary>
		/// Removes all the elements that match the conditions defined by the specified predicate.
		/// </summary>
		/// <param name="match">The System.Predicate`1 delegate that defines the conditions of the elements to remove.</param>
		/// <param name="removedCount">The number of elements removed from the list.</param>
		/// <exception cref="System.ArgumentNullException">match is null.</exception>
		TBuilder RemoveAll(Predicate<T> match, out int removedCount);

		/// <summary>
		/// Removes the element at the specified index of the list.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is equal to or greater than list.Count.</exception>
		TBuilder RemoveAt(int index);

		/// <summary>
		/// Removes a range of elements from the list.
		/// </summary>
		/// <param name="index">The zero-based starting index of the range of elements to remove.</param>
		/// <param name="count">The number of elements to remove.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
		/// <exception cref="System.ArgumentException">index and count do not denote a valid range of elements in the list.</exception>
		TBuilder RemoveRange(int index, int count);

		/// <summary>
		/// Reverses the order of the elements in the specified range.
		/// </summary>
		/// <param name="index">The zero-based starting index of the range to reverse.</param>
		/// <param name="count">The number of elements in the range to reverse.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
		/// <exception cref="System.ArgumentException">index and count do not denote a valid range of elements in the list.</exception>
		TBuilder Reverse(int index, int count);

		/// <summary>
		/// Reverses the order of the elements in the entire list.
		/// </summary>
		TBuilder Reverse();

		/// <summary>
		/// Sets the element at the specified index
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <param name="item">The object to set. The value can be null for reference types.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is equal to or greater than list.Count.</exception>
		TBuilder Set(int index, T item);

		/// <summary>
		/// Sorts the elements in the entire list using the specified System.Comparison`1.
		/// </summary>
		/// <param name="comparison">The System.Comparison`1 to use when comparing elements.</param>
		/// <exception cref="System.ArgumentNullException">comparison is null.</exception>
		/// <exception cref="System.ArgumentException">The implementation of comparison caused an error during the sort. For example, comparison might not return 0 when comparing an item with itself.</exception>
		TBuilder Sort(Comparison<T> comparison);

		/// <summary>
		/// Sorts the elements in a range of elements in list using the specified comparer.
		/// </summary>
		/// <param name="index">The zero-based starting index of the range to sort.</param>
		/// <param name="count">The length of the range to sort.</param>
		/// <param name="comparer">The System.Collections.Generic.IComparer`1 implementation to use when comparing
		/// elements, or null to use the default comparer System.Collections.Generic.Comparer`1.Default.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
		/// <exception cref="System.ArgumentException">index and count do not specify a valid range in the list.
		/// -or- The implementation of comparer caused an error during the sort. For example,
		/// comparer might not return 0 when comparing an item with itself.</exception>
		/// <exception cref="System.InvalidOperationException">comparer is null, and the default comparer System.Collections.Generic.Comparer`1.Default
		/// cannot find implementation of the System.IComparable`1 generic interface or the
		/// System.IComparable interface for type T.</exception>
		TBuilder Sort(int index, int count, IComparer<T>? comparer);

		/// <summary>
		/// Sorts the elements in the entire list using the default comparer.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">The default comparer System.Collections.Generic.Comparer`1.Default cannot find
		/// an implementation of the System.IComparable`1 generic interface or the System.IComparable
		/// interface for type T.</exception>
		TBuilder Sort();

		/// <summary>
		/// Sorts the elements in the entire list using the specified comparer.
		/// </summary>
		/// <param name="comparer">The System.Collections.Generic.IComparer`1 implementation to use when comparing
		/// elements, or null to use the default comparer System.Collections.Generic.Comparer`1.Default.</param>
		/// <exception cref="System.InvalidOperationException">comparer is null, and the default comparer System.Collections.Generic.Comparer`1.Default
		/// cannot find implementation of the System.IComparable`1 generic interface or the
		/// System.IComparable interface for type T.</exception>
		/// <exception cref="System.ArgumentException">The implementation of comparer caused an error during the sort. For example,
		/// comparer might not return 0 when comparing an item with itself.</exception>
		TBuilder Sort(IComparer<T>? comparer);

		/// <summary>
		///  Adds an object to the end of the list.
		/// </summary>
		/// <param name="item">The object to be added to the end of the list. The value can be null for reference types.</param>
		/// <param name="condition">Attempts to add the specified <c>item</c> only if the condition is true.</param>
		/// <param name="added">true if the item was added to the list successfully; otherwise, false.</param>
		TBuilder AddIf(T item, bool condition, out bool added);

		/// <summary>
		/// Adds the elements of the specified collection to the end of the list.
		/// </summary>
		/// <param name="collection">The collection whose elements should be added to the end of the list.
		/// The collection itself cannot be null, but it can contain elements that are null,
		/// if type T is a reference type.</param>>
		/// <param name="condition">Attempts to add the specified <c>collection</c> only if the condition is true.</param>
		/// <param name="added">true if the collection was added to the list successfully; otherwise, false.</param>
		TBuilder AddRangeIf(IEnumerable<T> collection, bool condition, out bool added);

		/// <summary>
		/// Removes all elements from the list.
		/// </summary>
		/// <param name="condition">Attempts to clear all items only if the condition is true.</param>
		/// <param name="cleared">true if all items was removed from the dictionary successfully; otherwise, false.</param>
		TBuilder ClearIf(bool condition, out bool cleared);

		/// <summary>
		/// Inserts an element into the list at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which item should be inserted.</param>
		/// <param name="item">The object to insert. The value can be null for reference types.</param>
		/// <param name="condition">Attempts to insert the specified <c>item</c> only if the condition is true.</param>
		/// <param name="inserted">true if the item was inserted to the list successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is greater than list.Count.</exception>
		TBuilder InsertIf(int index, T item, bool condition, out bool inserted);

		/// <summary>
		/// Inserts the elements of a collection into the list at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the new elements should be inserted.</param>
		/// <param name="collection">The collection whose elements should be inserted into the list.
		/// The collection itself cannot be null, but it can contain elements that are null,
		/// if type T is a reference type.</param>
		/// <param name="condition">Attempts to insert the specified <c>collection</c> only if the condition is true.</param>
		/// <param name="inserted">true if the collection was inserted to the list successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">collection is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is greater than list.Count.</exception>
		TBuilder InsertRangeIf(int index, IEnumerable<T> collection, bool condition, out bool inserted);

		/// <summary>
		/// Removes the first occurrence of a specific object from the list.
		/// </summary>
		/// <param name="item">The object to remove from the list. The value can be null for reference types.</param>
		/// <param name="condition">Attempts to remove the specified <c>item</c> only if the condition is true.</param>
		/// <param name="removed">true if item is successfully removed; otherwise, false. <paramref name="removed"/> is
		/// false if item was not found in the list.</param>
		TBuilder RemoveIf(T item, bool condition, out bool removed);

		TBuilder RemoveAtIf(int index, bool condition, out bool removed);

		TBuilder RemoveRangeIf(int index, int count, bool condition, out bool removed);

		TBuilder ReverseIf(int index, int count, bool condition, out bool reversed);

		TBuilder ReverseIf(bool condition, out bool reversed);

		TBuilder SetIf(int index, T item, bool condition, out bool set);

		TBuilder SortIf(Comparison<T> comparison, bool condition, out bool sorted);

		TBuilder SortIf(int index, int count, IComparer<T>? comparer, bool condition, out bool sorted);

		TBuilder SortIf(bool condition, out bool sorted);

		TBuilder SortIf(IComparer<T>? comparer, bool condition, out bool sorted);
	}

	public abstract class ListBuilderBase<TBuilder, T> : IListBuilder<TBuilder, T>
		where TBuilder : ListBuilderBase<TBuilder, T>
	{
		protected readonly TBuilder _builder;
		protected List<T> _list;

		protected ListBuilderBase(List<T> list)
		{
			_list = list;
			_builder = (TBuilder)this;
		}

		public virtual TBuilder Object(List<T> list)
		{
			_list = list ?? throw new ArgumentNullException(nameof(list));
			return _builder;
		}

		public List<T> ToObject()
			=> _list;

		public TBuilder Add(T item)
		{
			_list.Add(item);
			return _builder;
		}

		public TBuilder AddRange(IEnumerable<T> collection)
		{
			_list.AddRange(collection);
			return _builder;
		}

		public TBuilder Clear()
		{
			_list.Clear();
			return _builder;
		}

		public TBuilder Insert(int index, T item)
		{
			_list.Insert(index, item);
			return _builder;
		}

		public TBuilder InsertRange(int index, IEnumerable<T> collection)
		{
			_list.InsertRange(index, collection);
			return _builder;
		}

		public TBuilder Remove(T item, out bool removed)
		{
			removed = _list.Remove(item);
			return _builder;
		}

		public TBuilder RemoveAll(Predicate<T> match, out int removedCount)
		{
			removedCount = _list.RemoveAll(match);
			return _builder;
		}

		public TBuilder RemoveAt(int index)
		{
			_list.RemoveAt(index);
			return _builder;
		}

		public TBuilder RemoveRange(int index, int count)
		{
			_list.RemoveRange(index, count);
			return _builder;
		}

		public TBuilder Reverse(int index, int count)
		{
			_list.Reverse(index, count);
			return _builder;
		}

		public TBuilder Reverse()
		{
			_list.Reverse();
			return _builder;
		}

		public TBuilder Set(int index, T item)
		{
			_list[index] = item;
			return _builder;
		}

		public TBuilder Sort(Comparison<T> comparison)
		{
			_list.Sort(comparison);
			return _builder;
		}

		public TBuilder Sort(int index, int count, IComparer<T>? comparer)
		{
			_list.Sort(index, count, comparer);
			return _builder;
		}

		public TBuilder Sort()
		{
			_list.Sort();
			return _builder;
		}

		public TBuilder Sort(IComparer<T>? comparer)
		{
			_list.Sort(comparer);
			return _builder;
		}

		public TBuilder AddIf(T item, bool condition, out bool added)
		{
			if (!condition)
			{
				added = false;
				return _builder;
			}

			_list.Add(item);
			added = true;
			return _builder;
		}

		public TBuilder AddRangeIf(IEnumerable<T> collection, bool condition, out bool added)
		{
			if (!condition)
			{
				added = false;
				return _builder;
			}

			_list.AddRange(collection);
			added = true;
			return _builder;
		}

		public TBuilder ClearIf(bool condition, out bool cleared)
		{
			if (!condition)
			{
				cleared = false;
				return _builder;
			}

			_list.Clear();
			cleared = true;
			return _builder;
		}

		public TBuilder InsertIf(int index, T item, bool condition, out bool inserted)
		{
			if (!condition)
			{
				inserted = false;
				return _builder;
			}

			_list.Insert(index, item);
			inserted = true;
			return _builder;
		}

		public TBuilder InsertRangeIf(int index, IEnumerable<T> collection, bool condition, out bool inserted)
		{
			if (!condition)
			{
				inserted = false;
				return _builder;
			}

			_list.InsertRange(index, collection);
			inserted = true;
			return _builder;
		}

		public TBuilder RemoveIf(T item, bool condition, out bool removed)
		{
			if (!condition)
			{
				removed = false;
				return _builder;
			}

			removed = _list.Remove(item);
			return _builder;
		}

		public TBuilder RemoveAtIf(int index, bool condition, out bool removed)
		{
			if (!condition)
			{
				removed = false;
				return _builder;
			}

			_list.RemoveAt(index);
			removed = true;
			return _builder;
		}

		public TBuilder RemoveRangeIf(int index, int count, bool condition, out bool removed)
		{
			if (!condition)
			{
				removed = false;
				return _builder;
			}

			_list.RemoveRange(index, count);
			removed = true;
			return _builder;
		}

		public TBuilder ReverseIf(int index, int count, bool condition, out bool reversed)
		{
			if (!condition)
			{
				reversed = false;
				return _builder;
			}

			_list.Reverse(index, count);
			reversed = true;
			return _builder;
		}

		public TBuilder ReverseIf(bool condition, out bool reversed)
		{
			if (!condition)
			{
				reversed = false;
				return _builder;
			}

			_list.Reverse();
			reversed = true;
			return _builder;
		}

		public TBuilder SetIf(int index, T item, bool condition, out bool set)
		{
			if (!condition)
			{
				set = false;
				return _builder;
			}

			_list[index] = item;
			set = true;
			return _builder;
		}

		public TBuilder SortIf(Comparison<T> comparison, bool condition, out bool sort)
		{
			if (!condition)
			{
				sort = false;
				return _builder;
			}

			_list.Sort(comparison);
			sort = true;
			return _builder;
		}

		public TBuilder SortIf(int index, int count, IComparer<T>? comparer, bool condition, out bool sorted)
		{
			if (!condition)
			{
				sorted = false;
				return _builder;
			}

			_list.Sort(index, count, comparer);
			sorted = true;
			return _builder;
		}

		public TBuilder SortIf(bool condition, out bool sorted)
		{
			if (!condition)
			{
				sorted = false;
				return _builder;
			}

			_list.Sort();
			sorted = true;
			return _builder;
		}

		public TBuilder SortIf(IComparer<T>? comparer, bool condition, out bool sorted)
		{
			if (!condition)
			{
				sorted = false;
				return _builder;
			}

			_list.Sort(comparer);
			sorted = true;
			return _builder;
		}
	}

	public class ListBuilder<T> : ListBuilderBase<ListBuilder<T>, T>
	{
		public ListBuilder()
			: this(new List<T>())
		{
		}

		public ListBuilder(List<T> list)
			: base(list)
		{
		}

		public static implicit operator List<T>?(ListBuilder<T> builder)
		{
			if (builder == null)
				return null;

			return builder._list;
		}

		public static implicit operator ListBuilder<T>?(List<T> list)
		{
			if (list == null)
				return null;

			return new ListBuilder<T>(list);
		}
	}
}
