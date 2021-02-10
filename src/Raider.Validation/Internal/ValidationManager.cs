using System;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Validation
{
	internal class ValidationManager : IValidationManager
	{
		private readonly Dictionary<Type, Dictionary<Type, IValidationDescriptor>> _descriptorsRegister = new Dictionary<Type, Dictionary<Type, IValidationDescriptor>>(); //Dictionary<TDto, Dictionary<TCommand, IValidationDescriptor>>

		public Dictionary<Type, IValidationDescriptor>? GetValidationDescriptorFor<T>()
			=> GetValidationDescriptorFor(typeof(T));

		public Dictionary<Type, IValidationDescriptor>? GetValidationDescriptorFor(Type objectType)
		{
			if (!_descriptorsRegister.TryGetValue(objectType, out Dictionary<Type, IValidationDescriptor>? commandDescriptors))
				return null;

			return commandDescriptors.ToDictionary(k => k.Key, v => v.Value);
		}

		public IValidationDescriptor? GetValidationDescriptorFor<T, TCommand>()
			=> GetValidationDescriptorFor(typeof(T), typeof(TCommand));

		public IValidationDescriptor? GetValidationDescriptorFor(Type objectType, Type commandType)
		{
			if (!_descriptorsRegister.TryGetValue(objectType, out Dictionary<Type, IValidationDescriptor>? commandcommandDescriptors))
				return null;

			if (!commandcommandDescriptors.TryGetValue(commandType, out IValidationDescriptor? descriptor))
				return null;

			return descriptor;
		}

		public bool RegisterValidationDescriptorFor<T, TCommand>(IValidationDescriptor descriptor)
			=> RegisterValidationDescriptorFor(typeof(T), typeof(TCommand), descriptor);

		public bool RegisterValidationDescriptorFor(Type objectType, Type commandType, IValidationDescriptor descriptor)
		{
			if (descriptor == null)
				throw new ArgumentNullException(nameof(descriptor));

			if (!_descriptorsRegister.TryGetValue(objectType, out Dictionary<Type, IValidationDescriptor>? commandcommandDescriptors))
			{
				commandcommandDescriptors = new Dictionary<Type, IValidationDescriptor>();
				_descriptorsRegister.Add(objectType, commandcommandDescriptors);
			}

			commandcommandDescriptors[commandType] = descriptor;

			return true;
		}
	}
}
