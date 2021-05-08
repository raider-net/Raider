using System;
using System.Collections.Generic;

namespace Raider.Validation
{
	public interface IValidationManager
	{
		Dictionary<Type, IValidationDescriptorBuilder>? GetValidationDescriptorBuilderFor<T>();

		Dictionary<Type, IValidationDescriptorBuilder>? GetValidationDescriptorBuilderFor(Type objectType);

		IValidationDescriptorBuilder? GetValidationDescriptorBuilderFor<T, TCommand>();

		IValidationDescriptorBuilder? GetValidationDescriptorBuilderFor(Type objectType, Type commandType);

		IValidationDescriptor? GetValidationDescriptorFor(Type objectType, Type commandType, IServiceProvider serviceProvider);
	}
}
