using Raider.Extensions;
using Raider.Validation.Internal;
using System.Collections.Generic;

namespace Raider.Validation
{
	internal class DefaultOrEmptyValidator<T, TProperty> : PropertyValidator<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.DefaultOrEmpty;

		protected override string DefaultValidationMessage => "Must be empty.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must be empty.";

		private readonly object? _defaultValue;

		public DefaultOrEmptyValidator(PropertyValidator<T, TProperty> propertyValidator, object? defaultValue)
			: base(propertyValidator)
		{
			_defaultValue = defaultValue;
		}

		private IDictionary<string, object?> GetPlaceholderValues()
			=> new Dictionary<string, object?>
			{
				{ "PropertyName", GetDisplayName() }
			};

		private string GetValidationMessage()
			=> GetFormattedMessage(
					Resources.ValidationKeys.DefaultOrEmpty,
					DefaultValidationMessage,
					GetPlaceholderValues());

		private string GetValidationMessageWithProperty()
			=> GetFormattedMessage(
					Resources.ValidationKeys.DefaultOrEmpty_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues());

		internal override ValidationResult? Validate(ValidationContext context)
			=> ValidationHelper.IsDefaultOrEmpty(context.InstanceToValidate, _defaultValue)
				? null
				: new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty(), DetailInfoFunc?.Invoke(context.InstanceToValidate)));

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(typeof(T), ValidationFrame, ValidatorType, GetType().ToFriendlyFullName(), Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty())
			{
				DefaultValue = _defaultValue
			};
	}


	internal class NotDefaultOrEmptyValidator<T, TProperty> : PropertyValidator<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.NotDefaultOrEmpty;

		protected override string DefaultValidationMessage => "Must  not be empty.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must not be empty.";

		private readonly object? _defaultValue;

		public NotDefaultOrEmptyValidator(PropertyValidator<T, TProperty> propertyValidator, object? defaultValue)
			: base(propertyValidator)
		{
			_defaultValue = defaultValue;
		}

		private IDictionary<string, object?> GetPlaceholderValues()
			=> new Dictionary<string, object?>
			{
				{ "PropertyName", GetDisplayName() }
			};

		private string GetValidationMessage()
			=> GetFormattedMessage(
					Resources.ValidationKeys.NotDefaultOrEmpty,
					DefaultValidationMessage,
					GetPlaceholderValues());

		private string GetValidationMessageWithProperty()
			=> GetFormattedMessage(
					Resources.ValidationKeys.NotDefaultOrEmpty_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues());

		internal override ValidationResult? Validate(ValidationContext context)
			=> ValidationHelper.IsDefaultOrEmpty(context.InstanceToValidate, _defaultValue)
				? new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty(), DetailInfoFunc?.Invoke(context.InstanceToValidate)))
				: null;

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(typeof(T), ValidationFrame, ValidatorType, GetType().ToFriendlyFullName(), Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty())
			{
				DefaultValue = _defaultValue
			};
	}
}
