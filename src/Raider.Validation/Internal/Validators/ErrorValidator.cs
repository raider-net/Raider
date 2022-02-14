using System;

namespace Raider.Validation
{
	internal class ErrorValidator<T> : Validator<T>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.ErrorObject;
		public Func<object?, bool> Condition { get; }
		public string ErrorMessage { get; }

		public ErrorValidator(Func<object, object>? func, ValidationFrame validationFrame, Func<object?, bool> condition, string errorMessage, Func<object?, string>? detailInfoFunc)
			: base(func, validationFrame, true, null, detailInfoFunc)
		{
			Condition = condition ?? throw new ArgumentNullException(nameof(condition));
			ErrorMessage = string.IsNullOrWhiteSpace(errorMessage)
				? throw new ArgumentNullException(nameof(errorMessage))
				: errorMessage;
		}

		internal override ValidationResult Validate(ValidationContext context)
		{
			return Condition.Invoke(context.InstanceToValidate)
				? new ValidationResult().AddFailure(new ValidationFailure(context.ToReadOnlyValidationFrame(ValidationFrame), ValidatorType, Conditional, null, ErrorMessage, ErrorMessage, DetailInfoFunc?.Invoke(context.InstanceToValidate)))
				: new ValidationResult();
		}
	}

	internal class ErrorValidator<T, TProperty> : PropertyValidator<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.ErrorProperty;
		public Func<object?, bool> Condition { get; }
		public string ErrorMessage { get; }

		public ErrorValidator(Func<object, object>? func, ValidationFrame validationFrame, Func<object?, bool> condition, string errorMessage, Func<object?, string>? detailInfoFunc)
			: base(func, validationFrame, true, null, detailInfoFunc)
		{
			Condition = condition ?? throw new ArgumentNullException(nameof(condition));
			ErrorMessage = string.IsNullOrWhiteSpace(errorMessage)
				? throw new ArgumentNullException(nameof(errorMessage))
				: errorMessage;
		}

		internal override ValidationResult Validate(ValidationContext context)
		{
			return Condition.Invoke(context.InstanceToValidate)
				? new ValidationResult().AddFailure(new ValidationFailure(context.ToReadOnlyValidationFrame(ValidationFrame), ValidatorType, Conditional, null, ErrorMessage, ErrorMessage, DetailInfoFunc?.Invoke(context.InstanceToValidate)))
				: new ValidationResult();
		}
	}
}
