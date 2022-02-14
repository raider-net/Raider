using Raider.Extensions;
using Raider.Validation.Internal;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Raider.Validation
{
	internal class EqualValidator<T, TProperty> : PropertyValidator<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.Equal;

		protected override string DefaultValidationMessage => "Must be equal to '{ValueToCompare}'.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must be equal to '{ValueToCompare}'.";

		public IComparable? ValueToCompare { get; }
		public IEqualityComparer? Comparer { get; }


		public EqualValidator(PropertyValidator<T, TProperty> propertyValidator, IComparable? valueToCompare, IEqualityComparer? comparer = null)
			: base(propertyValidator)
		{
			ValueToCompare = valueToCompare;
			Comparer = comparer;
		}

		private IDictionary<string, object?> GetPlaceholderValues()
			=> new Dictionary<string, object?>
			{
				{ nameof(ValueToCompare), ValueToCompare },
				{ "PropertyName", GetDisplayName() }
			};

		private string GetValidationMessage()
			=> GetFormattedMessage(
					Resources.ValidationKeys.Equal,
					DefaultValidationMessage,
					GetPlaceholderValues());

		private string GetValidationMessageWithProperty()
			=> GetFormattedMessage(
					Resources.ValidationKeys.Equal_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues());

		internal override ValidationResult? Validate(ValidationContext context)
		{
			if (context.InstanceToValidate == null)
				return ValueToCompare == null
					? null
					: new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty(), DetailInfoFunc?.Invoke(context.InstanceToValidate)));

			if (ValueToCompare == null)
				return new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty(), DetailInfoFunc?.Invoke(context.InstanceToValidate)));

			if (Comparer == null)
				return Equals(ValueToCompare, context.InstanceToValidate)
					? null
					: new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty(), DetailInfoFunc?.Invoke(context.InstanceToValidate)));
			else
				return Comparer.Equals(ValueToCompare, context.InstanceToValidate)
					? null
					: new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty(), DetailInfoFunc?.Invoke(context.InstanceToValidate)));
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(typeof(T), ValidationFrame, ValidatorType, GetType().ToFriendlyFullName(), Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty())
			{
				ValueToCompare = ValueToCompare,
				Comparer = Comparer
			};
	}

	internal class NotEqualValidator<T, TProperty> : PropertyValidator<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.NotEqual;

		protected override string DefaultValidationMessage => "Must not be equal to '{ValueToCompare}'.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must not be equal to '{ValueToCompare}'.";

		public IComparable? ValueToCompare { get; }
		public IEqualityComparer? Comparer { get; }


		public NotEqualValidator(PropertyValidator<T, TProperty> propertyValidator, IComparable? valueToCompare, IEqualityComparer? comparer = null)
			: base(propertyValidator)
		{
			ValueToCompare = valueToCompare;
			Comparer = comparer;
		}

		private IDictionary<string, object?> GetPlaceholderValues()
			=> new Dictionary<string, object?>
			{
				{ nameof(ValueToCompare), ValueToCompare },
				{ "PropertyName", GetDisplayName() }
			};

		private string GetValidationMessage()
			=> GetFormattedMessage(
					Resources.ValidationKeys.NotEqual,
					DefaultValidationMessage,
					GetPlaceholderValues());

		private string GetValidationMessageWithProperty()
			=> GetFormattedMessage(
					Resources.ValidationKeys.NotEqual_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues());

		internal override ValidationResult? Validate(ValidationContext context)
		{
			if (context.InstanceToValidate == null)
				return ValueToCompare == null
					? new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty(), DetailInfoFunc?.Invoke(context.InstanceToValidate)))
					: null;

			if (ValueToCompare == null)
				return null;

			if (Comparer == null)
				return Equals(ValueToCompare, context.InstanceToValidate)
					? new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty(), DetailInfoFunc?.Invoke(context.InstanceToValidate)))
					: null;
			else
				return Comparer.Equals(ValueToCompare, context.InstanceToValidate)
					? new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty(), DetailInfoFunc?.Invoke(context.InstanceToValidate)))
					: null;
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(typeof(T), ValidationFrame, ValidatorType, GetType().ToFriendlyFullName(), Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty())
			{
				ValueToCompare = ValueToCompare,
				Comparer = Comparer
			};
	}
}
