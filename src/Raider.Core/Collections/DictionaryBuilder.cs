using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Raider.Collections
{
	/// <summary>
	/// Represents a builder for dictionary of keys and values.
	/// </summary>
	/// <typeparam name="TBuilder">The builder.</typeparam>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	public interface IDictionaryBuilder<TBuilder, TKey, TValue>
		where TBuilder : IDictionaryBuilder<TBuilder, TKey, TValue>
		where TKey : notnull
	{
		/// <summary>
		/// Sets the dictionary to build.
		/// </summary>
		TBuilder Object(Dictionary<TKey, TValue?> dictionary);

		/// <summary>
		/// Returns entire dictionary object.
		/// </summary>
		Dictionary<TKey, TValue?> ToObject();

		/// <summary>
		/// Adds the specified <c>key</c> and <c>value</c> to the dictionary.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add. The value can be null for reference types.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		/// <exception cref="System.ArgumentException"> An element with the same key already exists in the dictionary.</exception>
		TBuilder Add(TKey key, TValue? value);

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary. 
		/// </summary>
		/// <remarks>
		/// Unlike the <see cref="Add(TKey, TValue?)"/> method, this method doesn't throw an exception if the element with the given key exists in the dictionary.
		/// Unlike the <see cref="Set(TKey, TValue?)"/>, TryAdd doesn't override the element if the element with the given key exists in the dictionary.
		/// If the key already exists, <see cref="TryAdd(TKey, TValue?, out bool)"/> does nothing and <paramref name="added"/> sets to false.
		/// </remarks>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add. It can be null.</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder TryAdd(TKey key, TValue? value, out bool added);

		/// <summary>
		/// Removes all keys and values from the dictionary.
		/// </summary>
		TBuilder Clear();

		/// <summary>
		/// Removes the value with the specified key from the dictionary,
		/// and copies the element to the value parameter.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <param name="elementRemoved">true if the element is successfully found and removed; otherwise, false.</param>
		/// <param name="value">The removed element.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder Remove(TKey key, out bool elementRemoved, [MaybeNullWhen(false)] out TValue? value);

		/// <summary>
		/// Removes the value with the specified key from the dictionary.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <param name="elementRemoved">true if the element is successfully found and removed; otherwise, false.
		/// Parameter is false if key is not found in the dictionary.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder Remove(TKey key, out bool elementRemoved);

		/// <summary>
		/// Sets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key of the value to set.</param>
		/// <param name="value">The value of the element to set. The value can be null for reference types.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">The key does not exist in the collection.</exception>
		TBuilder Set(TKey key, TValue? value);

		/// <summary>
		/// Adds the specified <c>key</c> and <c>value</c> to the dictionary if condition == true.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="condition">Attempts to add the specified <c>value</c> only if the condition is true.</param>
		/// <param name="value">The value of the element to add. The value can be null for reference types.</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		/// <exception cref="System.ArgumentException"> An element with the same key already exists in the dictionary.</exception>
		TBuilder AddIf(TKey key, bool condition, TValue? value, out bool added);

		/// <summary>
		/// Attempts to add the specified <c>key</c> and <c>value</c> to the dictionary if condition == true.
		/// </summary>
		/// <remarks>
		/// Unlike the <see cref="AddIf(TKey, bool, TValue?, out bool)"/> method, this method doesn't throw an exception if the element with the given key exists in the dictionary.
		/// Unlike the <see cref="SetIf(TKey, bool, TValue?, out bool)"/>, TryAdd doesn't override the element if the element with the given key exists in the dictionary.
		/// If the key already exists, <see cref="TryAddIf(TKey, bool, TValue?, out bool)"/> does nothing and <paramref name="added"/> sets to false.
		/// </remarks>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="condition">Attempts to add the specified <c>value</c> only if the condition is true.</param>
		/// <param name="value">The value of the element to add. It can be null.</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder TryAddIf(TKey key, bool condition, TValue? value, out bool added);

		/// <summary>
		/// Removes all keys and values from the dictionary if condition == true.
		/// </summary>
		/// <param name="condition">Attempts to clear all values only if the condition is true.</param>
		/// <param name="cleared">true if all key/value pair was removed from the dictionary successfully; otherwise, false.</param>
		TBuilder ClearIf(bool condition, out bool cleared);

		/// <summary>
		/// Removes the value with the specified key from the dictionary,
		/// and copies the element to the value parameter if condition == true.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <param name="condition">Attempts to remove the specified <c>key</c> only if the condition is true.</param>
		/// <param name="elementRemoved">true if the element is successfully found and removed; otherwise, false.</param>
		/// <param name="value">The removed element.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder RemoveIf(TKey key, bool condition, out bool elementRemoved, [MaybeNullWhen(false)] out TValue? value);

		/// <summary>
		/// Removes the value with the specified key from the dictionary if condition == true.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <param name="condition">Attempts to remove the specified <c>key</c> only if the condition is true.</param>
		/// <param name="elementRemoved">true if the element is successfully found and removed; otherwise, false.
		/// Parameter is false if key is not found in the dictionary.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder RemoveIf(TKey key, bool condition, out bool elementRemoved);

		/// <summary>
		/// Sets the value associated with the specified key if condition == true.
		/// </summary>
		/// <param name="key">The key of the value to set.</param>
		/// <param name="condition">Attempts to set the specified <c>value</c> only if the condition is true.</param>
		/// <param name="value">The value of the element to set. The value can be null for reference types.</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">The key does not exist in the collection.</exception>
		TBuilder SetIf(TKey key, bool condition, TValue? value, out bool set);
	}

	///<inheritdoc cref="IDictionaryBuilder{TBuilder, TKey, TValue?}"/>
	/// <summary>
	/// Represents a builder for collection of keys and values.
	/// </summary>
	/// <typeparam name="TBuilder">The builder.</typeparam>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	public abstract class DictionaryBuilderBase<TBuilder, TKey, TValue> : IDictionaryBuilder<TBuilder, TKey, TValue?>
		where TBuilder : DictionaryBuilderBase<TBuilder, TKey, TValue>
		where TKey : notnull
	{
		protected readonly TBuilder _builder;
		protected Dictionary<TKey, TValue?> _dict;

		protected DictionaryBuilderBase(Dictionary<TKey, TValue?> dictionary)
		{
			_dict = dictionary;
			_builder = (TBuilder)this;
		}

		public virtual TBuilder Object(Dictionary<TKey, TValue?> dictionary)
		{
			_dict = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
			return _builder;
		}

		public Dictionary<TKey, TValue?> ToObject()
			=> _dict;

		public TBuilder Add(TKey key, TValue? value)
		{
			_dict.Add(key, value);
			return _builder;
		}

		public TBuilder TryAdd(TKey key, TValue? value, out bool added)
		{
			added = _dict.TryAdd(key, value);
			return _builder;
		}

		public TBuilder Clear()
		{
			_dict.Clear();
			return _builder;
		}

		public TBuilder Remove(TKey key, out bool elementRemoved, [MaybeNullWhen(false)] out TValue? value)
		{
			elementRemoved = _dict.Remove(key, out value);
			return _builder;
		}

		public TBuilder Remove(TKey key, out bool elementRemoved)
		{
			elementRemoved = _dict.Remove(key);
			return _builder;
		}

		public TBuilder Set(TKey key, TValue? value)
		{
			_dict[key] = value;
			return _builder;
		}

		public TBuilder AddIf(TKey key, bool condition, TValue? value, out bool added)
		{
			if (!condition)
			{
				added = false;
				return _builder;
			}

			_dict.Add(key, value);
			added = true;
			return _builder;
		}

		public TBuilder TryAddIf(TKey key, bool condition, TValue? value, out bool added)
		{
			if (!condition)
			{
				added = false;
				return _builder;
			}

			added = _dict.TryAdd(key, value);
			return _builder;
		}

		public TBuilder ClearIf(bool condition, out bool cleared)
		{
			if (!condition)
			{
				cleared = false;
				return _builder;
			}

			_dict.Clear();
			cleared = true;
			return _builder;
		}

		public TBuilder RemoveIf(TKey key, bool condition, out bool elementRemoved, [MaybeNullWhen(false)] out TValue? value)
		{
			if (!condition)
			{
				elementRemoved = false;
				value = default;
				return _builder;
			}

			elementRemoved = _dict.Remove(key, out value);
			return _builder;
		}

		public TBuilder RemoveIf(TKey key, bool condition, out bool elementRemoved)
		{
			if (!condition)
			{
				elementRemoved = false;
				return _builder;
			}

			elementRemoved = _dict.Remove(key);
			return _builder;
		}

		public TBuilder SetIf(TKey key, bool condition, TValue? value, out bool set)
		{
			if (!condition)
			{
				set = false;
				return _builder;
			}

			_dict[key] = value;
			set = true;
			return _builder;
		}
	}

	///<inheritdoc cref="DictionaryBuilderBase{TBuilder, TKey, TValue}"/>
	/// <summary>
	/// Represents a builder for collection of keys and values.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	public class DictionaryBuilder<TKey, TValue> : DictionaryBuilderBase<DictionaryBuilder<TKey, TValue?>, TKey, TValue?>
		where TKey : notnull
	{
		public DictionaryBuilder()
			: this(new Dictionary<TKey, TValue?>())
		{
		}

		public DictionaryBuilder(Dictionary<TKey, TValue?> dictionary)
			: base(dictionary)
		{
		}

		public static implicit operator Dictionary<TKey, TValue?>?(DictionaryBuilder<TKey, TValue?> builder)
		{
			if (builder == null)
				return null;

			return builder._dict;
		}

		public static implicit operator DictionaryBuilder<TKey, TValue?>?(Dictionary<TKey, TValue?> dictionary)
		{
			if (dictionary == null)
				return null;

			return new DictionaryBuilder<TKey, TValue?>(dictionary);
		}
	}
}
