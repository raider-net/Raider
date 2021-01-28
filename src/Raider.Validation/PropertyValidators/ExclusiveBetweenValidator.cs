using Raider.Validation.Internal;
using System;
using System.Collections.Generic;

namespace Raider.Validation
{
	internal class ExclusiveBetweenValidator<T, TProperty> : ValidatorBase<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.ExclusiveBetween;

		protected override string DefaultValidationMessage => "Must be between {From} and {To} (exclusive). You entered {Value}.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must be between {From} and {To} (exclusive). You entered {Value}.";

		public IComparable? From { get; }
		public IComparable? To { get; }

		public ExclusiveBetweenValidator(PropertyValidator<T, TProperty> propertyValidator, IComparable? from, IComparable? to)
			: base(propertyValidator)
		{
			From = from;
			To = to;
		}

		private IDictionary<string, object?> GetPlaceholderValues(IComparable? instanceToValidate)
			=> new Dictionary<string, object?>
			{
				{ nameof(From), From },
				{ nameof(To), To },
				{ "Value", instanceToValidate },
				{ "PropertyName", GetDisplayName() }
			};

		private string GetValidationMessage(IComparable instanceToValidate)
			=> GetFormattedMessage(
					Resources.ValidationKeys.ExclusiveBetween,
					DefaultValidationMessage,
					GetPlaceholderValues(instanceToValidate));

		private string GetValidationMessageWithProperty(IComparable instanceToValidate)
			=> GetFormattedMessage(
					Resources.ValidationKeys.ExclusiveBetween_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues(instanceToValidate));

		internal override ValidationFailure? Validate(IPropertyValidationContext context)
		{
			if (context.InstanceToValidate == null || From == null || To == null)
				return null;

			if (context.InstanceToValidate is IComparable value)
			{
				if (0 < value.CompareTo(From) && value.CompareTo(To) < 0)
					return null;
				else
					return new ValidationFailure(context.ToReadOnlyValidationFrame(), this, GetValidationMessage(value), GetValidationMessageWithProperty(value));
			}

			throw new InvalidOperationException($"{nameof(context.InstanceToValidate)} must implement {nameof(IComparable)}.");
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(PropertyValidator.ValidationFrame, ValidatorType, Conditional)
			{
				From = From,
				To = To
			};
	}
}
