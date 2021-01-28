using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class RegExValidatorTest
	{
		private readonly ITestOutputHelper _output;

		public RegExValidatorTest(ITestOutputHelper output)
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
		[Trait("Category", "string")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Empty, false)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
		public void StringNullable_RegEx(ValidationValueType type, bool isValid)
		{
			var person = new Person();
			switch (type)
			{
				case ValidationValueType.Null:
					person.MyStringNullable = null;
					break;
				case ValidationValueType.Empty:
					person.MyStringNullable = "";
					break;
				case ValidationValueType.Correct:
					person.MyStringNullable = "k";
					break;
				case ValidationValueType.Incorrect:
					person.MyStringNullable = "8";
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyStringNullable, x => x.RegEx("[a-z]+"));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.RegEx, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "string")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Empty, false)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
		public void StringNotNull_RegEx(ValidationValueType type, bool isValid)
		{
			var person = new Person();
			switch (type)
			{
				case ValidationValueType.Null:
					person.MyStringNotNull = null;
					break;
				case ValidationValueType.Empty:
					person.MyStringNotNull = "";
					break;
				case ValidationValueType.Correct:
					person.MyStringNotNull = "k";
					break;
				case ValidationValueType.Incorrect:
					person.MyStringNotNull = "8";
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyStringNotNull, x => x.RegEx("[a-z]+"));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.RegEx, result.Errors[0].Type);
			}
		}
	}
}
