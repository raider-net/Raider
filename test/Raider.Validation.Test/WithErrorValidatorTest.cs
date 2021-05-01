using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class WithErrorValidatorTest
	{
		private readonly ITestOutputHelper _output;
		private string _errorMessage = "TEST_ERROR";

		public WithErrorValidatorTest(ITestOutputHelper output)
		{
			_output = output ?? throw new ArgumentNullException(nameof(output));
		}

		[Theory]
		[Trait("Category", "object")]
		[InlineData(true)]
		[InlineData(false)]
		public void ObjectNullable(bool condition)
		{
			var person = new Person();

			var validator = Validator<Person>.Rules()
					.WithError(x => condition, _errorMessage);

			var result = validator.Validate(person);

			if (condition)
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.ErrorObject, result.Errors[0].Type);
				Assert.Equal(_errorMessage, result.Errors[0].Message);
			}
			else
			{
				Assert.Equal(0, result.Errors.Count);
			}
		}
	}
}
