using System;
using System.Collections.Generic;

namespace Raider.Collections
{
	/// <summary>
	/// Represents a builder for dictionary of keys and class values.
	/// </summary>
	/// <typeparam name="TBuilder">The builder.</typeparam>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	public interface IDictionaryClassBuilder<TBuilder, TKey, TValue> : IDictionaryBuilder<TBuilder, TKey, TValue>
		where TBuilder : IDictionaryClassBuilder<TBuilder, TKey, TValue>
		where TKey : notnull
		where TValue : class
	{
		/// <summary>
		/// Attempts to add the specified key and value to the dictionary if the value is not null.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add.</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfNotNull(TKey key, TValue value, out bool added);

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary. If the value is null, the default value is used.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add.</param>
		/// <param name="defaultValue">The default value function of the element to add (used if the value is null or returns null).</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfNotNull(TKey key, TValue value, Func<TValue> defaultValue, out bool added);

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary. If the value function is null or returns null, the default value function is used.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="condition">The value function of the element to add.</param>
		/// <param name="value">The value function of the element to add.</param>
		/// <param name="defaultValue">The default value function of the element to add (used if the value function is null or returns null).</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfNotNull(TKey key, bool condition, Func<TValue> value, Func<TValue> defaultValue, out bool added);

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary if the value is not null.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="value">The value of the element to set.</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfNotNull(TKey key, TValue value, out bool set);

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary. If the value is null, the default value is used.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="value">The value of the element to set.</param>
		/// <param name="defaultValue">The default value function of the element to set (used if the value is null or returns null).</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfNotNull(TKey key, TValue value, Func<TValue> defaultValue, out bool set);

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary. If the value function is null or returns null, the default value function is used.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="condition">The value function of the element to set.</param>
		/// <param name="value">The value function of the element to set.</param>
		/// <param name="defaultValue">The default value function of the element to set (used if the value function is null or returns null).</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfNotNull(TKey key, bool condition, Func<TValue> value, Func<TValue> defaultValue, out bool set);
	}

	///<inheritdoc cref="IDictionaryBuilder{TBuilder, TKey, TValue}"/>
	/// <summary>
	/// Represents a builder for collection of keys and class values.
	/// </summary>
	/// <typeparam name="TBuilder">The builder.</typeparam>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	public abstract class DictionaryClassBuilderBase<TBuilder, TKey, TValue> : DictionaryBuilderBase<TBuilder, TKey, TValue>, IDictionaryClassBuilder<TBuilder, TKey, TValue>
		where TBuilder : DictionaryClassBuilderBase<TBuilder, TKey, TValue>
		where TKey : notnull
		where TValue : class
	{
		protected DictionaryClassBuilderBase(Dictionary<TKey, TValue?> dictionary)
			: base(dictionary)
		{
		}

		public TBuilder AddIfNotNull(TKey key, TValue value, out bool added)
		{
			added = value != null && _dict.TryAdd(key, value);
			return _builder;
		}

		public TBuilder AddIfNotNull(TKey key, TValue value, Func<TValue> defaultValue, out bool added)
		{
			TValue val;
			if (value == null)
			{
				if (defaultValue == null)
				{
					added = false;
					return _builder;
				}
				else
				{
					val = defaultValue.Invoke();
					if (val == null)
					{
						added = false;
						return _builder;
					}
				}
			}
			else
			{
				val = value;
			}

			added = _dict.TryAdd(key, val);
			return _builder;
		}

		public TBuilder AddIfNotNull(TKey key, bool condition, Func<TValue> value, Func<TValue> defaultValue, out bool added)
		{
			if (!condition)
			{
				added = false;
				return _builder;
			}

			TValue val;
			if (value == null)
			{
				if (defaultValue == null)
				{
					added = false;
					return _builder;
				}
				else
				{
					val = defaultValue.Invoke();
					if (val == null)
					{
						added = false;
						return _builder;
					}
				}
			}
			else
			{
				val = value.Invoke();
				if (val == null)
				{
					if (defaultValue == null)
					{
						added = false;
						return _builder;
					}
					else
					{
						val = defaultValue.Invoke();
						if (val == null)
						{
							added = false;
							return _builder;
						}
					}
				}
			}

			added = _dict.TryAdd(key, val);
			return _builder;
		}

		public TBuilder SetIfNotNull(TKey key, TValue value, out bool set)
		{
			set = value != null;

			if (value != null)
				_dict[key] = value;

			return _builder;
		}

		public TBuilder SetIfNotNull(TKey key, TValue value, Func<TValue> defaultValue, out bool set)
		{
			TValue val;
			if (value == null)
			{
				if (defaultValue == null)
				{
					set = false;
					return _builder;
				}
				else
				{
					val = defaultValue.Invoke();
					if (val == null)
					{
						set = false;
						return _builder;
					}
				}
			}
			else
			{
				val = value;
			}

			set = true;
			_dict[key] = val;
			return _builder;
		}

		public TBuilder SetIfNotNull(TKey key, bool condition, Func<TValue> value, Func<TValue> defaultValue, out bool set)
		{
			if (!condition)
			{
				set = false;
				return _builder;
			}

			TValue val;
			if (value == null)
			{
				if (defaultValue == null)
				{
					set = false;
					return _builder;
				}
				else
				{
					val = defaultValue.Invoke();
					if (val == null)
					{
						set = false;
						return _builder;
					}
				}
			}
			else
			{
				val = value.Invoke();
				if (val == null)
				{
					if (defaultValue == null)
					{
						set = false;
						return _builder;
					}
					else
					{
						val = defaultValue.Invoke();
						if (val == null)
						{
							set = false;
							return _builder;
						}
					}
				}
			}

			set = true;
			_dict[key] = val;
			return _builder;
		}
	}

	///<inheritdoc cref="DictionaryBuilderBase{TBuilder, TKey, TValue}"/>
	/// <summary>
	/// Represents a builder for collection of keys and class values.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	public class DictionaryClassBuilder<TKey, TValue> : DictionaryClassBuilderBase<DictionaryClassBuilder<TKey, TValue>, TKey, TValue>
		where TKey : notnull
		where TValue : class
	{
		public DictionaryClassBuilder()
			: this(new Dictionary<TKey, TValue?>())
		{
		}

		public DictionaryClassBuilder(Dictionary<TKey, TValue?> dictionary)
			: base(dictionary)
		{
		}

		public static implicit operator Dictionary<TKey, TValue?>?(DictionaryClassBuilder<TKey, TValue> builder)
		{
			if (builder == null)
				return null;

			return builder._dict;
		}

		public static implicit operator DictionaryClassBuilder<TKey, TValue>?(Dictionary<TKey, TValue?> dictionary)
		{
			if (dictionary == null)
				return null;

			return new DictionaryClassBuilder<TKey, TValue>(dictionary);
		}
	}
}
