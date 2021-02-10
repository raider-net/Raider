using Raider.Extensions;
using Raider.Validation.Internal;
using System;
using System.Collections.Generic;

namespace Raider.Validation
{
	internal class LengthValidator<T, TProperty> : PropertyValidator<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.Length;

		protected override string DefaultValidationMessage => "Must be between {MinLength} and {MaxLength} characters. You entered {TotalLength} characters.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must be between {MinLength} and {MaxLength} characters. You entered {TotalLength} characters.";

		public int MinLength { get; }
		public int MaxLength { get; }

		public LengthValidator(PropertyValidator<T, TProperty> propertyValidator, int minLength, int maxLength)
			: base(propertyValidator)
		{
			if (maxLength < minLength)
				throw new ArgumentOutOfRangeException(nameof(maxLength), $"{nameof(maxLength)} should be larger than {nameof(minLength)}.");

			MinLength = minLength;
			MaxLength = maxLength;
		}

		private IDictionary<string, object?> GetPlaceholderValues(string? instanceToValidate)
			=> new Dictionary<string, object?>
			{
				{ nameof(MinLength), MinLength },
				{ nameof(MaxLength), MaxLength },
				{ "TotalLength", instanceToValidate?.Length ?? 0 },
				{ "PropertyName", GetDisplayName() }
			};

		private string GetValidationMessage(string? instanceToValidate)
			=> GetFormattedMessage(
					Resources.ValidationKeys.Length,
					DefaultValidationMessage,
					GetPlaceholderValues(instanceToValidate));

		private string GetValidationMessageWithProperty(string? instanceToValidate)
			=> GetFormattedMessage(
					Resources.ValidationKeys.Length_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues(instanceToValidate));

		internal override ValidationResult? Validate(ValidationContext context)
		{
			if (context.InstanceToValidate == null)
				return null;

			if (context.InstanceToValidate is string value)
			{
				if (MinLength <= value.Length && value.Length <= MaxLength)
					return null;
				else
					return new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(value), GetValidationMessageWithProperty(value)));
			}

			throw new InvalidOperationException($"{nameof(context.InstanceToValidate)} must be string.");
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(typeof(T), ValidationFrame, ValidatorType, GetType().ToFriendlyFullName(), Conditional, ClientConditionDefinition)
			{
				MaxLength = MaxLength,
				MinLength = MinLength
			};
	}
}
