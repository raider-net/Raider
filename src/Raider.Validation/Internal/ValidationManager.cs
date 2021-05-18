using Raider.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Validation
{
	internal class ValidationManager : IValidationManager
	{
		private readonly Dictionary<Type, Dictionary<Type, IValidationDescriptorBuilder>> _descriptorsRegister = new Dictionary<Type, Dictionary<Type, IValidationDescriptorBuilder>>(); //Dictionary<TDto, Dictionary<TCommand, IValidationDescriptorBuilder>>

		public Dictionary<Type, IValidationDescriptorBuilder>? GetValidationDescriptorBuilderFor<T>()
			=> GetValidationDescriptorBuilderFor(typeof(T));

		public Dictionary<Type, IValidationDescriptorBuilder>? GetValidationDescriptorBuilderFor(Type objectType)
		{
			if (!_descriptorsRegister.TryGetValue(objectType, out Dictionary<Type, IValidationDescriptorBuilder>? commandDescriptorBuilders))
				return null;

			return commandDescriptorBuilders.ToDictionary(k => k.Key, v => v.Value);
		}

		public IValidationDescriptorBuilder? GetValidationDescriptorBuilderFor<T, TCommand>()
			=> GetValidationDescriptorBuilderFor(typeof(T), typeof(TCommand));

		public IValidationDescriptorBuilder? GetValidationDescriptorBuilderFor(Type objectType, Type commandType)
		{
			if (!_descriptorsRegister.TryGetValue(objectType, out Dictionary<Type, IValidationDescriptorBuilder>? commandValidationDescriptorBuilders))
				return null;

			if (!commandValidationDescriptorBuilders.TryGetValue(commandType, out IValidationDescriptorBuilder? builder))
				return null;

			return builder;
		}

		public bool RegisterValidationDescriptorFor<T, TCommand>(IValidationDescriptorBuilder builder)
			=> RegisterValidationDescriptorFor(typeof(T), typeof(TCommand), builder);

		public bool RegisterValidationDescriptorFor(Type objectType, Type commandType, IValidationDescriptorBuilder builder)
		{
			if (builder == null)
				throw new ArgumentNullException(nameof(builder));

			if (!_descriptorsRegister.TryGetValue(objectType, out Dictionary<Type, IValidationDescriptorBuilder>? commandValidationDescriptorBuilders))
			{
				commandValidationDescriptorBuilders = new Dictionary<Type, IValidationDescriptorBuilder>();
				_descriptorsRegister.Add(objectType, commandValidationDescriptorBuilders);
			}

			commandValidationDescriptorBuilders[commandType] = builder;

			return true;
		}

		public IValidationDescriptor? GetValidationDescriptorFor(Type objectType, Type commandType, IServiceProvider serviceProvider, object? state = null)
		{
			var validationDescriptorBuilder = GetValidationDescriptorBuilderFor(objectType, commandType);
			if (validationDescriptorBuilder == null)
				throw new InvalidOperationException($"No {nameof(IValidationDescriptorBuilder)} found for <{objectType?.FullName ?? "NULL"}, {commandType?.FullName ?? "NULL"}>");

			IValidationDescriptor? descriptor = null;
			try
			{
				descriptor = validationDescriptorBuilder.ToDescriptor(serviceProvider);
			}
			catch (Exception ex)
			{
				throw new ConfigurationException($"Cannot create {nameof(IValidationDescriptor)} for <{objectType?.FullName ?? "NULL"}, {commandType?.FullName ?? "NULL"}>", ex);
			}

			if (descriptor == null)
				throw new InvalidOperationException($"{nameof(validationDescriptorBuilder.ToDescriptor)} for <{objectType?.FullName ?? "NULL"}, {commandType?.FullName ?? "NULL"}> returns null.");

			return descriptor;
		}
	}
}
