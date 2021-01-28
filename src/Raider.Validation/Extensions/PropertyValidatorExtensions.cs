using System;

namespace Raider.Validation
{
	public static class PropertyValidatorExtensions
	{
		public static ClassPropertyValidator<T, string> EmailAddress<T>(this ClassPropertyValidator<T, string> validator)
		{
			validator.Validators.Add(new EmailValidator<T, string>(validator));
			return validator;
		}

		public static StructPropertyValidator<T, TProperty> DefaultOrEmpty<T, TProperty>(this StructPropertyValidator<T, TProperty> validator)
			where TProperty : struct
		{
			validator.Validators.Add(new DefaultOrEmptyStructValidator<T, TProperty>(validator));
			return validator;
		}

		public static ClassPropertyValidator<T, TProperty> DefaultOrEmpty<T, TProperty>(this ClassPropertyValidator<T, TProperty> validator)
			where TProperty : class
		{
			validator.Validators.Add(new DefaultOrEmptyClassValidator<T, TProperty>(validator));
			return validator;
		}

		public static StructPropertyValidator<T, TProperty> NotDefaultOrEmpty<T, TProperty>(this StructPropertyValidator<T, TProperty> validator)
			where TProperty : struct
		{
			validator.Validators.Add(new NotDefaultOrEmptyStructValidator<T, TProperty>(validator));
			return validator;
		}

		public static ClassPropertyValidator<T, TProperty> NotDefaultOrEmpty<T, TProperty>(this ClassPropertyValidator<T, TProperty> validator)
			where TProperty : class
		{
			validator.Validators.Add(new NotDefaultOrEmptyClassValidator<T, TProperty>(validator));
			return validator;
		}

		public static StructPropertyValidator<T, TProperty> EqualsTo<T, TProperty>(this StructPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new EqualValidator<T, TProperty>(validator,value));
			return validator;
		}

