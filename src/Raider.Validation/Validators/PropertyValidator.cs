using Raider.Extensions;
using Raider.Validation.Internal;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Raider.Validation
{
	public abstract class PropertyValidator<T> : IValidator
	{
		public bool Conditional { get; }
		public ValidatorType ValidatorType { get; } = ValidatorType.NONE;
		internal Func<object, object> Func { get; }
		internal ValidationFrame ValidationFrame { get; set; }

		ValidationFrame IValidator.ValidationFrame => ValidationFrame;

		public abstract IValidationDescriptor ToDescriptor();
		internal abstract ValidationResult Validate(ValidationContext validationContext);

		ValidationResult IValidator.Validate(object? obj)
			=> throw new NotImplementedException();

		public PropertyValidator(Func<object, object> func, bool conditional)
		{
			Func = func ?? throw new ArgumentNullException(nameof(func));
			Conditional = conditional;
		}
	}

	public abstract class PropertyValidator<T, TProperty> : PropertyValidator<T>, IValidator
	{
		public PropertyValidator(Func<object, object> func, bool conditional)
			: base(func, conditional)
		{
		}
	}

	public class ClassPropertyValidator<T, TProperty> : PropertyValidator<T, TProperty>, IValidator
		where TProperty : class
	{
		internal List<ValidatorBase<T, TProperty>> Validators { get; set; }
		internal Expression<Func<T, TProperty>> Expression { get; }

		public ClassPropertyValidator(Expression<Func<T, TProperty>> expression, ValidationFrame validationFrame, bool conditional)
			: base(PropertyAccessor.GetCachedAccessor(expression).ToNonGeneric(), conditional)
		{
			ValidationFrame = validationFrame ?? throw new ArgumentNullException(nameof(validationFrame));
			Validators = new List<ValidatorBase<T, TProperty>>();
			Expression = expression;
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(ValidationFrame, ValidatorType, Conditional)
				.AddValidators(Validators);

		internal override ValidationResult Validate(ValidationContext validationContext)
		{
			var result = new ValidationResult();

			if (string.IsNullOrWhiteSpace(ValidationFrame.PropertyName))
				throw new InvalidOperationException($"{nameof(ValidationFrame)}.{nameof(ValidationFrame.PropertyName)} == null");

			var ctx = new RTPropertyValidationContext<T, TProperty>(ValidationFrame, validationContext, validationContext.InstanceToValidate == null ? null : Func(validationContext.InstanceToValidate));

			foreach (var validator in Validators)
				result.AddFailure(validator.Validate(ctx));

			return result;
		}
	}

	public class StructPropertyValidator<T, TProperty> : PropertyValidator<T, TProperty>, IValidator
		where TProperty : struct
	{
		internal List<ValidatorBase<T, TProperty>> Validators { get; set; }
		internal Expression<Func<T, TProperty>> Expression { get; }

		public StructPropertyValidator(Expression<Func<T, TProperty>> expression, ValidationFrame validationFrame, bool conditional)
			: base(PropertyAccessor.GetCachedAccessor(expression).ToNonGeneric(), conditional)
		{
			ValidationFrame = validationFrame ?? throw new ArgumentNullException(nameof(validationFrame));
			Validators = new List<ValidatorBase<T, TProperty>>();
			Expression = expression;
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(ValidationFrame, ValidatorType, Conditional)
				.AddValidators(Validators);

		internal override ValidationResult Validate(ValidationContext validationContext)
		{
			var result = new ValidationResult();

			if (string.IsNullOrWhiteSpace(ValidationFrame.PropertyName))
				throw new InvalidOperationException($"{nameof(ValidationFrame)}.{nameof(ValidationFrame.PropertyName)} == null");

			var ctx = new SPropertyValidationContext<T, TProperty>(ValidationFrame, validationContext, validationContext.InstanceToValidate == null ? null : Func(validationContext.InstanceToValidate));

			foreach (var validator in Validators)
				result.AddFailure(validator.Validate(ctx));

			return result;
		}
	}

	public class NullableStructPropertyValidator<T, TProperty> : PropertyValidator<T, TProperty>, IValidator
		where TProperty : struct
	{
		internal List<ValidatorBase<T, TProperty>> Validators { get; set; }
		internal Expression<Func<T, TProperty?>> Expression { get; }

		public NullableStructPropertyValidator(Expression<Func<T, TProperty?>> expression, ValidationFrame validationFrame, bool conditional)
			: base(PropertyAccessor.GetCachedAccessor(expression).ToNonGeneric(), conditional)
		{
			ValidationFrame = validationFrame ?? throw new ArgumentNullException(nameof(validationFrame));
			Validators = new List<ValidatorBase<T, TProperty>>();
			Expression = expression;
		}

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(ValidationFrame, ValidatorType, Conditional)
				.AddValidators(Validators);

		internal override ValidationResult Validate(ValidationContext validationContext)
		{
			var result = new ValidationResult();

			if (string.IsNullOrWhiteSpace(ValidationFrame.PropertyName))
				throw new InvalidOperationException($"{nameof(ValidationFrame)}.{nameof(ValidationFrame.PropertyName)} == null");

			var ctx = new SPropertyValidationContext<T, TProperty>(ValidationFrame, validationContext, validationContext.InstanceToValidate == null ? null : Func(validationContext.InstanceToValidate));

			foreach (var validator in Validators)
				result.AddFailure(validator.Validate(ctx));

			return result;
		}
	}
}
