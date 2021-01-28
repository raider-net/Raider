using Raider.Validation.Internal;
using System.Collections.Generic;

namespace Raider.Validation
{
	internal class DefaultOrEmptyClassValidator<T, TProperty> : ValidatorBase<T, TProperty>
			where TProperty : class
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.DefaultOrEmptyClass;

		protected override string DefaultValidationMessage => "Must be empty.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must be empty.";

		public DefaultOrEmptyClassValidator(PropertyValidator<T, TProperty> propertyValidator)
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
					Resources.ValidationKeys.DefaultOrEmpty,
					DefaultValidationMessage,
					GetPlaceholderValues());

		private string GetValidationMessageWithProperty()
			=> GetFormattedMessage(
					Resources.ValidationKeys.DefaultOrEmpty_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues());

		internal override ValidationFailure? Validate(IPropertyValidationContext context)
			=> (ValidationHelper.IsDefaultOrEmpty(context.InstanceToValidate) || ValidationHelper.IsDefault(context.InstanceToValidate))
				? null
				: new ValidationFailure(context.ToReadOnlyValidationFrame(), this, GetValidationMessage(), GetValidationMessageWithProperty());

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(PropertyValidator.ValidationFrame, ValidatorType, Conditional);
	}

	internal class DefaultOrEmptyStructValidator<T, TProperty> : ValidatorBase<T, TProperty>
			where TProperty : struct
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.DefaultOrEmptyStruct;

		protected override string DefaultValidationMessage => "Must be empty.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must be empty.";

		public DefaultOrEmptyStructValidator(PropertyValidator<T, TProperty> propertyValidator)
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
					Resources.ValidationKeys.DefaultOrEmpty,
					DefaultValidationMessage,
					GetPlaceholderValues());

		private string GetValidationMessageWithProperty()
			=> GetFormattedMessage(
					Resources.ValidationKeys.DefaultOrEmpty_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues());

		internal override ValidationFailure? Validate(IPropertyValidationContext context)
		{
			var propertyValue = (TProperty?)context.InstanceToValidate;

			return (ValidationHelper.IsDefaultOrEmpty(propertyValue))
				? null
				: new ValidationFailure(context.ToReadOnlyValidationFrame(), this, GetValidationMessage(), GetValidationMessageWithProperty());
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(PropertyValidator.ValidationFrame, ValidatorType, Conditional);
	}

	internal class NotDefaultOrEmptyClassValidator<T, TProperty> : ValidatorBase<T, TProperty>
			where TProperty : class
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.NotDefaultOrEmptyClass;

		protected override string DefaultValidationMessage => "Must  not be empty.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must not be empty.";

		public NotDefaultOrEmptyClassValidator(PropertyValidator<T, TProperty> propertyValidator)
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
					Resources.ValidationKeys.NotDefaultOrEmpty,
					DefaultValidationMessage,
					GetPlaceholderValues());

		private string GetValidationMessageWithProperty()
			=> GetFormattedMessage(
					Resources.ValidationKeys.NotDefaultOrEmpty_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues());

		internal override ValidationFailure? Validate(IPropertyValidationContext context)
			=> (ValidationHelper.IsDefaultOrEmpty(context.InstanceToValidate) || ValidationHelper.IsDefault(context.InstanceToValidate))
				? new ValidationFailure(context.ToReadOnlyValidationFrame(), this, GetValidationMessage(), GetValidationMessageWithProperty())
				: null;

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(PropertyValidator.ValidationFrame, ValidatorType, Conditional);
	}

	internal class NotDefaultOrEmptyStructValidator<T, TProperty> : ValidatorBase<T, TProperty>
			where TProperty : struct
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.NotDefaultOrEmptyStruct;

		protected override string DefaultValidationMessage => "Must  not be empty.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must not be empty.";

		public NotDefaultOrEmptyStructValidator(PropertyValidator<T, TProperty> propertyValidator)
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
					Resources.ValidationKeys.NotDefaultOrEmpty,
					DefaultValidationMessage,
					GetPlaceholderValues());

		private string GetValidationMessageWithProperty()
			=> GetFormattedMessage(
					Resources.ValidationKeys.NotDefaultOrEmpty_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues());

		internal override ValidationFailure? Validate(IPropertyValidationContext context)
		{
			var propertyValue = (TProperty?)context.InstanceToValidate;

			return (ValidationHelper.IsDefaultOrEmpty(propertyValue))
				? new ValidationFailure(context.ToReadOnlyValidationFrame(), this, GetValidationMessage(), GetValidationMessageWithProperty())
				: null;
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(PropertyValidator.ValidationFrame, ValidatorType, Conditional);
	}
}
