using System;

namespace Raider.Validation
{
	public interface IValidationDescriptorBuilder
	{
		Type ObjectType { get; }

		IValidationDescriptor ToDescriptor(IServiceProvider serviceProvider);

		IValidationDescriptor ToDescriptor(IServiceProvider serviceProvider, object? state = null);
	}
}
