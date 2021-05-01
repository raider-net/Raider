using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class WithPropertyErrorValidatorTest
	{
		private readonly ITestOutputHelper _output;
		private string _errorMessage = "TEST_ERROR";

		public WithPropertyErrorValidatorTest(ITestOutputHelper output)
		{
			_output = output ?? throw new ArgumentNullException(nameof(output));
		}

		[Theory]
		[Trait("Category", "int")]
		[InlineData(true)]
		[InlineData(false)]
		public void IntNullable(bool condition)
		{
			var person = new Person();

			var validator = Validator<Person>.Rules()
					.WithPropertyError(x=> x.MyIntNullable, x => condition, _errorMessage);

			var result = validator.Validate(person);

			if (condition)
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyIntNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.ErrorProperty, result.Errors[0].Type);
				Assert.Equal(_errorMessage, result.Errors[0].Message);
			}
			else
			{
				Assert.Equal(0, result.Errors.Count);
			}
		}

		[Theory]
		[Trait("Category", "decimal")]
		[InlineData(true)]
		[InlineData(false)]
		public void DecimalNullable(bool condition)
		{
			var person = new Person();

			var validator = Validator<Person>.Rules()
					.WithPropertyError(x => x.MyDecimalNullable, x => condition, _errorMessage);

			var result = validator.Validate(person);

			if (condition)
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDecimalNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.ErrorProperty, result.Errors[0].Type);
				Assert.Equal(_errorMessage, result.Errors[0].Message);
			}
			else
			{
				Assert.Equal(0, result.Errors.Count);
			}
		}

		[Theory]
		[Trait("Category", "bool")]
		[InlineData(true)]
		[InlineData(false)]
		public void BoolNullable(bool condition)
		{
			var person = new Person();

			var validator = Validator<Person>.Rules()
					.WithPropertyError(x => x.MyBoolNullable, x => condition, _errorMessage);

			var result = validator.Validate(person);

			if (condition)
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyBoolNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.ErrorProperty, result.Errors[0].Type);
				Assert.Equal(_errorMessage, result.Errors[0].Message);
			}
			else
			{
				Assert.Equal(0, result.Errors.Count);
			}
		}

		[Theory]
		[Trait("Category", "DateTime")]
		[InlineData(true)]
		[InlineData(false)]
		public void DateTimeNullable(bool condition)
		{
			var person = new Person();

			var validator = Validator<Person>.Rules()
					.WithPropertyError(x => x.MyDateTimeNullable, x => condition, _errorMessage);

			var result = validator.Validate(person);

			if (condition)
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDateTimeNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.ErrorProperty, result.Errors[0].Type);
				Assert.Equal(_errorMessage, result.Errors[0].Message);
			}
			else
			{
				Assert.Equal(0, result.Errors.Count);
			}
		}

		[Theory]
		[Trait("Category", "Guid")]
		[InlineData(true)]
		[InlineData(false)]
		public void GuidNullable(bool condition)
		{
			var person = new Person();

			var validator = Validator<Person>.Rules()
					.WithPropertyError(x => x.MyGuidNullable, x => condition, _errorMessage);

			var result = validator.Validate(person);

			if (condition)
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyGuidNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.ErrorProperty, result.Errors[0].Type);
				Assert.Equal(_errorMessage, result.Errors[0].Message);
			}
			else
			{
				Assert.Equal(0, result.Errors.Count);
			}
		}

		[Theory]
		[Trait("Category", "string")]
		[InlineData(true)]
		[InlineData(false)]
		public void StringNullable(bool condition)
		{
			var person = new Person();

			var validator = Validator<Person>.Rules()
					.WithPropertyError(x => x.MyStringNullable, x => condition, _errorMessage);

			var result = validator.Validate(person);

			if (condition)
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.ErrorProperty, result.Errors[0].Type);
				Assert.Equal(_errorMessage, result.Errors[0].Message);
			}
			else
			{
				Assert.Equal(0, result.Errors.Count);
			}
		}

		[Theory]
		[Trait("Category", "object")]
		[InlineData(true)]
		[InlineData(false)]
		public void ObjectNullable(bool condition)
		{
			var person = new Person();

			var validator = Validator<Person>.Rules()
					.WithPropertyError(x => x.ANullable, x => condition, _errorMessage);

			var result = validator.Validate(person);

			if (condition)
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.ANullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.ErrorProperty, result.Errors[0].Type);
				Assert.Equal(_errorMessage, result.Errors[0].Message);
			}
			else
			{
				Assert.Equal(0, result.Errors.Count);
			}
		}

		[Theory]
		[Trait("Category", "enumerable")]
		[InlineData(true)]
		[InlineData(false)]
		public void EnumerableNullable(bool condition)
		{
			var person = new Person();

			var validator = Validator<Person>.Rules()
					.WithPropertyError(x => x.MyAddressesNullable, x => condition, _errorMessage);

			var result = validator.Validate(person);

			if (condition)
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyAddressesNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.ErrorProperty, result.Errors[0].Type);
				Assert.Equal(_errorMessage, result.Errors[0].Message);
			}
			else
			{
				Assert.Equal(0, result.Errors.Count);
			}
		}
	}
}
