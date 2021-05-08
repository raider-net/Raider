using System;

namespace Raider.Validation
{
	public abstract class ValidationBuilder<T> : IValidationBuilder<T>, IValidationDescriptorBuilder
	{
		public Type ObjectType { get; } = typeof(T);

		public abstract Validator<T> BuildRules(IServiceProvider serviceProvider);

		public Validator<T> BuildRules(IServiceProvider serviceProvider, Validator<T> parent)
			=> BuildRules(serviceProvider).AttachTo(parent);

		public IValidationDescriptor ToDescriptor(IServiceProvider serviceProvider)
			=> BuildRules(serviceProvider)?.ToDescriptor() ?? throw new InvalidOperationException($"{nameof(BuildRules)}() returns null.");
	}
}
