using Raider.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Validation
{
	public static class PropertyValidatorExtensions
	{
		public static PropertyValidator<T, string?> EmailAddress<T>(this PropertyValidator<T, string?> validator, bool useUTFCharacters = false, bool useSpecialCharacters = false)
		{
			validator.AddValidator(new EmailValidator<T, string?>(validator, useUTFCharacters, useSpecialCharacters));
			return validator;
		}

		//public static PropertyValidator<T, TProperty?> DefaultOrEmpty<T, TProperty>(this PropertyValidator<T, TProperty?> validator)
		//	where TProperty : struct
		//{
		//	var defaultValue = typeof(TProperty).GetDefaultNullableValue();
		//	validator.AddValidator(new DefaultOrEmptyValidator<T, TProperty?>(validator, /*default(TProperty)*/defaultValue));
		//	return validator;
		//}

		public static PropertyValidator<T, TProperty?> DefaultOrEmpty<T, TProperty>(this PropertyValidator<T, TProperty?> validator)
		{
			var defaultValue = typeof(TProperty).GetDefaultNullableValue();
			validator.AddValidator(new DefaultOrEmptyValidator<T, TProperty?>(validator, /*default(TProperty)*/defaultValue));
			return validator;
		}

		//public static PropertyValidator<T, TProperty?> NotDefaultOrEmpty<T, TProperty>(this PropertyValidator<T, TProperty?> validator)
		//	where TProperty : struct
		//{
		//	var defaultValue = typeof(TProperty).GetDefaultNullableValue();
		//	validator.AddValidator(new NotDefaultOrEmptyValidator<T, TProperty?>(validator, /*default(TProperty)*/defaultValue));
		//	return validator;
		//}

		public static PropertyValidator<T, TProperty?> NotDefaultOrEmpty<T, TProperty>(this PropertyValidator<T, TProperty?> validator)
		{
			var defaultValue = typeof(TProperty).GetDefaultNullableValue();
			validator.AddValidator(new NotDefaultOrEmptyValidator<T, TProperty?>(validator, /*default(TProperty)*/defaultValue));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> EqualsTo<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty? value, IEqualityComparer? comparer = null)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new EqualValidator<T, TProperty?>(validator, value, comparer));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> EqualsTo<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty? value, IEqualityComparer? comparer = null)
			where TProperty : IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new EqualValidator<T, TProperty?>(validator, value, comparer));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> NotEqualsTo<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty? value, IEqualityComparer? comparer = null)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new NotEqualValidator<T, TProperty?>(validator, value, comparer));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> NotEqualsTo<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty? value, IEqualityComparer? comparer = null)
			where TProperty : IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new NotEqualValidator<T, TProperty?>(validator, value, comparer));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> EqualsTo<T, TProperty>(this PropertyValidator<T, TProperty?> validator, IEnumerable<TProperty>? values, IEqualityComparer? comparer = null)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			if (values == null || values.Count() < 2)
				validator.AddValidator(new EqualValidator<T, TProperty?>(validator, values?.FirstOrDefault(), comparer));
			else
				validator.AddValidator(new MultiEqualValidator<T, TProperty?>(validator, values?.Cast<IComparable?>(), comparer));

			return validator;
		}

		public static PropertyValidator<T, TProperty?> EqualsTo<T, TProperty>(this PropertyValidator<T, TProperty?> validator, IEnumerable<TProperty>? values, IEqualityComparer? comparer = null)
			where TProperty : IComparable<TProperty>, IComparable
		{

			if (values == null || values.Count() < 2)
				validator.AddValidator(new EqualValidator<T, TProperty?>(validator, values == null ? null : values.FirstOrDefault(), comparer));
			else
				validator.AddValidator(new MultiEqualValidator<T, TProperty?>(validator, values?.Cast<IComparable?>(), comparer));
			
			return validator;
		}

		public static PropertyValidator<T, TProperty?> NotEqualsTo<T, TProperty>(this PropertyValidator<T, TProperty?> validator, IEnumerable<TProperty>? values, IEqualityComparer? comparer = null)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{

			if (values == null || values.Count() < 2)
				validator.AddValidator(new NotEqualValidator<T, TProperty?>(validator, values?.FirstOrDefault(), comparer));
			else
				validator.AddValidator(new MultiNotEqualValidator<T, TProperty?>(validator, values?.Cast<IComparable?>(), comparer));
			
			return validator;
		}

		public static PropertyValidator<T, TProperty?> NotEqualsTo<T, TProperty>(this PropertyValidator<T, TProperty?> validator, IEnumerable<TProperty>? values, IEqualityComparer? comparer = null)
			where TProperty : IComparable<TProperty>, IComparable
		{

			if (values == null || values.Count() < 2)
				validator.AddValidator(new NotEqualValidator<T, TProperty?>(validator, values == null ? null : values.FirstOrDefault(), comparer));
			else
				validator.AddValidator(new MultiNotEqualValidator<T, TProperty?>(validator, values?.Cast<IComparable?>(), comparer));
			
			return validator;
		}

		public static PropertyValidator<T, TProperty?> LessThan<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new LessThanValidator<T, TProperty?>(validator, value));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> LessThan<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty value)
			where TProperty : IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new LessThanValidator<T, TProperty?>(validator, value));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> LessThanOrEqual<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new LessThanOrEqualValidator<T, TProperty?>(validator, value));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> LessThanOrEqual<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty value)
			where TProperty : IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new LessThanOrEqualValidator<T, TProperty?>(validator, value));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> GreaterThan<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new GreaterThanValidator<T, TProperty?>(validator, value));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> GreaterThan<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty value)
			where TProperty : IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new GreaterThanValidator<T, TProperty?>(validator, value));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> GreaterThanOrEqual<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new GreaterThanOrEqualValidator<T, TProperty?>(validator, value));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> GreaterThanOrEqual<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty value)
			where TProperty : IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new GreaterThanOrEqualValidator<T, TProperty?>(validator, value));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> ExclusiveBetween<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty from, TProperty to)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new ExclusiveBetweenValidator<T, TProperty?>(validator, from, to));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> ExclusiveBetween<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty from, TProperty to)
			where TProperty : IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new ExclusiveBetweenValidator<T, TProperty?>(validator, from, to));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> InclusiveBetween<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty from, TProperty to)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new InclusiveBetweenValidator<T, TProperty?>(validator, from, to));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> InclusiveBetween<T, TProperty>(this PropertyValidator<T, TProperty?> validator, TProperty from, TProperty to)
			where TProperty : IComparable<TProperty>, IComparable
		{
			validator.AddValidator(new InclusiveBetweenValidator<T, TProperty?>(validator, from, to));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> Null<T, TProperty>(this PropertyValidator<T, TProperty?> validator)
		{
			validator.AddValidator(new NullValidator<T, TProperty?>(validator));
			return validator;
		}

		public static PropertyValidator<T, TProperty?> NotNull<T, TProperty>(this PropertyValidator<T, TProperty?> validator)
		{
			validator.AddValidator(new NotNullValidator<T, TProperty?>(validator));
			return validator;
		}

		public static PropertyValidator<T, string?> RegEx<T>(this PropertyValidator<T, string?> validator, string? pattern)
		{
			validator.AddValidator(new RegExValidator<T, string?>(validator, pattern));
			return validator;
		}

		public static PropertyValidator<T, string?> MinLength<T>(this PropertyValidator<T, string?> validator, int minLength)
		{
			validator.AddValidator(new LengthValidator<T, string?>(validator, minLength, int.MaxValue));
			return validator;
		}

		public static PropertyValidator<T, string?> MaxLength<T>(this PropertyValidator<T, string?> validator, int maxLength)
		{
			validator.AddValidator(new LengthValidator<T, string?>(validator, 0, maxLength));
			return validator;
		}

		public static PropertyValidator<T, string?> Length<T>(this PropertyValidator<T, string?> validator, int minLength, int maxLength)
		{
			validator.AddValidator(new LengthValidator<T, string?>(validator, minLength, maxLength));
			return validator;
		}

		public static PropertyValidator<T, decimal> PrecisionScale<T>(this PropertyValidator<T, decimal> validator, int precision, int scale, bool ignoreTrailingZeros)
		{
			validator.AddValidator(new PrecisionScaleDecimalValidator<T, decimal>(validator, precision, scale, ignoreTrailingZeros));
			return validator;
		}

		public static PropertyValidator<T, decimal?> PrecisionScale<T>(this PropertyValidator<T, decimal?> validator, int precision, int scale, bool ignoreTrailingZeros)
		{
			validator.AddValidator(new PrecisionScaleDecimalValidator<T, decimal?>(validator, precision, scale, ignoreTrailingZeros));
			return validator;
		}

		public static PropertyValidator<T, double> PrecisionScale<T>(this PropertyValidator<T, double> validator, int precision, int scale, bool ignoreTrailingZeros)
		{
			validator.AddValidator(new PrecisionScaleDoubleValidator<T, double>(validator, precision, scale, ignoreTrailingZeros));
			return validator;
		}

		public static PropertyValidator<T, double?> PrecisionScale<T>(this PropertyValidator<T, double?> validator, int precision, int scale, bool ignoreTrailingZeros)
		{
			validator.AddValidator(new PrecisionScaleDoubleValidator<T, double?>(validator, precision, scale, ignoreTrailingZeros));
			return validator;
		}

		public static PropertyValidator<T, float> PrecisionScale<T>(this PropertyValidator<T, float> validator, int precision, int scale, bool ignoreTrailingZeros)
		{
			validator.AddValidator(new PrecisionScaleFloatValidator<T, float>(validator, precision, scale, ignoreTrailingZeros));
			return validator;
		}

		public static PropertyValidator<T, float?> PrecisionScale<T>(this PropertyValidator<T, float?> validator, int precision, int scale, bool ignoreTrailingZeros)
		{
			validator.AddValidator(new PrecisionScaleFloatValidator<T, float?>(validator, precision, scale, ignoreTrailingZeros));
			return validator;
		}
	}
}
