using Raider.Extensions;
using Raider.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Raider.Serializer
{
	public static class FormDataSerializer
	{
		public static FormDataSerializer<T> Serialize<T>(T obj)
			=> new FormDataSerializer<T>(obj);

		public static FormDataSerializer<T> Serialize<T>(T obj, string? prefix, bool writeEmptyValues = false)
			=> new FormDataSerializer<T>(obj, prefix, writeEmptyValues);
	}

	public class FormDataSerializer<T>
	{
		private readonly T _obj;
		private readonly string? _prefix;
		private ObjectWrapper<T>? _objectWrapper;
		private readonly List<KeyValuePair<string, string>> _formData;
		private readonly bool _writeEmptyValues;

		public FormDataSerializer(T obj)
			: this(obj, null, false) { }

		public FormDataSerializer(T obj, string? prefix, bool writeEmptyValues = false)
		{
			_obj = obj;
			_prefix = prefix;
			_writeEmptyValues = writeEmptyValues;
			_formData = new List<KeyValuePair<string, string>>();
		}

		private ObjectWrapper<T> GetObjectWrapper()
		{
			if (_objectWrapper == null)
				_objectWrapper = new ObjectWrapper<T>(_obj);

			return _objectWrapper;
		}

		public FormDataSerializer<T> Navigation<E>(Expression<Func<T, E>> expression, Action<FormDataSerializer<E>> serializer, bool? writeEmptyValues = null)
		{
			if (serializer == null)
				return this;

			var name = expression.GetMemberName();
			var value = GetObjectWrapper()[name];

			if (value == null)
				return this;

			return Navigation(name, (E)value, serializer, writeEmptyValues);
		}

		public FormDataSerializer<T> Navigation<E>(Expression<Func<T, E>> expression, E navigation, Action<FormDataSerializer<E>> serializer, bool? writeEmptyValues = null)
		{
			if (serializer == null || navigation == null)
				return this;

			var name = expression.GetMemberName();
			return Navigation(name, navigation, serializer, writeEmptyValues);
		}

		public FormDataSerializer<T> Navigation<E>(string name, E navigation, Action<FormDataSerializer<E>> serializer, bool? writeEmptyValues = null)
		{
			if (serializer == null || navigation == null)
				return this;

			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			var formDataSerializer = new FormDataSerializer<E>(navigation, name, writeEmptyValues ?? _writeEmptyValues);
			serializer(formDataSerializer);

			return this;
		}

		public FormDataSerializer<T> Collection<E>(Expression<Func<T, IEnumerable<E>>> expression, Action<FormDataSerializer<E>> serializer, bool? writeEmptyValues = null)
		{
			if (serializer == null)
				return this;

			var name = expression.GetMemberName();
			var value = GetObjectWrapper()[name];

			if (value == null)
				return this;

			return Collection(name, (IEnumerable<E>)value, serializer, writeEmptyValues);
		}

		public FormDataSerializer<T> Collection<E>(Expression<Func<T, IEnumerable<E>>> expression, IEnumerable<E> enumerable, Action<FormDataSerializer<E>> serializer, bool? writeEmptyValues = null)
		{
			if (serializer == null || enumerable == null || !enumerable.Any())
				return this;

			var name = expression.GetMemberName();
			return Collection(name, enumerable, serializer, writeEmptyValues);
		}

		public FormDataSerializer<T> Collection<E>(string name, IEnumerable<E> enumerable, Action<FormDataSerializer<E>> serializer, bool? writeEmptyValues = null)
		{
			if (serializer == null || enumerable == null || !enumerable.Any())
				return this;

			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			var index = 0;
			foreach (var item in enumerable)
			{
				var originalCount = _formData.Count;
				var formDataSerializer = new FormDataSerializer<E>(item, $"{name}[{index}]", writeEmptyValues ?? _writeEmptyValues);
				serializer(formDataSerializer);
				if (_formData.Count < originalCount)
					index++;
			}

			return this;
		}

		public FormDataSerializer<T> Attach(string name, IFormDataSerializable? formDataSerializable)
		{
			if (formDataSerializable == null)
				return this;

			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			var navigationFormData = formDataSerializable.Serialize(name);

			if (navigationFormData != null && 0 < navigationFormData.Count)
				_formData.AddRange(navigationFormData);

			return this;
		}

		public FormDataSerializer<T> Attach(string name, IEnumerable<IFormDataSerializable>? formDataSerializables)
		{
			if (formDataSerializables == null || !formDataSerializables.Any())
				return this;

			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			var index = 0;
			foreach (var formDataSerializable in formDataSerializables)
			{
				var navigationFormData = formDataSerializable.Serialize($"{name}[{index}]");
				if (navigationFormData != null && 0 < navigationFormData.Count)
				{
					_formData.AddRange(navigationFormData);
					index++;
				}
			}

			return this;
		}

		public FormDataSerializer<T> Property<V>(Expression<Func<T, V>> expression)
		{
			if (_obj == null)
				return this;

			var name = expression.GetMemberName();
			var value = GetObjectWrapper()[name];

			if (_writeEmptyValues || value != null)
				_formData.Add(new KeyValuePair<string, string>(string.IsNullOrWhiteSpace(_prefix) ? name : $"{_prefix}.{name}", value?.ToString() ?? string.Empty));

			return this;
		}

		public FormDataSerializer<T> Property<V>(Expression<Func<T, V>> expression, V? value)
		{
			if (_obj == null)
				return this;

			var name = expression.GetMemberName();

			if (_writeEmptyValues || value != null)
				_formData.Add(new KeyValuePair<string, string>(string.IsNullOrWhiteSpace(_prefix) ? name : $"{_prefix}.{name}", value?.ToString() ?? string.Empty));

			return this;
		}

		public FormDataSerializer<T> Property<V>(Expression<Func<T, V>> expression, string? value)
		{
			if (_obj == null)
				return this;

			var name = expression.GetMemberName();

			if (_writeEmptyValues || value != null)
				_formData.Add(new KeyValuePair<string, string>(string.IsNullOrWhiteSpace(_prefix) ? name : $"{_prefix}.{name}", value ?? string.Empty));

			return this;
		}

		public FormDataSerializer<T> Property<V>(string name, V? value)
		{
			if (_obj == null)
				return this;

			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			if (_writeEmptyValues || value != null)
				_formData.Add(new KeyValuePair<string, string>(string.IsNullOrWhiteSpace(_prefix) ? name : $"{_prefix}.{name}", value?.ToString() ?? string.Empty));

			return this;
		}

		public FormDataSerializer<T> Property(string name, string? value)
		{
			if (_obj == null)
				return this;

			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			if (_writeEmptyValues || value != null)
				_formData.Add(new KeyValuePair<string, string>(string.IsNullOrWhiteSpace(_prefix) ? name : $"{_prefix}.{name}", value ?? string.Empty));

			return this;
		}

		public FormDataSerializer<T> AddRange(List<KeyValuePair<string, string>> formData)
		{
			if (formData == null || formData.Count == 0)
				return this;

			_formData.AddRange(formData);
			return this;
		}

		public List<KeyValuePair<string, string>> ToFormData()
			=> _formData.ToList();
	}
}
