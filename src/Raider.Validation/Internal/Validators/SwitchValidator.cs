using Raider.Extensions;
using Raider.Validation.Internal;
using System;

namespace Raider.Validation
{
	internal class SwitchValidator<T> : Validator<T>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.SwitchValidator;
		public Func<object?, bool> Condition { get; }
		public Validator<T> True { get; }
		public Validator<T> False { get; }

		public SwitchValidator(Func<object, object>? func, ValidationFrame validationFrame, Func<object?, bool> condition, Func<object?, string>? detailInfoFunc)
			: base(func, validationFrame, true, null, detailInfoFunc)
		{
			Condition = condition ?? throw new ArgumentNullException(nameof(condition));
			True = new Validator<T>(Func, ValidationFrame, true, null,detailInfoFunc);
			False = new Validator<T>(Func, ValidationFrame, true, null, detailInfoFunc);
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(typeof(T), ValidationFrame, ValidatorType, GetType().ToFriendlyFullName(), Conditional, ClientConditionDefinition, null, null)
				.AddValidators(new[] { True, False });
	}
}
