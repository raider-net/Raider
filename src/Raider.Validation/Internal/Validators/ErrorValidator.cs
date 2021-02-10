using System;

namespace Raider.Validation
{
	internal class ErrorValidator<T> : Validator<T>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.ErrorObject;
		public Func<object?, bool> Condition { get; }
		public string ErrorMessage { get; }

		public ErrorValidator(Func<object, object>? func, ValidationFrame validationFrame, Func<object?, bool> condition, string errorMessage)
			: base(func, validationFrame, true)
		{
			Condition = condition ?? throw new ArgumentNullException(nameof(condition));
			ErrorMessage = string.IsNullOrWhiteSpace(errorMessage)
				? throw new ArgumentNullException(nameof(errorMessage))
				: errorMessage;
		}

		internal override ValidationResult Validate(ValidationContext validationContext)
		{
			return Condition.Invoke(validationContext.InstanceToValidate)
				? new ValidationResult().AddFailure(new ValidationFailure(validationContext.ToReadOnlyValidationFrame(ValidationFrame), ValidatorType, Conditional, null, ErrorMessage, ErrorMessage))
				: new ValidationResult();
		}
	}

	internal class ErrorValidator<T, TProperty> : PropertyValidator<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.ErrorProperty;
		public Func<object?, bool> Condition { get; }
		public string ErrorMessage { get; }

		public ErrorValidator(Func<object, object>? func, ValidationFrame validationFrame, Func<object?, bool> condition, string errorMessage)
			: base(func, validationFrame, true, null)
		{
			Condition = condition ?? throw new ArgumentNullException(nameof(condition));
			ErrorMessage = string.IsNullOrWhiteSpace(errorMessage)
				? throw new ArgumentNullException(nameof(errorMessage))
				: errorMessage;
		}

		internal override ValidationResult Validate(ValidationContext validationContext)
		{
			return Condition.Invoke(validationContext.InstanceToValidate)
				? new ValidationResult().AddFailure(new ValidationFailure(validationContext.ToReadOnlyValidationFrame(ValidationFrame), ValidatorType, Conditional, null, ErrorMessage, ErrorMessage))
				: new ValidationResult();
		}
	}
}
