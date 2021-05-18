using Raider.Validation.Test.Model;
using System;

namespace Raider.Validation.Test.Validators
{
	[ValidationRegister(typeof(TestCommand))]
	public class BarProfileItemValidator : ValidationBuilder<CItem>
	{
		public bool Condition { get; set; }

		public BarProfileItemValidator Configure(bool condition)
		{
			Condition = condition;
			return this;
		}

		public override Validator<CItem> BuildRules(IServiceProvider serviceProvider, object? state = null)
			=> Validator<CItem>.Rules()
				.ForProperty(x => x.CItemStringNullable, x => x.NotDefaultOrEmpty().NotNull())
				.ForProperty(x => x.CItemIntNullable, x => x
					.EqualsTo(5)
					.NotEqualsTo(10)
					.ExclusiveBetween(4, 6)
					.InclusiveBetween(5, 5)
					.GreaterThanOrEqual(15)
					.GreaterThan(14)
					.LessThanOrEqual(5)
					.LessThan(6), c => Condition)
				.ForProperty(x => x.CItemDateTimeNullable, x => x.NotDefaultOrEmpty(), c => Condition)
				.ForProperty(x => x.CItemDecimalNullable, x => x.PrecisionScale(4, 2, false), c => Condition);
	}
}
