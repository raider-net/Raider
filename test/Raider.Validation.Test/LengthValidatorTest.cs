using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class LengthValidatorTest
	{
		private readonly ITestOutputHelper _output;

		public LengthValidatorTest(ITestOutputHelper output)
		{
			_output = output ?? throw new ArgumentNullException(nameof(output));
		}

		[Theory]
		[Trait("Category", "string")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Empty, true)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
		public void StringNullable(ValidationValueType type, bool isValid)
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
					person.MyStringNullable = "1234";
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyStringNullable, x => x.Length(0, 3));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.Length, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "string")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Empty, true)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
		public void StringNotNull(ValidationValueType type, bool isValid)
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
					person.MyStringNotNull = "1234";
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyStringNotNull, x => x.Length(0, 3));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.Length, result.Errors[0].Type);
			}
		}
	}
}
