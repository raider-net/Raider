using Raider.Extensions;
using Raider.Validation.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Validation
{
	internal class MultiEqualValidator<T, TProperty> : PropertyValidator<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.MultiEqual;

		protected override string DefaultValidationMessage => "Must be equal to one of the values '[{ValuesToCompare}]'.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must be equal to one of the values '[{ValuesToCompare}]'.";

		public IEnumerable<IComparable?>? ValuesToCompare { get; }
		public IEqualityComparer? Comparer { get; }


		public MultiEqualValidator(PropertyValidator<T, TProperty> propertyValidator, IEnumerable<IComparable?>? valuesToCompare, IEqualityComparer? comparer = null)
			: base(propertyValidator)
		{
			ValuesToCompare = valuesToCompare?.Distinct().ToList();
			Comparer = comparer;
		}

		private IDictionary<string, object?> GetPlaceholderValues()
			=> new Dictionary<string, object?>
			{
				{ nameof(ValuesToCompare), ValuesToCompare },
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
				return (ValuesToCompare == null || ValuesToCompare.Any(x => x == null))
					? null
					: new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty()));

			if (ValuesToCompare == null)
				return new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty()));

			if (Comparer == null)
				return ValuesToCompare.Any(x => Equals(x, context.InstanceToValidate))
					? null
					: new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty()));
			else
				return ValuesToCompare.Any(x => Comparer.Equals(x, context.InstanceToValidate))
					? null
					: new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty()));
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(typeof(T), ValidationFrame, ValidatorType, GetType().ToFriendlyFullName(), Conditional, ClientConditionDefinition)
			{
				ValuesToCompare = ValuesToCompare,
				Comparer = Comparer
			};
	}

	internal class MultiNotEqualValidator<T, TProperty> : PropertyValidator<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.MultiNotEqual;

		protected override string DefaultValidationMessage => "Must not be equal to any of the values '[{ValuesToCompare}]'.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must not be equal to any of the values '[{ValuesToCompare}]'.";

		public IEnumerable<IComparable?>? ValuesToCompare { get; }
		public IEqualityComparer? Comparer { get; }


		public MultiNotEqualValidator(PropertyValidator<T, TProperty> propertyValidator, IEnumerable<IComparable?>? valuesToCompare, IEqualityComparer? comparer = null)
			: base(propertyValidator)
		{
			ValuesToCompare = valuesToCompare?.Distinct().ToList();
			Comparer = comparer;
		}

		private IDictionary<string, object?> GetPlaceholderValues()
			=> new Dictionary<string, object?>
			{
				{ nameof(ValuesToCompare), ValuesToCompare },
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
				return (ValuesToCompare == null || ValuesToCompare.Any(x => x == null))
					? new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty()))
					: null;

			if (ValuesToCompare == null)
				return null;

			if (Comparer == null)
				return ValuesToCompare.Any(x => Equals(x, context.InstanceToValidate))
					? new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty()))
					: null;
			else
				return ValuesToCompare.Any(x => Comparer.Equals(x, context.InstanceToValidate))
					? new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty()))
					: null;
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(typeof(T), ValidationFrame, ValidatorType, GetType().ToFriendlyFullName(), Conditional, ClientConditionDefinition)
			{
				ValuesToCompare = ValuesToCompare,
				Comparer = Comparer
			};
	}
}
