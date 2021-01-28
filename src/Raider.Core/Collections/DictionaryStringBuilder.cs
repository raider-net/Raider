using System;
using System.Collections.Generic;

namespace Raider.Collections
{
	/// <summary>
	/// Represents a builder for dictionary of keys and string values.
	/// </summary>
	/// <typeparam name="TBuilder">The builder.</typeparam>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	public interface IDictionaryStringBuilder<TBuilder, TKey> : IDictionaryClassBuilder<TBuilder, TKey, string>
		where TBuilder : IDictionaryStringBuilder<TBuilder, TKey>
		where TKey : notnull
	{
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

	/// <summary>
	/// Represents a builder for dictionary of keys and string values.
	/// </summary>
	/// <typeparam name="TBuilder">The builder.</typeparam>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	public abstract class DictionaryStringBuilderBase<TBuilder, TKey> : DictionaryClassBuilderBase<TBuilder, TKey, string>, IDictionaryStringBuilder<TBuilder, TKey>
		where TBuilder : DictionaryStringBuilderBase<TBuilder, TKey>
		where TKey : notnull
	{
		protected DictionaryStringBuilderBase(Dictionary<TKey, string> dictionary)
			: base(dictionary)
		{
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

	/// <summary>
	/// Represents a builder for dictionary of keys and string values.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	public class DictionaryStringBuilder<TKey> : DictionaryStringBuilderBase<DictionaryStringBuilder<TKey>, TKey>
		where TKey : notnull
	{
		public DictionaryStringBuilder()
			: this(new Dictionary<TKey, string>())
		{
		}

		public DictionaryStringBuilder(Dictionary<TKey, string> dictionary)
			: base(dictionary)
		{
		}

		public static implicit operator Dictionary<TKey, string>?(DictionaryStringBuilder<TKey> builder)
		{
			if (builder == null)
				return null;

			return builder._dict;
		}

		public static implicit operator DictionaryStringBuilder<TKey>?(Dictionary<TKey, string> dictionary)
		{
			if (dictionary == null)
				return null;

			return new DictionaryStringBuilder<TKey>(dictionary);
		}
	}
}
