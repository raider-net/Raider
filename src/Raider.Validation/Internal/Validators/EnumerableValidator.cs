using Raider.Extensions;
using Raider.Validation.Internal;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Raider.Validation
{
	internal interface IEnumerableValidator<T> { }

	internal class EnumerableValidator<T, TItem> : Validator<TItem>, IEnumerableValidator<T>
	{
		public override ValidatorType ValidatorType { get; } = ValidatorType.EnumerableValidator;
		public override bool Conditional { get; }

		internal Expression<Func<T, IEnumerable<TItem>>> Expression { get; }

		public EnumerableValidator(Expression<Func<T, IEnumerable<TItem>>> expression, ValidationFrame validationFrame, bool conditional)
			: base(PropertyAccessor.GetCachedAccessor(expression).ToNonGeneric(), validationFrame, conditional)
		{
			Expression = expression;
		}

		internal override ValidationResult Validate(ValidationContext ctx)
		{
			var result = new ValidationResult();

#pragma warning disable CS8604 // Possible null reference argument.
			var enumeration = (IEnumerable<TItem>?)ctx.InstanceToValidate;
#pragma warning restore CS8604 // Possible null reference argument.

			if (enumeration != null)
			{
				using (var enumerator = enumeration.GetEnumerator())
				{
					int index = -1;
					while (enumerator.MoveNext())
					{
						index++;

						var obj = enumerator.Current;

						var newCtx = new ValidationContext(obj, ctx);
						newCtx.Indexes[ValidationFrame.Depth] = index;
						var itemResult = base.Validate(newCtx);
						result.Merge(itemResult);
					}
				}
			}

			return result;
		}
	}
}
