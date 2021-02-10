using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class NavigationValidatorTest
	{
		private readonly ITestOutputHelper _output;

		public NavigationValidatorTest(ITestOutputHelper output)
		{
			_output = output ?? throw new ArgumentNullException(nameof(output));
		}

		[Theory]
		[Trait("Category", "object")]
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Empty, false)]
		[InlineData(ValidationValueType.Correct, true)]
		public void ObjectNullable(ValidationValueType type, bool isValid)
		{
			var person = new Person();
			switch (type)
			{
				case ValidationValueType.Null:
					person.ANullable = null;
					break;
				case ValidationValueType.Empty:
					person.ANullable = new A();
					break;
				case ValidationValueType.Correct:
					person.ANullable = new A { AStringNullable = "test" };
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForNavigation(x => x.ANullable, x => x.ForProperty(p => p.AStringNullable, v => v.EqualsTo("test")));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.ANullable.AStringNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.Equal, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "object")]
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Empty, false)]
		[InlineData(ValidationValueType.Correct, true)]
		public void ObjectNotNull(ValidationValueType type, bool isValid)
		{
			var person = new Person();
			switch (type)
			{
				case ValidationValueType.Null:
					person.ANotNull = null;
					break;
				case ValidationValueType.Empty:
					person.ANotNull = new A();
					break;
				case ValidationValueType.Correct:
					person.ANotNull = new A { AStringNullable = "test" };
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForNavigation(x => x.ANotNull, x => x.ForProperty(p => p.AStringNullable, v => v.EqualsTo("test")));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.ANotNull.AStringNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.Equal, result.Errors[0].Type);
			}
		}
	}
}
