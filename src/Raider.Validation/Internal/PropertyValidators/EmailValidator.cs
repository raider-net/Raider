using Raider.Extensions;
using Raider.Validation.Internal;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Raider.Validation
{
	internal class EmailValidator<T, TProperty> : PropertyValidator<T, TProperty>
	{
		private const string _expressionUTF_F_SPEC_F = @"^[a-zA-Z0-9](?!.*[.-]{2})[a-zA-Z0-9_.-]*[a-zA-Z0-9](?<![.-])@(?!-)[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,}(?<!\.)$";
		private const string _expressionUTF_F_SPEC_T = @"^[a-zA-Z0-9](?!.*[.-]{2})[a-zA-Z0-9!$%&'*+/=?^_`{|}~.-]*[a-zA-Z0-9](?<![.-])@(?!-)[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,}(?<!\.)$";
		private const string _expressionUTF_T_SPEC_F = @"^[\p{L}\p{N}](?!.*[.-]{2})[\p{L}\p{N}_.-]*[\p{L}\p{N}](?<![.-])@(?!-)[\p{L}\p{N}-]+(\.[\p{L}\p{N}-]+)*\.[\p{L}]{2,}(?<!\.)$";
		private const string _expressionUTF_T_SPEC_T = @"^[\p{L}\p{N}](?!.*[.-]{2})[\p{L}\p{N}!$%&'*+/=?^_`{|}~.-]*[\p{L}\p{N}](?<![.-])@(?!-)[\p{L}\p{N}-]+(\.[\p{L}\p{N}-]+)*\.[\p{L}]{2,}(?<!\.)$";
		private static readonly Regex _regex_F_SPEC_F = CreateRegEx(false, false);
		private static readonly Regex _regex_F_SPEC_T = CreateRegEx(false, true);
		private static readonly Regex _regex_T_SPEC_F = CreateRegEx(true, false);
		private static readonly Regex _regex_T_SPEC_T = CreateRegEx(true, true);
		private readonly bool _useUTFCharacters;
		private readonly bool _useSpecialCharacters;

		public override ValidatorType ValidatorType { get; } = ValidatorType.Email;

		protected override string DefaultValidationMessage => "Is not a valid email address.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' is not a valid email address.";

		public EmailValidator(
			PropertyValidator<T, TProperty> propertyValidator,
			bool useUTFCharacters = false,
			bool useSpecialCharacters = false)
			: base(propertyValidator)
		{
			_useUTFCharacters = useUTFCharacters;
			_useSpecialCharacters = useSpecialCharacters;
		}

		private static Regex CreateRegEx(bool useUTFCharacters, bool useSpecialCharacters)
		{
			const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

			if (useUTFCharacters)
			{
				if (useSpecialCharacters)
				{
					return new Regex(_expressionUTF_T_SPEC_T, options, TimeSpan.FromSeconds(2.0));
				}
				else
				{
					return new Regex(_expressionUTF_T_SPEC_F, options, TimeSpan.FromSeconds(2.0));
				}
			}
			else
			{
				if (useSpecialCharacters)
				{
					return new Regex(_expressionUTF_F_SPEC_T, options, TimeSpan.FromSeconds(2.0));
				}
				else
				{
					return new Regex(_expressionUTF_F_SPEC_F, options, TimeSpan.FromSeconds(2.0));
				}
			}
		}

		private IDictionary<string, object?> GetPlaceholderValues()
			=> new Dictionary<string, object?>
			{
				{ "PropertyName", GetDisplayName() }
			};

		private string GetValidationMessage()
			=> GetFormattedMessage(
					Resources.ValidationKeys.Email,
					DefaultValidationMessage,
					GetPlaceholderValues());

		private string GetValidationMessageWithProperty()
			=> GetFormattedMessage(
					Resources.ValidationKeys.Email_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues());

		internal override ValidationResult? Validate(ValidationContext context)
		{
			Regex regex;

			if (_useUTFCharacters)
			{
				if (_useSpecialCharacters)
				{
					regex = _regex_T_SPEC_T;
				}
				else
				{
					regex = _regex_T_SPEC_F;
				}
			}
			else
			{
				if (_useSpecialCharacters)
				{
					regex = _regex_F_SPEC_T;
				}
				else
				{
					regex = _regex_F_SPEC_F;
				}
			}

			return context.InstanceToValidate == null || (context.InstanceToValidate is string value && regex.IsMatch(value))
				? null
				: new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty(), DetailInfoFunc?.Invoke(context.InstanceToValidate)));
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(typeof(T), ValidationFrame, ValidatorType, GetType().ToFriendlyFullName(), Conditional, ClientConditionDefinition, GetValidationMessage(), GetValidationMessageWithProperty());
	}
}
