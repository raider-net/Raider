using Raider.Extensions;
using Raider.Validation.Internal;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Raider.Validation
{
	internal class RegExValidator<T, TProperty> : PropertyValidator<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.RegEx;

		protected override string DefaultValidationMessage => "Is not in the correct format.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' is not in the correct format.";

		private readonly Regex? _regex;

		public string? Pattern { get; }

		public RegExValidator(PropertyValidator<T, TProperty> propertyValidator, string? pattern)
			: base(propertyValidator)
		{
			Pattern = pattern;
			if (Pattern != null)
				_regex = new Regex(Pattern, RegexOptions.None, TimeSpan.FromSeconds(2.0));
		}

		private IDictionary<string, object?> GetPlaceholderValues()
			=> new Dictionary<string, object?>
			{
				{ "PropertyName", GetDisplayName() }
			};

		private string GetValidationMessage()
			=> GetFormattedMessage(
					Resources.ValidationKeys.RegEx,
					DefaultValidationMessage,
					GetPlaceholderValues());

		private string GetValidationMessageWithProperty()
			=> GetFormattedMessage(
					Resources.ValidationKeys.RegEx_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues());

		internal override ValidationResult? Validate(ValidationContext context)
			=> context.InstanceToValidate == null || _regex == null || (context.InstanceToValidate is string value && _regex.IsMatch(value))
				? null
				: new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty()));

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(typeof(T), ValidationFrame, ValidatorType, GetType().ToFriendlyFullName(), Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty())
			{
				Pattern = Pattern
			};
	}
}
