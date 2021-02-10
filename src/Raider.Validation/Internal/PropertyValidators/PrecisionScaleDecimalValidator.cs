using Raider.Extensions;
using Raider.Validation.Internal;
using System;
using System.Collections.Generic;

namespace Raider.Validation
{
	internal class PrecisionScaleDecimalValidator<T, TProperty> : PropertyValidator<T, TProperty>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.PrecisionScale;

		protected override string DefaultValidationMessage => "Must  not be more than {ExpectedPrecision} digits in total, with allowance for {ExpectedScale} decimals. {Digits} digits and {ActualScale} decimals were found.";
		protected override string DefaultValidationMessageWithProperty => "'{PropertyName}' must not be more than {ExpectedPrecision} digits in total, with allowance for {ExpectedScale} decimals. {Digits} digits and {ActualScale} decimals were found.";

		public int Scale { get; }
		public int Precision { get; }
		public bool IgnoreTrailingZeros { get; }

		public PrecisionScaleDecimalValidator(PropertyValidator<T, TProperty> propertyValidator, int precision, int scale, bool ignoreTrailingZeros)
			: base(propertyValidator)
		{
			Scale = scale;
			Precision = precision;
			IgnoreTrailingZeros = ignoreTrailingZeros;

			if (Scale < 0)
				throw new ArgumentOutOfRangeException(nameof(scale), $"Scale must be a positive integer. [value:{Scale}].");

			if (Precision < 0)
				throw new ArgumentOutOfRangeException(nameof(precision), $"Precision must be a positive integer. [value:{Precision}].");

			if (Precision < Scale)
				throw new ArgumentOutOfRangeException(nameof(scale), $"Scale must be less than precision. [scale:{Scale}, precision:{Precision}].");
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(typeof(T), ValidationFrame, ValidatorType, GetType().ToFriendlyFullName(), Conditional, ClientConditionDefinition)
			{
				Scale = Scale,
				Precision = Precision,
				IgnoreTrailingZeros = IgnoreTrailingZeros
			};

		private IDictionary<string, object?> GetPlaceholderValues(int scale, int actualIntegerDigits)
			=> new Dictionary<string, object?>
			{
				{ "ExpectedPrecision", Precision },
				{ "ExpectedScale", Scale },
				{ "ActualScale", scale },
				{ "Digits", actualIntegerDigits },
				{ "PropertyName", GetDisplayName() }
			};

		private string GetValidationMessage(int scale, int actualIntegerDigits)
			=> GetFormattedMessage(
					Resources.ValidationKeys.PrecisionScale,
					DefaultValidationMessage,
					GetPlaceholderValues(scale, actualIntegerDigits));

		private string GetValidationMessageWithProperty(int scale, int actualIntegerDigits)
			=> GetFormattedMessage(
					Resources.ValidationKeys.PrecisionScale_WithProperty,
					DefaultValidationMessageWithProperty,
					GetPlaceholderValues(scale, actualIntegerDigits));

		internal override ValidationResult? Validate(ValidationContext context)
		{
			if (context.InstanceToValidate is not decimal value)
				return null;

			var scale = GetScale(value);
			var precision = GetPrecision(value);
			var actualIntegerDigits = precision - scale;
			var expectedIntegerDigits = Precision - Scale;
			if (Scale < scale || expectedIntegerDigits < actualIntegerDigits)
			{
				return new ValidationResult(new ValidationFailure(context.ToReadOnlyValidationFrame(), ValidatorType, Conditional, ClientConditionDefinition, GetValidationMessage(scale, actualIntegerDigits), GetValidationMessageWithProperty(scale, actualIntegerDigits)));
			}

			return null;
		}

		private static UInt32[] GetBits(decimal @decimal)
		{
			return (uint[])(object)decimal.GetBits(@decimal);
		}

		private static decimal GetMantissa(decimal @decimal)
		{
			var bits = GetBits(@decimal);
			return (bits[2] * 4294967296m * 4294967296m) + (bits[1] * 4294967296m) + bits[0];
		}


		private static uint GetUnsignedScale(decimal @decimal)
		{
			var bits = GetBits(@decimal);
			uint scale = (bits[3] >> 16) & 31;
			return scale;
		}

		private int GetScale(decimal @decimal)
		{
			uint scale = GetUnsignedScale(@decimal);
			if (IgnoreTrailingZeros)
			{
				return (int)(scale - NumTrailingZeros(@decimal));
			}

			return (int)scale;
		}

		private static uint NumTrailingZeros(decimal @decimal)
		{
			uint trailingZeros = 0;
			uint scale = GetUnsignedScale(@decimal);
			for (decimal tmp = GetMantissa(@decimal); tmp % 10m == 0 && trailingZeros < scale; tmp /= 10)
			{
				trailingZeros++;
			}

			return trailingZeros;
		}

		private int GetPrecision(decimal @decimal)
		{
			uint precision = 0;
			for (decimal tmp = GetMantissa(@decimal); tmp >= 1; tmp /= 10)
			{
				precision++;
			}

			if (IgnoreTrailingZeros)
			{
				return (int)(precision - NumTrailingZeros(@decimal));
			}

			return (int)precision;
		}
	}
}
