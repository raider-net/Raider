using System;

namespace Raider.Validation
{
	public abstract class ValidationBuilder<T> : IValidationBuilder<T>, IValidationDescriptorBuilder
	{
		public Type ObjectType { get; } = typeof(T);

		public abstract Validator<T> BuildRules(IServiceProvider serviceProvider, object? state = null);

		public Validator<T> BuildRules(IServiceProvider serviceProvider, Validator<T> parent, object? state = null)
			=> BuildRules(serviceProvider, state).AttachTo(parent);

		public Validator<T> BuildRules(IServiceProvider serviceProvider)
			=> BuildRules(serviceProvider, (object?)null);

		public Validator<T> BuildRules(IServiceProvider serviceProvider, Validator<T> parent)
			=> BuildRules(serviceProvider, (object?)null).AttachTo(parent);

		public IValidationDescriptor ToDescriptor(IServiceProvider serviceProvider, object? state = null)
			=> BuildRules(serviceProvider, state)?.ToDescriptor() ?? throw new InvalidOperationException($"{nameof(BuildRules)}() returns null.");

		public IValidationDescriptor ToDescriptor(IServiceProvider serviceProvider)
			=> BuildRules(serviceProvider, (object?)null)?.ToDescriptor() ?? throw new InvalidOperationException($"{nameof(BuildRules)}() returns null.");
	}
}
