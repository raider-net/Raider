using Raider.Validation.Internal;
using System;
using System.Collections.Generic;

namespace Raider.Validation
{
	internal class GreaterThanValidator<T, TProperty> : ValidatorBase<T, TProperty>
	{
		public IComparable? ValueToCompare { get; }

		protected override string DefaultValidationMessage => "Must be greater than '{ValueToCompare}'.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must be greater than '{ValueToCompare}'.";

		public override ValidatorType ValidatorType { get; } = ValidatorType.GreaterThan;

		public GreaterThanValidator(PropertyValidator<T, TProperty> propertyValidator, IComparable? valueToCompare)
			: base(propertyValidator)
		{
			ValueToCompare = valueToCompare;
		}

		private IDictionary<string, object?> GetPlaceholderValues()
			=> new Dictionary<string, object?>
			{
				{ nameof(ValueToCompare), ValueToCompare },
				{ "PropertyName", GetDisplayName() }
			};

		private string GetValidationMessage()
			=> GetFormattedMessage(
					Resources.ValidationKeys.GreaterThan,
					DefaultValidationMessage,
					GetPlaceholderValues());

		private string GetValidationMessageWithProperty()
			=> GetFormattedMessage(
					Resources.ValidationKeys.GreaterThan_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues());

		internal override ValidationFailure? Validate(IPropertyValidationContext context)
			=> context.InstanceToValidate == null || ValueToCompare == null || (context.InstanceToValidate is IComparable value && 0 < value.CompareTo(ValueToCompare))
				? null
				: new ValidationFailure(context.ToReadOnlyValidationFrame(), this, GetValidationMessage(), GetValidationMessageWithProperty());

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(PropertyValidator.ValidationFrame, ValidatorType, Conditional)
			{
				ValueToCompare = ValueToCompare,
			};
	}
}
