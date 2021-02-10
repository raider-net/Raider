using System;

namespace Raider.Validation
{
	public abstract class ValidationBuilder<T> : IValidationBuilder<T>, IValidationDescriptorBuilder
	{
		public abstract Validator<T> BuildRules();

		public Validator<T> BuildRules(Validator<T> parent)
			=> BuildRules().AttachTo(parent);

		public IValidationDescriptor ToDescriptor()
			=> BuildRules()?.ToDescriptor() ?? throw new InvalidOperationException($"{nameof(BuildRules)}() returns null.");
	}
}
