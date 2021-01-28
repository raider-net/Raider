using Raider.Extensions;
using Raider.Validation.Internal;
using System;
using System.Linq.Expressions;

namespace Raider.Validation
{
	public interface INavigationValidator<T>
	{
		internal Func<object, object>? Func { get; }
		internal ValidationFrame ValidationFrame { get; }
		ValidationResult Validate(ValidationContext ctx);
	}

	public class NavigationValidator<T, TProperty> : Validator<TProperty>, IValidator, INavigationValidator<T>
	{
		public override bool Conditional { get; }
		public override ValidatorType ValidatorType { get; } = ValidatorType.NONE;
		Func<object, object>? INavigationValidator<T>.Func => Func;
		ValidationFrame INavigationValidator<T>.ValidationFrame => ValidationFrame;

		internal Expression<Func<T, TProperty?>> Expression { get; }

		public NavigationValidator(Expression<Func<T, TProperty?>> expression, ValidationFrame validationFrame, bool conditional)
			: base(PropertyAccessor.GetCachedAccessor(expression).ToNonGeneric(), validationFrame, conditional)
		{
			Expression = expression;
		}

		public override ValidationResult Validate(ValidationContext ctx)
		{
			var result = new ValidationResult();

			var itemResult = base.Validate(ctx);
			result.Merge(itemResult);

			return result;
		}
	}
}
