using Raider.Text;
using Raider.Validation.Client;
using System;
using System.Collections.Generic;

namespace Raider.Validation
{
	public interface IPropertyValidator<T> { }

	public class PropertyValidator<T, TProperty> : Validator<T>, IPropertyValidator<T>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.PropertyValidator;
		protected virtual string DefaultValidationMessage { get; }
		protected virtual string DefaultValidationMessageWithProperty { get; }
		protected TemplateFormatter TemplateFormatter { get; }

		internal PropertyValidator(Func<object, object>? func, ValidationFrame validationFrame, bool conditional, IClientConditionDefinition? clientConditionDefinition)
			: base(func, validationFrame, conditional, clientConditionDefinition)
		{
			DefaultValidationMessage = "";
			DefaultValidationMessageWithProperty = "";
			TemplateFormatter = new TemplateFormatter();
		}

		protected PropertyValidator(PropertyValidator<T, TProperty> propertyValidator)
			: base(propertyValidator.Func, propertyValidator.ValidationFrame, propertyValidator.Conditional, propertyValidator.ClientConditionDefinition)
		{
			DefaultValidationMessage = "";
			DefaultValidationMessageWithProperty = "";
			TemplateFormatter = new TemplateFormatter();
		}

		internal override ValidationResult? Validate(ValidationContext validationContext)
		{
			var result = new ValidationResult();

			if (string.IsNullOrWhiteSpace(ValidationFrame.PropertyName))
				throw new InvalidOperationException($"{nameof(ValidationFrame)}.{nameof(ValidationFrame.PropertyName)} == null");

			var ctx = new ValidationContext(validationContext.InstanceToValidate == null ? null : Func(validationContext.InstanceToValidate), validationContext)
				.SetValidationFrame(ValidationFrame);

			foreach (var validator in Validators)
				result.Merge(validator.Validate(ctx));

			return result;
		}

		protected string GetFormattedMessage(string resourceKey, string defaultMessage, IDictionary<string, object?>? placeholderValues = null)
		{
			var template = ValidatorConfiguration.Localizer?[resourceKey] ?? defaultMessage;
			return TemplateFormatter.Format(template, placeholderValues) ?? "?Error";
		}

		protected string GetDisplayName()
			=> null; // ValidatorConfiguration.DisplayNameResolver?.Invoke(typeof(T), PropertyValidator.Expression, PropertyValidator.Expression) ?? PropertyValidator.ValidationFrame.PropertyName;
	}
}
