using Raider.Extensions;
using Raider.Validation.Internal;
using System;
using System.Collections.Generic;

namespace Raider.Validation
{
	internal class ExclusiveBetweenValidator<T, TProperty> : PropertyValidator<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.ExclusiveBetween;

		protected override string DefaultValidationMessage => "Must be between {From} and {To} (exclusive).";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must be between {From} and {To} (exclusive).";

		public IComparable? From { get; }
		public IComparable? To { get; }

		public ExclusiveBetweenValidator(PropertyValidator<T, TProperty> propertyValidator, IComparable? from, IComparable? to)
			: base(propertyValidator)
		{
			From = from;
			To = to;
		}

		private IDictionary<string, object?> GetPlaceholderValues()
			=> new Dictionary<string, object?>
			{
				{ nameof(From), From },
				{ nameof(To), To },
				{ "PropertyName", GetDisplayName() }
			};

		private string GetValidationMessage()
			=> GetFormattedMessage(
					Resources.ValidationKeys.ExclusiveBetween,
					DefaultValidationMessage,
					GetPlaceholderValues());

		private string GetValidationMessageWithProperty()
			=> GetFormattedMessage(
					Resources.ValidationKeys.ExclusiveBetween_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues());

		internal override ValidationResult? Validate(ValidationContext context)
		{
			if (context.InstanceToValidate == null || From == null || To == null)
				return null;

			if (context.InstanceToValidate is IComparable value)
			{
				if (0 < value.CompareTo(From) && value.CompareTo(To) < 0)
					return null;
				else
					return new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty()));
			}

			throw new InvalidOperationException($"{nameof(context.InstanceToValidate)} must implement {nameof(IComparable)}.");
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(typeof(T), ValidationFrame, ValidatorType, GetType().ToFriendlyFullName(), Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty())
			{
				From = From,
				To = To
			};
	}
}
