using System;

namespace Raider.Validation
{
	public interface IValidationDescriptorBuilder
	{
		Type ObjectType { get; }

		IValidationDescriptor ToDescriptor(IServiceProvider serviceProvider);
	}
}
