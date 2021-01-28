using Raider.Validation.Internal;
using System.Collections.Generic;

namespace Raider.Validation
{
	internal class NullValidator<T, TProperty> : ValidatorBase<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.Null;

		protected override string DefaultValidationMessage => "Must be empty.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must be empty.";

		public NullValidator(PropertyValidator<T, TProperty> propertyValidator)
			: base(propertyValidator)
		{
		}

		private IDictionary<string, object?> GetPlaceholderValues()
			=> new Dictionary<string, object?>
			{
				{ "PropertyName", GetDisplayName() }
			};

		private string GetValidationMessage()
			=> GetFormattedMessage(
					Resources.ValidationKeys.Null,
					DefaultValidationMessage,
					GetPlaceholderValues());

		private string GetValidationMessageWithProperty()
			=> GetFormattedMessage(
					Resources.ValidationKeys.Null_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues());

		internal override ValidationFailure? Validate(IPropertyValidationContext context)
			=> context.InstanceToValidate == null
				? null
				: new ValidationFailure(context.ToReadOnlyValidationFrame(), this, GetValidationMessage(), GetValidationMessageWithProperty());

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(PropertyValidator.ValidationFrame, ValidatorType, Conditional);
	}

	internal class NotNullValidator<T, TProperty> : ValidatorBase<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.NotNull;

		protected override string DefaultValidationMessage => "Must not be empty.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must not be empty.";

		public NotNullValidator(PropertyValidator<T, TProperty> propertyValidator)
			: base(propertyValidator)
		{
		}

		private IDictionary<string, object?> GetPlaceholderValues()
			=> new Dictionary<string, object?>
			{
				{ "PropertyName", GetDisplayName() }
			};

		private string GetValidationMessage()
			=> GetFormattedMessage(
					Resources.ValidationKeys.NotNull,
					DefaultValidationMessage,
					GetPlaceholderValues());

		private string GetValidationMessageWithProperty()
			=> GetFormattedMessage(
					Resources.ValidationKeys.NotNull_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues());

		internal override ValidationFailure? Validate(IPropertyValidationContext context)
			=> context.InstanceToValidate == null
				? new ValidationFailure(context.ToReadOnlyValidationFrame(), this, GetValidationMessage(), GetValidationMessageWithProperty())
				: null;

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(PropertyValidator.ValidationFrame, ValidatorType, Conditional);
	}
}
