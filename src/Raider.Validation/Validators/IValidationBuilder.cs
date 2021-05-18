using System;

namespace Raider.Validation
{
	public interface IValidationBuilder<T> : IValidationDescriptorBuilder
	{
		Validator<T> BuildRules(IServiceProvider serviceProvider);

		Validator<T> BuildRules(IServiceProvider serviceProvider, Validator<T> parent);

		Validator<T> BuildRules(IServiceProvider serviceProvider, object? state = null);

		Validator<T> BuildRules(IServiceProvider serviceProvider, Validator<T> parent, object? state = null);
	}
}
