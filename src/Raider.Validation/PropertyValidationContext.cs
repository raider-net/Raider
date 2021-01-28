using Raider.Validation.Internal;
using System;

namespace Raider.Validation
{
	public interface IPropertyValidationContext
	{
		IValidationFrame ValidationFrame { get; }
		ValidationContext ValidationContext { get; }
		object? InstanceToValidate { get; }

		IValidationFrame ToReadOnlyValidationFrame();
	}

	public class RTPropertyValidationContext<T, TProperty> : IPropertyValidationContext
		where TProperty : class
	{
		public IValidationFrame ValidationFrame { get; }
		public ValidationContext ValidationContext { get; }
		public object? InstanceToValidate { get; }

		public RTPropertyValidationContext(IValidationFrame validationFrame, ValidationContext validationContext, object? instanceToValidate)
		{
			ValidationFrame = validationFrame ?? throw new ArgumentNullException(nameof(validationFrame));
			ValidationContext = validationContext ?? throw new ArgumentNullException(nameof(validationContext));
			InstanceToValidate = instanceToValidate;

			if (string.IsNullOrWhiteSpace(ValidationFrame.PropertyName))
				throw new InvalidOperationException($"{nameof(ValidationFrame)}.{nameof(ValidationFrame.PropertyName)} == null");
		}

		public IValidationFrame ToReadOnlyValidationFrame()
			=> ValidationContext.ToReadOnlyValidationFrame(ValidationFrame);
	}

	public class SPropertyValidationContext<T, TProperty> : IPropertyValidationContext
		where TProperty : struct
	{
		public IValidationFrame ValidationFrame { get; }
		public ValidationContext ValidationContext { get; }
		public object? InstanceToValidate { get; }

		public SPropertyValidationContext(IValidationFrame validationFrame, ValidationContext validationContext, object? instanceToValidate)
		{
			ValidationFrame = validationFrame ?? throw new ArgumentNullException(nameof(validationFrame));
			ValidationContext = validationContext ?? throw new ArgumentNullException(nameof(validationContext));
			InstanceToValidate = instanceToValidate;

			if (string.IsNullOrWhiteSpace(ValidationFrame.PropertyName))
				throw new InvalidOperationException($"{nameof(ValidationFrame)}.{nameof(ValidationFrame.PropertyName)} == null");
		}

		public IValidationFrame ToReadOnlyValidationFrame()
			=> ValidationContext.ToReadOnlyValidationFrame(ValidationFrame);
	}
}
