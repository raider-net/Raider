using System;

namespace Raider.Validation
{
	internal class ConditionalValidator<T> : Validator<T>, IValidator
	{
		public Func<object?, bool> Condition { get; }
		public override ValidatorType ValidatorType { get; } = ValidatorType.NONE;

		public ConditionalValidator(Func<object, object>? func, ValidationFrame validationFrame, Func<object?, bool> condition)
			: base(func, validationFrame, true)
		{
			Condition = condition ?? throw new ArgumentNullException(nameof(condition));
		}
	}
}