		public static ClassPropertyValidator<T, TProperty> EqualsTo<T, TProperty>(this ClassPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : class, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new EqualValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static StructPropertyValidator<T, TProperty> NotEqualsTo<T, TProperty>(this StructPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new NotEqualValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static ClassPropertyValidator<T, TProperty> NotEqualsTo<T, TProperty>(this ClassPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : class, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new NotEqualValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static StructPropertyValidator<T, TProperty> LessThan<T, TProperty>(this StructPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new LessThanValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static ClassPropertyValidator<T, TProperty> LessThan<T, TProperty>(this ClassPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : class, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new LessThanValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static StructPropertyValidator<T, TProperty> LessThanOrEqual<T, TProperty>(this StructPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new LessThanOrEqualValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static ClassPropertyValidator<T, TProperty> LessThanOrEqual<T, TProperty>(this ClassPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : class, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new LessThanOrEqualValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static StructPropertyValidator<T, TProperty> GreaterThan<T, TProperty>(this StructPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new GreaterThanValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static ClassPropertyValidator<T, TProperty> GreaterThan<T, TProperty>(this ClassPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : class, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new GreaterThanValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static StructPropertyValidator<T, TProperty> GreaterThanOrEqual<T, TProperty>(this StructPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new GreaterThanOrEqualValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static ClassPropertyValidator<T, TProperty> GreaterThanOrEqual<T, TProperty>(this ClassPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : class, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new GreaterThanOrEqualValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static StructPropertyValidator<T, TProperty> ExclusiveBetween<T, TProperty>(this StructPropertyValidator<T, TProperty> validator, TProperty from, TProperty to)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new ExclusiveBetweenValidator<T, TProperty>(validator, from, to));
			return validator;
		}

		public static ClassPropertyValidator<T, TProperty> ExclusiveBetween<T, TProperty>(this ClassPropertyValidator<T, TProperty> validator, TProperty from, TProperty to)
			where TProperty : class, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new ExclusiveBetweenValidator<T, TProperty>(validator, from, to));
			return validator;
		}

		public static StructPropertyValidator<T, TProperty> InclusiveBetween<T, TProperty>(this StructPropertyValidator<T, TProperty> validator, TProperty from, TProperty to)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new InclusiveBetweenValidator<T, TProperty>(validator, from, to));
			return validator;
		}

		public static ClassPropertyValidator<T, TProperty> InclusiveBetween<T, TProperty>(this ClassPropertyValidator<T, TProperty> validator, TProperty from, TProperty to)
			where TProperty : class, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new InclusiveBetweenValidator<T, TProperty>(validator, from, to));
			return validator;
		}

		public static ClassPropertyValidator<T, TProperty> Null<T, TProperty>(this ClassPropertyValidator<T, TProperty> validator)
			where TProperty : class
		{
			validator.Validators.Add(new NullValidator<T, TProperty>(validator));
			return validator;
		}

		public static StructPropertyValidator<T, TProperty> Null<T, TProperty>(this StructPropertyValidator<T, TProperty> validator)
			where TProperty : struct
		{
			validator.Validators.Add(new NullValidator<T, TProperty>(validator));
			return validator;
		}

		public static ClassPropertyValidator<T, TProperty> NotNull<T, TProperty>(this ClassPropertyValidator<T, TProperty> validator)
			where TProperty : class
		{
			validator.Validators.Add(new NotNullValidator<T, TProperty>(validator));
			return validator;
		}

		public static StructPropertyValidator<T, TProperty> NotNull<T, TProperty>(this StructPropertyValidator<T, TProperty> validator)
			where TProperty : struct
		{
			validator.Validators.Add(new NotNullValidator<T, TProperty>(validator));
			return validator;
		}

		public static ClassPropertyValidator<T, string> RegEx<T>(this ClassPropertyValidator<T, string> validator, string? pattern)
		{
			validator.Validators.Add(new RegExValidator<T, string>(validator, pattern));
			return validator;
		}

		public static ClassPropertyValidator<T, string> MinLength<T>(this ClassPropertyValidator<T, string> validator, int minLength)
		{
			validator.Validators.Add(new LengthValidator<T, string>(validator, minLength, int.MaxValue));
			return validator;
		}

		public static ClassPropertyValidator<T, string> MaxLength<T>(this ClassPropertyValidator<T, string> validator, int maxLength)
		{
			validator.Validators.Add(new LengthValidator<T, string>(validator, 0, maxLength));
			return validator;
		}

		public static ClassPropertyValidator<T, string> Length<T>(this ClassPropertyValidator<T, string> validator, int minLength, int maxLength)
		{
			validator.Validators.Add(new LengthValidator<T, string>(validator, minLength, maxLength));
			return validator;
		}

		public static StructPropertyValidator<T, decimal> PrecisionScale<T>(this StructPropertyValidator<T, decimal> validator, int precision, int scale, bool ignoreTrailingZeros)
		{
			validator.Validators.Add(new PrecisionScaleDecimalValidator<T, decimal>(validator, precision, scale, ignoreTrailingZeros));
			return validator;
		}

		public static StructPropertyValidator<T, double> PrecisionScale<T>(this StructPropertyValidator<T, double> validator, int precision, int scale, bool ignoreTrailingZeros)
		{
			validator.Validators.Add(new PrecisionScaleDoubleValidator<T, double>(validator, precision, scale, ignoreTrailingZeros));
			return validator;
		}

		public static StructPropertyValidator<T, float> PrecisionScale<T>(this StructPropertyValidator<T, float> validator, int precision, int scale, bool ignoreTrailingZeros)
		{
			validator.Validators.Add(new PrecisionScaleFloatValidator<T, float>(validator, precision, scale, ignoreTrailingZeros));
			return validator;
		}

		public static NullableStructPropertyValidator<T, TProperty> DefaultOrEmpty<T, TProperty>(this NullableStructPropertyValidator<T, TProperty> validator)
			where TProperty : struct
		{
			validator.Validators.Add(new DefaultOrEmptyStructValidator<T, TProperty>(validator));
			return validator;
		}

		public static NullableStructPropertyValidator<T, TProperty> NotDefaultOrEmpty<T, TProperty>(this NullableStructPropertyValidator<T, TProperty> validator)
			where TProperty : struct
		{
			validator.Validators.Add(new NotDefaultOrEmptyStructValidator<T, TProperty>(validator));
			return validator;
		}

		public static NullableStructPropertyValidator<T, TProperty> EqualsTo<T, TProperty>(this NullableStructPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new EqualValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static NullableStructPropertyValidator<T, TProperty> NotEqualsTo<T, TProperty>(this NullableStructPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new NotEqualValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static NullableStructPropertyValidator<T, TProperty> LessThan<T, TProperty>(this NullableStructPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new LessThanValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static NullableStructPropertyValidator<T, TProperty> LessThanOrEqual<T, TProperty>(this NullableStructPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new LessThanOrEqualValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static NullableStructPropertyValidator<T, TProperty> GreaterThan<T, TProperty>(this NullableStructPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new GreaterThanValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static NullableStructPropertyValidator<T, TProperty> GreaterThanOrEqual<T, TProperty>(this NullableStructPropertyValidator<T, TProperty> validator, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new GreaterThanOrEqualValidator<T, TProperty>(validator, value));
			return validator;
		}

		public static NullableStructPropertyValidator<T, TProperty> ExclusiveBetween<T, TProperty>(this NullableStructPropertyValidator<T, TProperty> validator, TProperty from, TProperty to)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new ExclusiveBetweenValidator<T, TProperty>(validator, from, to));
			return validator;
		}

		public static NullableStructPropertyValidator<T, TProperty> InclusiveBetween<T, TProperty>(this NullableStructPropertyValidator<T, TProperty> validator, TProperty from, TProperty to)
			where TProperty : struct, IComparable<TProperty>, IComparable
		{
			validator.Validators.Add(new InclusiveBetweenValidator<T, TProperty>(validator, from, to));
			return validator;
		}

		public static NullableStructPropertyValidator<T, TProperty> Null<T, TProperty>(this NullableStructPropertyValidator<T, TProperty> validator)
			where TProperty : struct
		{
			validator.Validators.Add(new NullValidator<T, TProperty>(validator));
			return validator;
		}

		public static NullableStructPropertyValidator<T, TProperty> NotNull<T, TProperty>(this NullableStructPropertyValidator<T, TProperty> validator)
			where TProperty : struct
		{
			validator.Validators.Add(new NotNullValidator<T, TProperty>(validator));
			return validator;
		}

		public static NullableStructPropertyValidator<T, decimal> PrecisionScale<T>(this NullableStructPropertyValidator<T, decimal> validator, int precision, int scale, bool ignoreTrailingZeros)
		{
			validator.Validators.Add(new PrecisionScaleDecimalValidator<T, decimal>(validator, precision, scale, ignoreTrailingZeros));
			return validator;
		}

		public static NullableStructPropertyValidator<T, double> PrecisionScale<T>(this NullableStructPropertyValidator<T, double> validator, int precision, int scale, bool ignoreTrailingZeros)
		{
			validator.Validators.Add(new PrecisionScaleDoubleValidator<T, double>(validator, precision, scale, ignoreTrailingZeros));
			return validator;
		}

		public static NullableStructPropertyValidator<T, float> PrecisionScale<T>(this NullableStructPropertyValidator<T, float> validator, int precision, int scale, bool ignoreTrailingZeros)
		{
			validator.Validators.Add(new PrecisionScaleFloatValidator<T, float>(validator, precision, scale, ignoreTrailingZeros));
			return validator;
		}
	}
}
