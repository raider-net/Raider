using Raider.Validation.Client;
using System;

namespace Raider.Validation
{
	internal class ConditionalValidator<T> : Validator<T>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.ConditionalValidator;
		public Func<object?, bool>? Condition { get; }

		public ConditionalValidator(Func<object, object>? func, ValidationFrame validationFrame, Func<object?, bool>? condition, IClientConditionDefinition? clientConditionDefinition = null)
			: base(func, validationFrame, true, clientConditionDefinition)
		{
			if (condition == null && clientConditionDefinition == null)
				throw new ArgumentNullException(nameof(condition));

			Condition = condition;
		}
	}
}
