using Raider.Extensions;
using Raider.Validation.Internal;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Raider.Validation
{
	public abstract class Validator : IValidator
	{
		public abstract bool Conditional { get; }
		public abstract ValidatorType ValidatorType { get; }
		internal Type CommandType { get; set; }
		internal abstract ValidationFrame ValidationFrame { get; }

		public abstract ValidationResult Validate(object? obj);

		internal Func<object, object>? Func { get; }

		ValidationFrame IValidator.ValidationFrame => ValidationFrame;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public Validator(Func<object, object>? func)
		{
			Func = func;
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public abstract IValidationDescriptor ToDescriptor();
	}



	public class Validator<T> : Validator, IValidator
	{
		public override bool Conditional { get; }
		public override ValidatorType ValidatorType { get; } = ValidatorType.NONE;
		internal override ValidationFrame ValidationFrame { get; }

		private List<IValidator> _validators;
		internal IReadOnlyList<IValidator> Validators => _validators;

		public Validator()
			: this(null)
		{
		}

		public Validator(Func<object, object>? func)
			: base(func)
		{
			ValidationFrame = new ValidationFrameRoot(typeof(T).FullName ?? "$ROOT");
			_validators = new List<IValidator>();
		}

		public Validator(Func<object, object>? func, ValidationFrame validationFrame, bool conditional)
			: base(func)
		{
			Conditional = conditional;
			ValidationFrame = validationFrame ?? throw new ArgumentNullException(nameof(validationFrame));
			_validators = new List<IValidator>();
		}

		internal void AddValidator(IValidator validator)
		{
			if (validator == null)
				throw new ArgumentNullException(nameof(validator));

			_validators.Add(validator);
		}

		internal Validator<T> SoftClone(Type commandType)
			=> new Validator<T>(Func)
			{
				CommandType = commandType ?? throw new ArgumentNullException(nameof(commandType)),
				_validators = _validators, //netreba klonovat list, klonuje sa len preto aby manager mohol pri registracii nastavit CommandType
			};

		//TODO: VALIDATE ASYNC !!! ???

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(ValidationFrame, ValidatorType, Conditional)
				.AddValidators(Validators);

		public override ValidationResult Validate(object? obj)
			=> Validate((T)obj);

		public virtual ValidationResult Validate(ValidationContext ctx)
		{
			if (ctx == null)
				throw new ArgumentNullException(nameof(ctx));

			var result = new ValidationResult();

			foreach (var validator in _validators)
			{
				if (validator is PropertyValidator<T> propertyValidator)
				{
					var propResult = propertyValidator.Validate(ctx);
					result.Merge(propResult);

					if (propResult.Interrupted)
						break;
				}

				else if (validator is ConditionalValidator<T> conditionalValidator)
				{
					if (!conditionalValidator.Condition.Invoke(ctx.InstanceToValidate))
						continue;

					var condResult = conditionalValidator.Validate(ctx);
					result.Merge(condResult);

					if (condResult.Interrupted)
						break;
				}

				else if (validator is INavigationValidator<T> navigationValidator)
				{
					var navObj = ctx.InstanceToValidate == null ? null : navigationValidator.Func?.Invoke(ctx.InstanceToValidate);
					var objResult = navigationValidator.Validate(new ValidationContext(navObj, ctx));
					result.Merge(objResult);

					if (objResult.Interrupted)
						break;
				}

				else if (validator is IEnumerableValidator<T> enumerableValidator)
				{
					var enumerableObj = ctx.InstanceToValidate == null ? null : enumerableValidator.Func?.Invoke(ctx.InstanceToValidate);
					var objResult = enumerableValidator.Validate(new ValidationContext(enumerableObj, ctx));
					result.Merge(objResult);

					if (objResult.Interrupted)
						break;
				}

				else if (validator is SwitchValidator<T> switchValidator)
				{
					ValidationResult condResult;

					if (switchValidator.Condition.Invoke(ctx.InstanceToValidate))
						condResult = switchValidator.True.Validate(ctx);
					else
						condResult = switchValidator.False.Validate(ctx);

					result.Merge(condResult);

					if (condResult.Interrupted)
						break;
				}

				else if (validator is ErrorValidator<T> errorValidator)
				{
					var objResult = errorValidator.Validate(ctx);
					result.Merge(objResult);

					if (objResult.Interrupted)
						break;
				}

				else
				{
					throw new NotSupportedException($"Invalid validator type {validator?.GetType().FullName ?? "null"}");
				}
			}

			return result;
		}

		public ValidationResult Validate(T? obj)
		{
			var result = new ValidationResult();
			var ctx = new ValidationContext(obj, null);
			return Validate(ctx);
		}

		public Validator<T> If(Func<T?, bool> condition, Action<Validator<T>> @true)
		{
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));
			if (@true == null)
				throw new ArgumentNullException(nameof(@true));

			var validator = new ConditionalValidator<T>(Func, ValidationFrame, condition.ToNonGeneric());
			AddValidator(validator);

			@true.Invoke(validator);
			return this;
		}

		public Validator<T> IfElse(Func<T?, bool> condition, Action<Validator<T>> @true, Action<Validator<T>> @false)
		{
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));
			if (@true == null)
				throw new ArgumentNullException(nameof(@true));
			if (@false == null)
				throw new ArgumentNullException(nameof(@false));

			var switchValidator = new SwitchValidator<T>(Func, ValidationFrame, condition.ToNonGeneric());
			AddValidator(switchValidator);

			@true.Invoke(switchValidator.True);
			@false.Invoke(switchValidator.False);
			return this;
		}

		public Validator<T> WithError(Func<T?, bool> condition, string errorMessage)
		{
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));
			if (string.IsNullOrWhiteSpace(errorMessage))
				throw new ArgumentNullException(nameof(errorMessage));

			var validator = new ErrorValidator<T>(Func, ValidationFrame, condition.ToNonGeneric(), errorMessage);
			AddValidator(validator);

			return this;
		}

		public Validator<T> WithPropertyError<TProperty>(Expression<Func<T, TProperty>> expression, Func<T?, bool> condition, string errorMessage)
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));
			if (string.IsNullOrWhiteSpace(errorMessage))
				throw new ArgumentNullException(nameof(errorMessage));

			var newValidationFrame = ValidationFrame.AddProperty(expression.GetMemberName());
			if (newValidationFrame == null)
				throw new InvalidOperationException($"Validator for {expression} already exists.");

			var getter = PropertyAccessor.GetCachedAccessor(expression);
			var validator = new ErrorValidator<T, TProperty>(getter.ToNonGeneric(), newValidationFrame, condition.ToNonGeneric(), errorMessage);
			AddValidator(validator);

			return this;
		}

		public Validator<T> ForProperty<TProperty>(Expression<Func<T, TProperty>> expression, Action<StructPropertyValidator<T, TProperty>> propertyValidator, Func<T?, bool>? condition = null)
			where TProperty : struct
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));
			if (propertyValidator == null)
				throw new ArgumentNullException(nameof(propertyValidator));

			var newValidationFrame = ValidationFrame.AddProperty(expression.GetMemberName());
			if (newValidationFrame == null)
				throw new InvalidOperationException($"Validator for {expression} already exists.");

			var propValidator = new StructPropertyValidator<T, TProperty>(expression, newValidationFrame, Conditional || condition != null);

			if (condition == null)
			{
				AddValidator(propValidator);
			}
			else
			{
				var condValidator = new ConditionalValidator<T>(Func, ValidationFrame, condition?.ToNonGeneric());
				AddValidator(condValidator);
				condValidator.AddValidator(propValidator);
			}

			propertyValidator.Invoke(propValidator);
			return this;
		}

		public Validator<T> ForProperty<TProperty>(Expression<Func<T, TProperty?>> expression, Action<NullableStructPropertyValidator<T, TProperty>> propertyValidator, Func<T?, bool>? condition = null)
			where TProperty : struct
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));
			if (propertyValidator == null)
				throw new ArgumentNullException(nameof(propertyValidator));

			var newValidationFrame = ValidationFrame.AddProperty(expression.GetMemberName());
			if (newValidationFrame == null)
				throw new InvalidOperationException($"Validator for {expression} already exists.");

			var propValidator = new NullableStructPropertyValidator<T, TProperty>(expression, newValidationFrame, Conditional || condition != null);

			if (condition == null)
			{
				AddValidator(propValidator);
			}
			else
			{
				var condValidator = new ConditionalValidator<T>(Func, ValidationFrame, condition?.ToNonGeneric());
				AddValidator(condValidator);
				condValidator.AddValidator(propValidator);
			}

			propertyValidator.Invoke(propValidator);
			return this;
		}

		public Validator<T> ForProperty(Expression<Func<T, string?>> expression, Action<ClassPropertyValidator<T, string>> propertyValidator, Func<T?, bool>? condition = null)
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));
			if (propertyValidator == null)
				throw new ArgumentNullException(nameof(propertyValidator));

			var newValidationFrame = ValidationFrame.AddProperty(expression.GetMemberName());
			if (newValidationFrame == null)
				throw new InvalidOperationException($"Validator for {expression} already exists.");

			var propValidator = new ClassPropertyValidator<T, string>(expression, newValidationFrame, Conditional || condition != null);

			if (condition == null)
			{
				AddValidator(propValidator);
			}
			else
			{
				var condValidator = new ConditionalValidator<T>(Func, ValidationFrame, condition?.ToNonGeneric());
				AddValidator(condValidator);
				condValidator.AddValidator(propValidator);
			}

			propertyValidator.Invoke(propValidator);
			return this;
		}

		public Validator<T> ForProperty<TProperty>(Expression<Func<T, TProperty?>> expression, Action<ClassPropertyValidator<T, TProperty>> propertyValidator, Func<T?, bool>? condition = null)
			where TProperty : class
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));
			if (propertyValidator == null)
				throw new ArgumentNullException(nameof(propertyValidator));

			var newValidationFrame = ValidationFrame.AddProperty(expression.GetMemberName());
			if (newValidationFrame == null)
				throw new InvalidOperationException($"Validator for {expression} already exists.");

			var propValidator = new ClassPropertyValidator<T, TProperty>(expression, newValidationFrame, Conditional || condition != null);

			if (condition == null)
			{
				AddValidator(propValidator);
			}
			else
			{
				var condValidator = new ConditionalValidator<T>(Func, ValidationFrame, condition?.ToNonGeneric());
				AddValidator(condValidator);
				condValidator.AddValidator(propValidator);
			}

			propertyValidator.Invoke(propValidator);
			return this;
		}

		public Validator<T> ForProperty<TItem>(Expression<Func<T, IEnumerable<TItem>?>> expression, Action<ClassPropertyValidator<T, IEnumerable<TItem>>> propertyValidator, Func<T?, bool>? condition = null)
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));
			if (propertyValidator == null)
				throw new ArgumentNullException(nameof(propertyValidator));

			var newValidationFrame = ValidationFrame.AddProperty(expression.GetMemberName());
			if (newValidationFrame == null)
				throw new InvalidOperationException($"Validator for {expression} already exists.");

			var propValidator = new ClassPropertyValidator<T, IEnumerable<TItem>>(expression, newValidationFrame, Conditional || condition != null);

			if (condition == null)
			{
				AddValidator(propValidator);
			}
			else
			{
				var condValidator = new ConditionalValidator<T>(Func, ValidationFrame, condition?.ToNonGeneric());
				AddValidator(condValidator);
				condValidator.AddValidator(propValidator);
			}

			propertyValidator.Invoke(propValidator);
			return this;
		}

		public Validator<T> ForNavigation<TProperty>(Expression<Func<T, TProperty?>> expression, Action<NavigationValidator<T, TProperty>> validator, Func<T?, bool>? condition = null)
			where TProperty : class
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));
			if (validator == null)
				throw new ArgumentNullException(nameof(validator));

			var newValidationFrame = ValidationFrame.AddNavigation(typeof(TProperty).FullName, expression.GetMemberName());
			if (newValidationFrame == null)
				throw new InvalidOperationException($"Validator for {expression} already exists.");

			var navigationValidator = new NavigationValidator<T, TProperty>(expression, newValidationFrame, Conditional || condition != null);

			if (condition == null)
			{
				AddValidator(navigationValidator);
			}
			else
			{
				var condValidator = new ConditionalValidator<T>(Func, ValidationFrame, condition?.ToNonGeneric());
				AddValidator(condValidator);
				condValidator.AddValidator(navigationValidator);
			}

			validator.Invoke(navigationValidator);
			return this;
		}

		public Validator<T> ForEach<TItem>(Expression<Func<T, IEnumerable<TItem>>> expression, Action<EnumerableValidator<T, TItem>> validator, Func<T?, bool>? condition = null)
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));
			if (validator == null)
				throw new ArgumentNullException(nameof(validator));

			var newValidationFrame = ValidationFrame.AddEnumeration(typeof(TItem).FullName, expression.GetMemberName());
			if (newValidationFrame == null)
				throw new InvalidOperationException($"Validator for {expression} already exists.");

			var enumerableValidator = new EnumerableValidator<T, TItem>(expression, newValidationFrame, Conditional || condition != null);

			if (condition == null)
			{
				AddValidator(enumerableValidator);
			}
			else
			{
				var condValidator = new ConditionalValidator<T>(Func, ValidationFrame, condition?.ToNonGeneric());
				AddValidator(condValidator);
				condValidator.AddValidator(enumerableValidator);
			}

			validator.Invoke(enumerableValidator);
			return this;
		}

		public NavigationValidator<T, TProperty> GetNavigateValidator<TProperty>(Expression<Func<T, TProperty?>> expression, bool setValidatorChain = true)
			where TProperty : class
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));

			var newValidationFrame = ValidationFrame.AddNavigation(typeof(TProperty).FullName, expression.GetMemberName());
			if (newValidationFrame == null)
				throw new InvalidOperationException($"Validator for {expression} already exists.");

			var navigationValidator = new NavigationValidator<T, TProperty>(expression, newValidationFrame, Conditional);
			
			if (setValidatorChain)
				AddValidator(navigationValidator);
			
			return navigationValidator;
		}

		public EnumerableValidator<T, TItem> GetEnumerableValidator<TItem>(Expression<Func<T, IEnumerable<TItem>>> expression, bool setValidatorChain = true)
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));

			var newValidationFrame = ValidationFrame.AddEnumeration(typeof(TItem).FullName, expression.GetMemberName());
			if (newValidationFrame == null)
				throw new InvalidOperationException($"Validator for {expression} already exists.");

			var enumerableValidator = new EnumerableValidator<T, TItem>(expression, newValidationFrame, Conditional);

			if (setValidatorChain)
				AddValidator(enumerableValidator);

			return enumerableValidator;
		}
	}
}
