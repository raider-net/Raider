using System;
using System.Collections.Generic;

namespace Raider.Validation
{
	public interface IValidationManager
	{
		Dictionary<Type, IValidationDescriptor>? GetValidationDescriptorFor<T>();

		Dictionary<Type, IValidationDescriptor>? GetValidationDescriptorFor(Type objectType);

		IValidationDescriptor? GetValidationDescriptorFor<T, TCommand>();

		IValidationDescriptor? GetValidationDescriptorFor(Type objectType, Type commandType);
	}
}
