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
	public interface IDictionaryBuilder<TBuilder, TKey> : IDictionaryBuilder<TBuilder, TKey, object>
		where TBuilder : IDictionaryBuilder<TBuilder, TKey>
		where TKey : notnull
	{
		/// <summary>
		/// Attempts to add the specified key and value to the dictionary if the value is not null.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add.</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfNotNull<TValue>(TKey key, TValue? value, out bool added)
			where TValue : class;

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary. If the value is null, the default value is used.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add.</param>
		/// <param name="defaultValue">The default value function of the element to add (used if the value is null or returns null).</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfNotNull<TValue>(TKey key, TValue? value, Func<TValue>? defaultValue, out bool added)
			where TValue : class;

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary. If the value function is null or returns null, the default value function is used.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="condition">The value function of the element to add.</param>
		/// <param name="value">The value function of the element to add.</param>
		/// <param name="defaultValue">The default value function of the element to add (used if the value function is null or returns null).</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfNotNull<TValue>(TKey key, bool condition, Func<TValue>? value, Func<TValue>? defaultValue, out bool added)
			where TValue : class;

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary if the value is not null.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="value">The value of the element to set.</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfNotNull<TValue>(TKey key, TValue? value, out bool set)
			where TValue : class;

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary. If the value is null, the default value is used.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="value">The value of the element to set.</param>
		/// <param name="defaultValue">The default value function of the element to set (used if the value is null or returns null).</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfNotNull<TValue>(TKey key, TValue? value, Func<TValue>? defaultValue, out bool set)
			where TValue : class;

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary. If the value function is null or returns null, the default value function is used.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="condition">The value function of the element to set.</param>
		/// <param name="value">The value function of the element to set.</param>
		/// <param name="defaultValue">The default value function of the element to set (used if the value function is null or returns null).</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfNotNull<TValue>(TKey key, bool condition, Func<TValue>? value, Func<TValue>? defaultValue, out bool set)
			where TValue : class;

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary if value.HasValue.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add.</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfHasValue<TValue>(TKey key, TValue? value, out bool added)
			where TValue : struct;

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary. If value.HasValue == false, the default value is used.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add.</param>
		/// <param name="defaultValue">The default value function of the element to add (used if the value function is null or returns no value).</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfHasValue<TValue>(TKey key, TValue? value, Func<TValue?> defaultValue, out bool added)
			where TValue : struct;

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary. If value function is null or returns no value, the default value function is used.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="condition">The value function of the element to add.</param>
		/// <param name="value">The value function of the element to add.</param>
		/// <param name="defaultValue">The default value function of the element to add (used if the value function is null or returns no value).</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfHasValue<TValue>(TKey key, bool condition, Func<TValue?> value, Func<TValue?> defaultValue, out bool added)
			where TValue : struct;

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary if value.HasValue.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="value">The value of the element to set.</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfHasValue<TValue>(TKey key, TValue? value, out bool set)
			where TValue : struct;

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary. If value.HasValue == false, the default value is used.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="value">The value of the element to set.</param>
		/// <param name="defaultValue">The default value function of the element to set (used if the value function is null or returns no value).</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfHasValue<TValue>(TKey key, TValue? value, Func<TValue?> defaultValue, out bool set)
			where TValue : struct;

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary. If value function is null or returns no value, the default value function is used.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="condition">The value function of the element to set.</param>
		/// <param name="value">The value function of the element to set.</param>
		/// <param name="defaultValue">The default value function of the element to set (used if the value function is null or returns no value).</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfHasValue<TValue>(TKey key, bool condition, Func<TValue?> value, Func<TValue?> defaultValue, out bool set)
			where TValue : struct;

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary if the value is not empty.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add.</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfNotEmpty(TKey key, string? value, out bool added);

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary. If the value is empty, the default value is used.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add.</param>
		/// <param name="defaultValue">The default value of the element to add (used if the value is empty).</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfNotEmpty(TKey key, string? value, Func<string>? defaultValue, out bool added);

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary. If the value function is null or returns empty string, the default value function is used.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="condition">The value function of the element to add.</param>
		/// <param name="value">The value function of the element to add.</param>
		/// <param name="defaultValue">The default value function of the element to add (used if the value function is null or returns empty string).</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfNotEmpty(TKey key, bool condition, Func<string>? value, Func<string>? defaultValue, out bool added);

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary if the value is not white space.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add.</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfNotWhiteSpace(TKey key, string? value, out bool added);

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary. If the value is white space, the default value is used.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add.</param>
		/// <param name="defaultValue">The default value of the element to add (used if the value is  white space).</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfNotWhiteSpace(TKey key, string? value, Func<string>? defaultValue, out bool added);

		/// <summary>
		/// Attempts to add the specified key and value to the dictionary. If the value function is null or returns white space, the default value function is used.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="condition">The value function of the element to add.</param>
		/// <param name="value">The value function of the element to add.</param>
		/// <param name="defaultValue">The default value function of the element to add (used if the value function is null or returns white space).</param>
		/// <param name="added">true if the key/value pair was added to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder AddIfNotWhiteSpace(TKey key, bool condition, Func<string>? value, Func<string>? defaultValue, out bool added);

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary if the value is not empty.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="value">The value of the element to set.</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfNotEmpty(TKey key, string? value, out bool set);

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary. If the value is empty, the default value is used.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="value">The value of the element to set.</param>
		/// <param name="defaultValue">The default value of the element to set (used if the value is empty).</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfNotEmpty(TKey key, string? value, Func<string>? defaultValue, out bool set);

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary. If the value function is null or returns empty string, the default value function is used.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="condition">The value function of the element to set.</param>
		/// <param name="value">The value function of the element to set.</param>
		/// <param name="defaultValue">The default value function of the element to set (used if the value function is null or returns empty string).</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfNotEmpty(TKey key, bool condition, Func<string>? value, Func<string>? defaultValue, out bool set);

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary if the value is not white space.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="value">The value of the element to set.</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfNotWhiteSpace(TKey key, string? value, out bool set);

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary. If the value is white space, the default value is used.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="value">The value of the element to set.</param>
		/// <param name="defaultValue">The default value of the element to set (used if the value is  white space).</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfNotWhiteSpace(TKey key, string? value, Func<string>? defaultValue, out bool set);

		/// <summary>
		/// Attempts to set the specified key and value to the dictionary. If the value function is null or returns white space, the default value function is used.
		/// </summary>
		/// <param name="key">The key of the element to set.</param>
		/// <param name="condition">The value function of the element to set.</param>
		/// <param name="value">The value function of the element to set.</param>
		/// <param name="defaultValue">The default value function of the element to set (used if the value function is null or returns white space).</param>
		/// <param name="set">true if the key/value pair was set to the dictionary successfully; otherwise, false.</param>
		/// <exception cref="System.ArgumentNullException">key is null.</exception>
		TBuilder SetIfNotWhiteSpace(TKey key, bool condition, Func<string>? value, Func<string>? defaultValue, out bool set);
	}

	///<inheritdoc cref="IDictionaryBuilder{TBuilder, TKey, TValue}"/>
	/// <summary>
	/// Represents a builder for collection of keys and class values.
	/// </summary>
	/// <typeparam name="TBuilder">The builder.</typeparam>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	public abstract class DictionaryBuilderBase<TBuilder, TKey> : DictionaryBuilderBase<TBuilder, TKey, object>, IDictionaryBuilder<TBuilder, TKey>
		where TBuilder : DictionaryBuilderBase<TBuilder, TKey>
		where TKey : notnull
	{
		protected DictionaryBuilderBase(Dictionary<TKey, object?> dictionary)
			: base(dictionary)
		{
		}

		public TBuilder AddIfNotNull<TValue>(TKey key, TValue? value, out bool added)
			where TValue : class
		{
			added = value != null && _dict.TryAdd(key, value);
			return _builder;
		}

		public TBuilder AddIfNotNull<TValue>(TKey key, TValue? value, Func<TValue>? defaultValue, out bool added)
			where TValue : class
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

		public TBuilder AddIfNotNull<TValue>(TKey key, bool condition, Func<TValue>? value, Func<TValue>? defaultValue, out bool added)
			where TValue : class
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

		public TBuilder SetIfNotNull<TValue>(TKey key, TValue? value, out bool set)
			where TValue : class
		{
			set = value != null;

			if (value != null)
				_dict[key] = value;

			return _builder;
		}

		public TBuilder SetIfNotNull<TValue>(TKey key, TValue? value, Func<TValue>? defaultValue, out bool set)
			where TValue : class
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

		public TBuilder SetIfNotNull<TValue>(TKey key, bool condition, Func<TValue>? value, Func<TValue>? defaultValue, out bool set)
			where TValue : class
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

		public TBuilder AddIfHasValue<TValue>(TKey key, TValue? value, out bool added)
			where TValue : struct
		{
			added = value.HasValue && _dict.TryAdd(key, value.Value);
			return _builder;
		}

		public TBuilder AddIfHasValue<TValue>(TKey key, TValue? value, Func<TValue?> defaultValue, out bool added)
			where TValue : struct
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

		public TBuilder AddIfHasValue<TValue>(TKey key, bool condition, Func<TValue?> value, Func<TValue?> defaultValue, out bool added)
			where TValue : struct
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

		public TBuilder SetIfHasValue<TValue>(TKey key, TValue? value, out bool set)
			where TValue : struct
		{
			set = value.HasValue;

			if (value.HasValue)
				_dict[key] = value.Value;

			return _builder;
		}

		public TBuilder SetIfHasValue<TValue>(TKey key, TValue? value, Func<TValue?> defaultValue, out bool set)
			where TValue : struct
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

		public TBuilder SetIfHasValue<TValue>(TKey key, bool condition, Func<TValue?> value, Func<TValue?> defaultValue, out bool set)
			where TValue : struct
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

		public TBuilder AddIfNotEmpty(TKey key, string? value, out bool added)
		{
			added = !string.IsNullOrEmpty(value) && _dict.TryAdd(key, value);
			return _builder;
		}

		public TBuilder AddIfNotEmpty(TKey key, string? value, Func<string>? defaultValue, out bool added)
		{
			string val;
			if (string.IsNullOrEmpty(value))
			{
				if (defaultValue == null)
				{
					added = false;
					return _builder;
				}
				else
				{
					val = defaultValue.Invoke();
					if (string.IsNullOrEmpty(val))
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

		public TBuilder AddIfNotEmpty(TKey key, bool condition, Func<string>? value, Func<string>? defaultValue, out bool added)
		{
			if (!condition)
			{
				added = false;
				return _builder;
			}

			string val;
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
					if (string.IsNullOrEmpty(val))
					{
						added = false;
						return _builder;
					}
				}
			}
			else
			{
				val = value.Invoke();
				if (string.IsNullOrEmpty(val))
				{
					if (defaultValue == null)
					{
						added = false;
						return _builder;
					}
					else
					{
						val = defaultValue.Invoke();
						if (string.IsNullOrEmpty(val))
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

		public TBuilder AddIfNotWhiteSpace(TKey key, string? value, out bool added)
		{
			added = !string.IsNullOrWhiteSpace(value) && _dict.TryAdd(key, value);
			return _builder;
		}

		public TBuilder AddIfNotWhiteSpace(TKey key, string? value, Func<string>? defaultValue, out bool added)
		{
			string val;
			if (string.IsNullOrWhiteSpace(value))
			{
				if (defaultValue == null)
				{
					added = false;
					return _builder;
				}
				else
				{
					val = defaultValue.Invoke();
					if (string.IsNullOrWhiteSpace(val))
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

		public TBuilder AddIfNotWhiteSpace(TKey key, bool condition, Func<string>? value, Func<string>? defaultValue, out bool added)
		{
			if (!condition)
			{
				added = false;
				return _builder;
			}

			string val;
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
					if (string.IsNullOrWhiteSpace(val))
					{
						added = false;
						return _builder;
					}
				}
			}
			else
			{
				val = value.Invoke();
				if (string.IsNullOrWhiteSpace(val))
				{
					if (defaultValue == null)
					{
						added = false;
						return _builder;
					}
					else
					{
						val = defaultValue.Invoke();
						if (string.IsNullOrWhiteSpace(val))
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

		public TBuilder SetIfNotEmpty(TKey key, string? value, out bool set)
		{
			set = !string.IsNullOrEmpty(value);

			if (!string.IsNullOrEmpty(value))
				_dict[key] = value;

			return _builder;
		}

		public TBuilder SetIfNotEmpty(TKey key, string? value, Func<string>? defaultValue, out bool set)
		{
			string val;
			if (string.IsNullOrEmpty(value))
			{
				if (defaultValue == null)
				{
					set = false;
					return _builder;
				}
				else
				{
					val = defaultValue.Invoke();
					if (string.IsNullOrEmpty(val))
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

		public TBuilder SetIfNotEmpty(TKey key, bool condition, Func<string>? value, Func<string>? defaultValue, out bool set)
		{
			if (!condition)
			{
				set = false;
				return _builder;
			}

			string val;
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
					if (string.IsNullOrEmpty(val))
					{
						set = false;
						return _builder;
					}
				}
			}
			else
			{
				val = value.Invoke();
				if (string.IsNullOrEmpty(val))
				{
					if (defaultValue == null)
					{
						set = false;
						return _builder;
					}
					else
					{
						val = defaultValue.Invoke();
						if (string.IsNullOrEmpty(val))
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

		public TBuilder SetIfNotWhiteSpace(TKey key, string? value, out bool set)
		{
			set = !string.IsNullOrWhiteSpace(value);

			if (!string.IsNullOrWhiteSpace(value))
				_dict[key] = value;

			return _builder;
		}

		public TBuilder SetIfNotWhiteSpace(TKey key, string? value, Func<string>? defaultValue, out bool set)
		{
			string val;
			if (string.IsNullOrWhiteSpace(value))
			{
				if (defaultValue == null)
				{
					set = false;
					return _builder;
				}
				else
				{
					val = defaultValue.Invoke();
					if (string.IsNullOrWhiteSpace(val))
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

		public TBuilder SetIfNotWhiteSpace(TKey key, bool condition, Func<string>? value, Func<string>? defaultValue, out bool set)
		{
			if (!condition)
			{
				set = false;
				return _builder;
			}

			string val;
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
					if (string.IsNullOrWhiteSpace(val))
					{
						set = false;
						return _builder;
					}
				}
			}
			else
			{
				val = value.Invoke();
				if (string.IsNullOrWhiteSpace(val))
				{
					if (defaultValue == null)
					{
						set = false;
						return _builder;
					}
					else
					{
						val = defaultValue.Invoke();
						if (string.IsNullOrWhiteSpace(val))
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
	public class DictionaryBuilder<TKey> : DictionaryBuilderBase<DictionaryBuilder<TKey>, TKey>
		where TKey : notnull
	{
		public DictionaryBuilder()
			: this(new Dictionary<TKey, object?>())
		{
		}

		public DictionaryBuilder(Dictionary<TKey, object?> dictionary)
			: base(dictionary)
		{
		}

		public static implicit operator Dictionary<TKey, object?>?(DictionaryBuilder<TKey> builder)
		{
			if (builder == null)
				return null;

			return builder._dict;
		}

		public static implicit operator DictionaryBuilder<TKey>?(Dictionary<TKey, object?> dictionary)
		{
			if (dictionary == null)
				return null;

			return new DictionaryBuilder<TKey>(dictionary);
		}
	}
}
