using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class PrecisionScaleValidatorTest
	{
		private readonly ITestOutputHelper _output;

		public PrecisionScaleValidatorTest(ITestOutputHelper output)
		{
			_output = output ?? throw new ArgumentNullException(nameof(output));
			var validationMgr = new ValidationManager();
		}

		private IValidator RegisterAndGet<T>(Validator<T> validator)
		{
			var validationMgr = new ValidationManager();
			validationMgr.RegisterRulesFor<T, Command>(validator);
			var registeredValidator = validationMgr.GetRulesFor(typeof(T), typeof(Command));
			if (registeredValidator == null)
				throw new InvalidOperationException("validationRuleSet == null");

			return registeredValidator;
		}

		[Theory]
		[Trait("Category", "decimal")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, true)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
		public void DecimalNullable(ValidationValueType type, bool isValid)
		{
			var person = new Person();
			switch (type)
			{
				case ValidationValueType.Null:
					person.MyDecimalNullable = null;
					break;
				case ValidationValueType.Default:
					person.MyDecimalNullable = default(decimal);
					break;
				case ValidationValueType.Correct:
					person.MyDecimalNullable = 12.34m;
					break;
				case ValidationValueType.Incorrect:
					person.MyDecimalNullable = 12.345m;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDecimalNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.PrecisionScale, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "decimal")]
		//[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, true)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
		public void DecimalNotNull(ValidationValueType type, bool isValid)
		{
			var person = new Person();
			switch (type)
			{
				//case ValidationValueType.Null:
				//	person.MyDecimalNotNull = null;
				//	break;
				case ValidationValueType.Default:
					person.MyDecimalNotNull = default(decimal);
					break;
				case ValidationValueType.Correct:
					person.MyDecimalNotNull = 12.34m;
					break;
				case ValidationValueType.Incorrect:
					person.MyDecimalNotNull = 12.345m;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyDecimalNotNull, x => x.PrecisionScale(4, 2, false));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDecimalNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.PrecisionScale, result.Errors[0].Type);
			}
		}
	}
}
