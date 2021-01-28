using Raider.Extensions;
using Raider.Validation.Internal;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Raider.Validation
{
	public interface IEnumerableValidator<T>
	{
		internal Func<object, object>? Func { get; }
		internal ValidationFrame ValidationFrame { get; }
		ValidationResult Validate(ValidationContext ctx);
	}

	public class EnumerableValidator<T, TItem> : Validator<TItem>, IValidator, IEnumerableValidator<T>
	{
		public override bool Conditional { get; }
		public override ValidatorType ValidatorType { get; } = ValidatorType.NONE;
		Func<object, object>? IEnumerableValidator<T>.Func => Func;
		ValidationFrame IEnumerableValidator<T>.ValidationFrame => ValidationFrame;

		internal Expression<Func<T, IEnumerable<TItem>>> Expression { get; }

		public EnumerableValidator(Expression<Func<T, IEnumerable<TItem>>> expression, ValidationFrame validationFrame, bool conditional)
			: base(PropertyAccessor.GetCachedAccessor(expression).ToNonGeneric(), validationFrame, conditional)
		{
			Expression = expression;
		}

		public override ValidationResult Validate(ValidationContext ctx)
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
