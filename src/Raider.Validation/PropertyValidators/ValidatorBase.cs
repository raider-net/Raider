using Raider.Text;
using System;
using System.Collections.Generic;

namespace Raider.Validation
{
	internal abstract class ValidatorBase<T, TProperty> : IValidator
	{
		protected PropertyValidator<T, TProperty> PropertyValidator { get; }
		protected abstract string DefaultValidationMessage { get; }
		protected abstract string DefaultValidationMessageWithProperty { get; }
		public bool Conditional => PropertyValidator.Conditional;
		public abstract ValidatorType ValidatorType { get; }
		protected TemplateFormatter TemplateFormatter { get; }

		ValidationFrame IValidator.ValidationFrame => PropertyValidator.ValidationFrame;

		public ValidatorBase(PropertyValidator<T, TProperty> propertyValidator)
		{
			PropertyValidator = propertyValidator ?? throw new ArgumentNullException(nameof(propertyValidator));
			TemplateFormatter = new TemplateFormatter();
		}

		public abstract IValidationDescriptor ToDescriptor();

		ValidationResult IValidator.Validate(object? obj)
			=> throw new NotImplementedException();

		internal abstract ValidationFailure? Validate(IPropertyValidationContext context);

		protected string GetFormattedMessage(string resourceKey, string defaultMessage, IDictionary<string, object?>? placeholderValues = null)
		{
			var template = ValidatorConfiguration.Localizer?[resourceKey] ?? defaultMessage;
			return TemplateFormatter.Format(template, placeholderValues) ?? "?Error";
		}

		protected string GetDisplayName()
			=> null; // ValidatorConfiguration.DisplayNameResolver?.Invoke(typeof(T), PropertyValidator.Expression, PropertyValidator.Expression) ?? PropertyValidator.ValidationFrame.PropertyName;
	}
}
