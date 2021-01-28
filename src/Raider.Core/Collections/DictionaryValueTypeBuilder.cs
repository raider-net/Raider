using System;
using System.Collections.Generic;

namespace Raider.Collections
{
	/// <summary>
	/// Represents a builder for dictionary of keys and value types.
	/// </summary>
	/// <typeparam name="TBuilder">The builder.</typeparam>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the value types in the dictionary.</typeparam>
	public interface IDictionaryValueTypeBuilder<TBuilder, TKey, TValue> : IDictionaryBuilder<TBuilder, TKey, TValue>
		where TBuilder : IDictionaryValueTypeBuilder<TBuilder, TKey, TValue>
		where TKey : notnull
		where TValue : struct
	{
		/// <summary>
		/// Attempts to add the specified key and value to the dictionary if value.HasValue.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add.</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfHasValue(TKey key, TValue? value, out bool added);

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary. If value.HasValue == false, the default value is used.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add.</param>
		/// <param name="defaultValue">The default value function of the element to add (used if the value function is null or returns no value).</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfHasValue(TKey key, TValue? value, Func<TValue?> defaultValue, out bool added);

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary. If value function is null or returns no value, the default value function is used.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="condition">The value function of the element to add.</param>
		/// <param name="value">The value function of the element to add.</param>
		/// <param name="defaultValue">The default value function of the element to add (used if the value function is null or returns no value).</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfHasValue(TKey key, bool condition, Func<TValue?> value, Func<TValue?> defaultValue, out bool added);

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary if value.HasValue.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="value">The value of the element to set.</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfHasValue(TKey key, TValue? value, out bool set);

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary. If value.HasValue == false, the default value is used.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="value">The value of the element to set.</param>
		/// <param name="defaultValue">The default value function of the element to set (used if the value function is null or returns no value).</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfHasValue(TKey key, TValue? value, Func<TValue?> defaultValue, out bool set);

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary. If value function is null or returns no value, the default value function is used.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="condition">The value function of the element to set.</param>
		/// <param name="value">The value function of the element to set.</param>
		/// <param name="defaultValue">The default value function of the element to set (used if the value function is null or returns no value).</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfHasValue(TKey key, bool condition, Func<TValue?> value, Func<TValue?> defaultValue, out bool set);
	}

	/// <summary>
	/// Represents a builder for dictionary of keys and value types.
	/// </summary>
	/// <typeparam name="TBuilder">The builder.</typeparam>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the value types in the dictionary.</typeparam>
	public abstract class DictionaryValueTypeBuilderBase<TBuilder, TKey, TValue> : DictionaryBuilderBase<TBuilder, TKey, TValue>, IDictionaryValueTypeBuilder<TBuilder, TKey, TValue>
		where TBuilder : DictionaryValueTypeBuilderBase<TBuilder, TKey, TValue>
		where TKey : notnull
		where TValue : struct
	{
		protected DictionaryValueTypeBuilderBase(Dictionary<TKey, TValue> dictionary)
			: base(dictionary)
		{
		}

		public TBuilder AddIfHasValue(TKey key, TValue? value, out bool added)
		{
			added = value.HasValue && _dict.TryAdd(key, value.Value);
			return _builder;
		}

		public TBuilder AddIfHasValue(TKey key, TValue? value, Func<TValue?> defaultValue, out bool added)
		{
			TValue val;
			if (value.HasValue)
			{
				val = value.Value;
			}
			else
			{
				if (defaultValue == null)
				{
					added = false;
					return _builder;
				}
				else
				{
					var nullableVal = defaultValue.Invoke();
					if (nullableVal.HasValue)
					{
						val = nullableVal.Value;
					}
					else
					{
						added = false;
						return _builder;
					}
				}
			}

			added = _dict.TryAdd(key, val);
			return _builder;
		}

		public TBuilder AddIfHasValue(TKey key, bool condition, Func<TValue?> value, Func<TValue?> defaultValue, out bool added)
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
					var nullableVal = defaultValue.Invoke();
					if (nullableVal.HasValue)
					{
						val = nullableVal.Value;
					}
					else
					{
						added = false;
						return _builder;
					}
				}
			}
			else
			{
				var nullableVal = value.Invoke();
				if (nullableVal.HasValue)
				{
					val = nullableVal.Value;
				}
				else
				{
					if (defaultValue == null)
					{
						added = false;
						return _builder;
					}
					else
					{
						nullableVal = defaultValue.Invoke();
						if (nullableVal.HasValue)
						{
							val = nullableVal.Value;
						}
						else
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

		public TBuilder SetIfHasValue(TKey key, TValue? value, out bool set)
		{
			set = value.HasValue;

			if (value.HasValue)
				_dict[key] = value.Value;

			return _builder;
		}

		public TBuilder SetIfHasValue(TKey key, TValue? value, Func<TValue?> defaultValue, out bool set)
		{
			TValue val;
			if (value.HasValue)
			{
				val = value.Value;
			}
			else
			{
				if (defaultValue == null)
				{
					set = false;
					return _builder;
				}
				else
				{
					var nullableVal = defaultValue.Invoke();
					if (nullableVal.HasValue)
					{
						val = nullableVal.Value;
					}
					else
					{
						set = false;
						return _builder;
					}
				}
			}

			set = true;
			_dict[key] = val;
			return _builder;
		}

		public TBuilder SetIfHasValue(TKey key, bool condition, Func<TValue?> value, Func<TValue?> defaultValue, out bool set)
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
					var nullableVal = defaultValue.Invoke();
					if (nullableVal.HasValue)
					{
						val = nullableVal.Value;
					}
					else
					{
						set = false;
						return _builder;
					}
				}
			}
			else
			{
				var nullableVal = value.Invoke();
				if (nullableVal.HasValue)
				{
					val = nullableVal.Value;
				}
				else
				{
					if (defaultValue == null)
					{
						set = false;
						return _builder;
					}
					else
					{
						nullableVal = defaultValue.Invoke();
						if (nullableVal.HasValue)
						{
							val = nullableVal.Value;
						}
						else
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

	/// <summary>
	/// Represents a builder for dictionary of keys and value types.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the value types in the dictionary.</typeparam>
	public class DictionaryValueTypeBuilder<TKey, TValue> : DictionaryValueTypeBuilderBase<DictionaryValueTypeBuilder<TKey, TValue>, TKey, TValue>
		where TKey : notnull
		where TValue : struct
	{
		public DictionaryValueTypeBuilder()
			: this(new Dictionary<TKey, TValue>())
		{
		}

		public DictionaryValueTypeBuilder(Dictionary<TKey, TValue> dictionary)
			: base(dictionary)
		{
		}

		public static implicit operator Dictionary<TKey, TValue>?(DictionaryValueTypeBuilder<TKey, TValue> builder)
		{
			if (builder == null)
				return null;

			return builder._dict;
		}

		public static implicit operator DictionaryValueTypeBuilder<TKey, TValue>?(Dictionary<TKey, TValue> dictionary)
		{
			if (dictionary == null)
				return null;

			return new DictionaryValueTypeBuilder<TKey, TValue>(dictionary);
		}
	}
}
