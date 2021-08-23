using System;

namespace Raider.Reflection
{
	public class ObjectWrapper<T>
	{
		private readonly TypeWrapper<T> _typeManager;
		private T? _currentInstance;

		public object? this[string propertyFieldName]
		{
			get { return GetValueInternal(propertyFieldName); }
			set { SetValueInternal(propertyFieldName, value); }
		}

		public ObjectWrapper()
			: this(null, default)
		{
		}

		public ObjectWrapper(T? instance)
			: this(null, instance)
		{
		}

		internal ObjectWrapper(TypeWrapper<T>? typeManager)
			: this(typeManager, default)
		{
		}

		internal ObjectWrapper(TypeWrapper<T>? typeManager, T? instance)
		{
			_typeManager = typeManager ?? TypeWrapper<T>.Create();
			_currentInstance = instance;
		}

		public ObjectWrapper<T> SetInstance(T? instance)
		{
			_currentInstance = instance;
			return this;
		}

		public object? GetValue(string memberName)
		{
			if (string.IsNullOrWhiteSpace(memberName))
				throw new ArgumentNullException(nameof(memberName));

			if (_currentInstance == null)
				throw new InvalidOperationException("No instance was set.");

			if (_typeManager.Getters.TryGetValue(memberName, out Func<T?, object?>? getter))
				return getter(_currentInstance);

			throw new InvalidOperationException($"No getter for {memberName} was found.");
		}

		public object? GetStaticValue(string memberName)
		{
			if (string.IsNullOrWhiteSpace(memberName))
				throw new ArgumentNullException(nameof(memberName));

			if (_typeManager.StaticGetters.TryGetValue(memberName, out Func<T?, object?>? staticGetter))
				return staticGetter(default);

			throw new InvalidOperationException($"No static getter for {memberName} was found.");
		}

		private object? GetValueInternal(string memberName)
		{
			if (string.IsNullOrWhiteSpace(memberName))
				throw new ArgumentNullException(nameof(memberName));

			if (_typeManager.StaticGetters.TryGetValue(memberName, out Func<T?, object?>? staticGetter))
				return staticGetter(default);

			if (_currentInstance == null)
				throw new InvalidOperationException("No instance was set.");

			if (_typeManager.Getters.TryGetValue(memberName, out Func<T?, object?>? getter))
				return getter(_currentInstance);

			throw new InvalidOperationException($"No getter for {memberName} was found.");
		}

		public void SetValue(string memberName, object? value)
		{
			if (string.IsNullOrWhiteSpace(memberName))
				throw new ArgumentNullException(nameof(memberName));

			if (_currentInstance == null)
				throw new InvalidOperationException("No instance was set.");

			if (_typeManager.Setters.TryGetValue(memberName, out Action<T?, object?>? setter))
			{
				setter(_currentInstance, value);
				return;
			}

			throw new InvalidOperationException($"No setter for {memberName} was found.");
		}

		public void SetStaticValue(string memberName, object? value)
		{
			if (string.IsNullOrWhiteSpace(memberName))
				throw new ArgumentNullException(nameof(memberName));

			if (_typeManager.StaticSetters.TryGetValue(memberName, out Action<T?, object?>? staticSetter))
			{
				staticSetter(default, value);
				return;
			}

			throw new InvalidOperationException($"No static setter for {memberName} was found.");
		}

		private void SetValueInternal(string memberName, object? value)
		{
			if (string.IsNullOrWhiteSpace(memberName))
				throw new ArgumentNullException(nameof(memberName));

			if (_typeManager.StaticSetters.TryGetValue(memberName, out Action<T?, object?>? staticSetter))
			{
				staticSetter(default, value);
				return;
			}

			if (_currentInstance == null)
				throw new InvalidOperationException("No instance was set.");

			if (_typeManager.Setters.TryGetValue(memberName, out Action<T?, object?>? setter))
			{
				setter(_currentInstance, value);
				return;
			}

			throw new InvalidOperationException($"No setter for {memberName} was found.");
		}
	}
}
