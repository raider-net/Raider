using Raider.Extensions;
using Raider.Validation.Internal;
using System;
using System.Linq.Expressions;

namespace Raider.Validation
{
	internal interface INavigationValidator<T> { }

	internal class NavigationValidator<T, TProperty> : Validator<TProperty>, INavigationValidator<T>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.NavigationValidator;
		public override bool Conditional { get; }

		public NavigationValidator(Expression<Func<T, TProperty?>> expression, ValidationFrame validationFrame, bool conditional, Func<object?, string>? detailInfoFunc)
			: base(PropertyAccessor.GetCachedAccessor(expression).ToNonGeneric(), validationFrame, conditional, null, detailInfoFunc)
		{
		}

		internal override ValidationResult Validate(ValidationContext ctx)
		{
			var result = new ValidationResult();

			var itemResult = base.Validate(ctx);
			result.Merge(itemResult);

			return result;
		}
	}
}
