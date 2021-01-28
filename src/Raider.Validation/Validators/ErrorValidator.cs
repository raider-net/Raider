using Raider.Validation.Internal;
using System;

namespace Raider.Validation
{
	internal class ErrorValidator<T> : IValidator
	{
		public bool Conditional { get; } = true;
		public virtual ValidatorType ValidatorType { get; } = ValidatorType.ErrorObject;
		public Func<object, object>? Func { get; }
		public ValidationFrame ValidationFrame { get; }
		public Func<object?, bool> Condition { get; }
		public string ErrorMessage { get; }

		public ErrorValidator(Func<object, object>? func, ValidationFrame validationFrame, Func<object?, bool> condition, string errorMessage)
		{
			Func = func;
			ValidationFrame = validationFrame ?? throw new ArgumentNullException(nameof(validationFrame));
			Condition = condition ?? throw new ArgumentNullException(nameof(condition));
			ErrorMessage = string.IsNullOrWhiteSpace(errorMessage)
				? throw new ArgumentNullException(nameof(errorMessage))
				: errorMessage;
		}

		public IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(ValidationFrame, ValidatorType, Conditional);

		internal virtual ValidationResult Validate(ValidationContext validationContext)
		{
			return Condition.Invoke(validationContext.InstanceToValidate)
				? new ValidationResult().AddFailure(new ValidationFailure(validationContext.ToReadOnlyValidationFrame(ValidationFrame), this, ErrorMessage, ErrorMessage))
				: new ValidationResult();
		}

		ValidationResult IValidator.Validate(object? obj)
			=> throw new NotImplementedException();
	}

	internal class ErrorValidator<T, TProperty> : ErrorValidator<T>, IValidator
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.ErrorProperty;

		public ErrorValidator(Func<object, object>? func, ValidationFrame validationFrame, Func<object?, bool> condition, string errorMessage)
			: base(func, validationFrame, condition, errorMessage)
		{
		}

		internal override ValidationResult Validate(ValidationContext validationContext)
		{
			return Condition.Invoke(validationContext.InstanceToValidate)
				? new ValidationResult().AddFailure(new ValidationFailure(validationContext.ToReadOnlyValidationFrame(ValidationFrame), this, ErrorMessage, ErrorMessage)) //TODO dopln info o property
				: new ValidationResult();
		}
	}
}
