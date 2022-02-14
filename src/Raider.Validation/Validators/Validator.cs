using Raider.Extensions;
using Raider.Validation.Client;
using Raider.Validation.Internal;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Raider.Validation
{
	public abstract class ValidatorBase
	{
		public abstract bool Conditional { get; }
		public abstract IClientConditionDefinition? ClientConditionDefinition { get; }
		public abstract ValidatorType ValidatorType { get; }
		//internal abstract ValidationFrame ValidationFrame { get; }
		internal Func<object, object>? Func { get; }
		internal Func<object?, string>? DetailInfoFunc { get; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public ValidatorBase(Func<object, object>? func, Func<object?, string>? detailInfoFunc)
		{
			Func = func;
			DetailInfoFunc = detailInfoFunc;
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public abstract IValidationDescriptor ToDescriptor();

		protected abstract ValidationResult Validate(object? obj);

		internal abstract ValidationResult? Validate(ValidationContext ctx);

		internal abstract void UpdateValidationFrame(ValidationFrame validationFrame);
	}

	public class Validator<T> : ValidatorBase, IValidator<T>
	{
		public override bool Conditional { get; }
		public override IClientConditionDefinition? ClientConditionDefinition { get; }
		public override ValidatorType ValidatorType { get; } = ValidatorType.Validator;
		internal ValidationFrame ValidationFrame { get; private set; }

		private List<ValidatorBase> _validators;
		internal IReadOnlyList<ValidatorBase> Validators => _validators;

		private Validator(Func<object, object>? func, ValidationFrame? validationFrame, ValidatorType validatorType, bool conditional)
			: base(func, null)
		{
			ValidationFrame = validationFrame ?? new ValidationFrameRoot(typeof(T).FullName ?? "$ROOT");
			_validators = new List<ValidatorBase>();
			ValidatorType = validatorType;
			Conditional = conditional;
		}

		internal Validator(Func<object, object>? func, ValidationFrame validationFrame, bool conditional, IClientConditionDefinition? clientConditionDefinition, Func<object?, string>? detailInfoFunc)
			: base(func, detailInfoFunc)
		{
			Conditional = conditional;
			ClientConditionDefinition = clientConditionDefinition;
			ValidationFrame = validationFrame ?? throw new ArgumentNullException(nameof(validationFrame));
			_validators = new List<ValidatorBase>();
		}

		public static Validator<T> Rules()
		{
			return new Validator<T>(null, null, ValidatorType.Validator, false);
		}

		public virtual Validator<T> AttachTo(Validator<T> parent)
		{
			if (this is not Validator<T>)
				throw new InvalidOperationException($"Applicable only for {nameof(Validator<T>)}.");

			foreach (var validator in _validators)
			{
				validator.UpdateValidationFrame(parent.ValidationFrame);
				parent._validators.Add(validator);
			}

			return this;
		}

		internal override void UpdateValidationFrame(ValidationFrame validationFrame)
		{
			//if (ValidationFrame.Parent == null)
			//{
			//	ValidationFrame = validationFrame;
			//}
			//else
			//{
			//	ValidationFrame.SetParent(validationFrame);
			//}

			ValidationFrame.SetParent(validationFrame, this is ErrorValidator<T>);
		}

		internal void AddValidator(ValidatorBase validator)
		{
			if (validator == null)
				throw new ArgumentNullException(nameof(validator));

			_validators.Add(validator);
		}

		//TODO: VALIDATE ASYNC !!! ???

		public override IValidationDescriptor ToDescriptor()
			=> new ValidationDescriptor(typeof(T), ValidationFrame, ValidatorType, GetType().ToFriendlyFullName(), Conditional, ClientConditionDefinition, null, null)
				.AddValidators(Validators);

		protected override ValidationResult Validate(object? obj)
			=> Validate((T)obj);

		internal override ValidationResult? Validate(ValidationContext ctx)
		{
			if (ctx == null)
				throw new ArgumentNullException(nameof(ctx));

			var result = new ValidationResult();

			var propertyValidatorType = typeof(PropertyValidator<,>);
			foreach (var validator in _validators)
			{
				if (validator is IPropertyValidator<T>)
				{
					var propResult = validator.Validate(ctx);
					result.Merge(propResult);

					if (propResult.Interrupted)
						break;
				}

				else if (validator is ConditionalValidator<T> conditionalValidator)
				{
					if (conditionalValidator.Condition != null)
					{
						if (!conditionalValidator.Condition.Invoke(ctx.InstanceToValidate))
							continue;
					}
					else if (conditionalValidator.ClientConditionDefinition != null)
					{
						if (!conditionalValidator.ClientConditionDefinition.Execute(ctx.InstanceToValidate))
							continue;
					}

					var condResult = conditionalValidator.Validate(ctx);
					result.Merge(condResult);

					if (condResult.Interrupted)
						break;
				}

				else if (validator is INavigationValidator<T> navigationValidator)
				{
					var navObj = ctx.InstanceToValidate == null ? null : validator.Func?.Invoke(ctx.InstanceToValidate);
					var objResult = validator.Validate(new ValidationContext(navObj, ctx));
					result.Merge(objResult);

					if (objResult.Interrupted)
						break;
				}

				else if (validator is IEnumerableValidator<T> enumerableValidator)
				{
					var enumerableObj = ctx.InstanceToValidate == null ? null : validator.Func?.Invoke(ctx.InstanceToValidate);
					var objResult = validator.Validate(new ValidationContext(enumerableObj, ctx));
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
			var ctx = new ValidationContext(obj, null);
			return Validate(ctx);
		}

		public Validator<T> If(Func<T?, bool> condition, Action<Validator<T>> @true, Func<T?, string>? detailInfoFunc = null)
		{
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));
			if (@true == null)
				throw new ArgumentNullException(nameof(@true));

			var validator = new ConditionalValidator<T>(Func, ValidationFrame, condition.ToNonGeneric(), null, detailInfoFunc?.ToNonGeneric());
			AddValidator(validator);

			@true.Invoke(validator);
			return this;
		}

		public Validator<T> IfElse(Func<T?, bool> condition, Action<Validator<T>> @true, Action<Validator<T>> @false, Func<T?, string>? detailInfoFunc = null)
		{
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));
			if (@true == null)
				throw new ArgumentNullException(nameof(@true));
			if (@false == null)
				throw new ArgumentNullException(nameof(@false));

			var switchValidator = new SwitchValidator<T>(Func, ValidationFrame, condition.ToNonGeneric(), detailInfoFunc?.ToNonGeneric());
			AddValidator(switchValidator);

			@true.Invoke(switchValidator.True);
			@false.Invoke(switchValidator.False);
			return this;
		}

		public Validator<T> WithError(Func<T?, bool> condition, string errorMessage, Func<T?, string>? detailInfoFunc = null)
		{
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));
			if (string.IsNullOrWhiteSpace(errorMessage))
				throw new ArgumentNullException(nameof(errorMessage));

			var validator = new ErrorValidator<T>(Func, ValidationFrame, condition.ToNonGeneric(), errorMessage, detailInfoFunc?.ToNonGeneric());
			AddValidator(validator);

			return this;
		}

		public Validator<T> WithPropertyError<TProperty>(Expression<Func<T, TProperty>> expression, Func<T?, bool> condition, string errorMessage, Func<T?, string>? detailInfoFunc = null)
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));
			if (string.IsNullOrWhiteSpace(errorMessage))
				throw new ArgumentNullException(nameof(errorMessage));

			var newValidationFrame = ValidationFrame.AddProperty(typeof(T).FullName, expression.GetMemberName());

			var getter = PropertyAccessor.GetCachedAccessor(expression);
			var validator = new ErrorValidator<T, TProperty>(getter.ToNonGeneric(), newValidationFrame, condition.ToNonGeneric(), errorMessage, detailInfoFunc?.ToNonGeneric());
			AddValidator(validator);

			return this;
		}

		public Validator<T> ForProperty<TProperty>(
			Expression<Func<T, TProperty>> expression,
			Action<PropertyValidator<T, TProperty>> propertyValidator,
			Func<T?, bool>? condition = null,
			Func<ClientCondition<T>, IClientConditionDefinition>? clientCondition = null,
			Func<T?, string>? detailInfoFunc = null)
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));
			if (propertyValidator == null)
				throw new ArgumentNullException(nameof(propertyValidator));

			var newValidationFrame = ValidationFrame.AddProperty(typeof(T).FullName, expression.GetMemberName());

			var cc = new ClientCondition<T>();
			var clientConditionDefinition = clientCondition?.Invoke(cc);

			var propValidator = new PropertyValidator<T, TProperty>(PropertyAccessor.GetCachedAccessor(expression).ToNonGeneric(), newValidationFrame, condition != null, clientConditionDefinition, detailInfoFunc?.ToNonGeneric());

			if (condition == null && clientConditionDefinition == null)
			{
				AddValidator(propValidator);
			}
			else
			{
				var condValidator = new ConditionalValidator<T>(Func, newValidationFrame, condition?.ToNonGeneric(), clientConditionDefinition, detailInfoFunc?.ToNonGeneric());
				AddValidator(condValidator);
				condValidator.AddValidator(propValidator);
			}

			propertyValidator.Invoke(propValidator);
			return this;
		}

		public Validator<T> ForNavigation<TProperty>(Expression<Func<T, TProperty?>> expression, Action<Validator<TProperty>> validator, Func<T?, bool>? condition = null, Func<T?, string>? detailInfoFunc = null)
			where TProperty : class
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));
			if (validator == null)
				throw new ArgumentNullException(nameof(validator));

			var newValidationFrame = ValidationFrame.AddNavigation(typeof(TProperty).FullName, expression.GetMemberName());

			var navigationValidator = new NavigationValidator<T, TProperty>(expression, newValidationFrame, condition != null, detailInfoFunc?.ToNonGeneric());

			if (condition == null)
			{
				AddValidator(navigationValidator);
			}
			else
			{
				var condValidator = new ConditionalValidator<T>(Func, newValidationFrame, condition?.ToNonGeneric(), null, detailInfoFunc?.ToNonGeneric());
				AddValidator(condValidator);
				condValidator.AddValidator(navigationValidator);
			}

			validator.Invoke(navigationValidator);
			return this;
		}

		public Validator<T> ForEach<TItem>(Expression<Func<T, IEnumerable<TItem>>> expression, Action<Validator<TItem>> validator, Func<T?, bool>? condition = null, Func<T?, string>? detailInfoFunc = null)
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));
			if (validator == null)
				throw new ArgumentNullException(nameof(validator));

			var newValidationFrame = ValidationFrame.AddEnumeration(typeof(TItem).FullName, expression.GetMemberName());

			var enumerableValidator = new EnumerableValidator<T, TItem>(expression, newValidationFrame, condition != null, detailInfoFunc?.ToNonGeneric());

			if (condition == null)
			{
				AddValidator(enumerableValidator);
			}
			else
			{
				var condValidator = new ConditionalValidator<T>(Func, newValidationFrame, condition?.ToNonGeneric(), null, detailInfoFunc?.ToNonGeneric());
				AddValidator(condValidator);
				condValidator.AddValidator(enumerableValidator);
			}

			validator.Invoke(enumerableValidator);
			return this;
		}

		public Validator<TProperty> GetNavigateValidator<TProperty>(Expression<Func<T, TProperty?>> expression, bool setValidatorChain = true, Func<T?, string>? detailInfoFunc = null)
			where TProperty : class
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));

			var newValidationFrame = ValidationFrame.AddNavigation(typeof(TProperty).FullName, expression.GetMemberName());

			var navigationValidator = new NavigationValidator<T, TProperty>(expression, newValidationFrame, Conditional, detailInfoFunc?.ToNonGeneric());

			if (setValidatorChain)
				AddValidator(navigationValidator);

			return navigationValidator;
		}

		public Validator<TItem> GetEnumerableValidator<TItem>(Expression<Func<T, IEnumerable<TItem>>> expression, bool setValidatorChain = true, Func<T?, string>? detailInfoFunc = null)
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));

			var newValidationFrame = ValidationFrame.AddEnumeration(typeof(TItem).FullName, expression.GetMemberName());

			var enumerableValidator = new EnumerableValidator<T, TItem>(expression, newValidationFrame, Conditional, detailInfoFunc?.ToNonGeneric());

			if (setValidatorChain)
				AddValidator(enumerableValidator);

			return enumerableValidator;
		}

		public override string ToString()
		{
			return $"{ValidatorType}<{typeof(T).FullName?.GetLastSplitSubstring(".")}> | {ValidationFrame} | Conditional={Conditional} | Validators={Validators.Count}";
		}
	}
}
