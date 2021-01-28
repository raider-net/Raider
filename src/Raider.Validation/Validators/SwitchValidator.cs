using Raider.Validation.Internal;
using System;

namespace Raider.Validation
{
	internal class SwitchValidator<T> : IValidator
	{
		public bool Conditional { get; } = true;
		public ValidatorType ValidatorType { get; } = ValidatorType.NONE;
		public Func<object, object>? Func { get; }
		public ValidationFrame ValidationFrame { get; }

		public Func<object?, bool> Condition { get; }
		public Validator<T> True { get; }
		public Validator<T> False { get; }

		public SwitchValidator(Func<object, object>? func, ValidationFrame validationFrame, Func<object?, bool> condition)
		{
			Func = func;
			ValidationFrame = validationFrame ?? throw new ArgumentNullException(nameof(validationFrame));
			Condition = condition ?? throw new ArgumentNullException(nameof(condition));
			True = new Validator<T>(Func, ValidationFrame, true);
			False = new Validator<T>(Func, ValidationFrame, true);
		}

		public IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(ValidationFrame, ValidatorType, Conditional)
				.AddValidators(new[] { True, False });

		ValidationResult IValidator.Validate(object? obj)
			=> throw new NotImplementedException();
	}
}
