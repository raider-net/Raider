using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class ErrorValidatorTest
	{
		private readonly ITestOutputHelper _output;

		public ErrorValidatorTest(ITestOutputHelper output)
		{
			_output = output ?? throw new ArgumentNullException(nameof(output));
		}

		[Fact]
		[Trait("Category", "base")]
		public void Base_ObjectError()
		{
			var validator = Validator<Person>.Rules()
					.WithError(x => x?.MyStringNullable == null, "E001");

			var result = validator.Validate(null);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.ErrorObject, result.Errors[0].Type);
			Assert.Equal("E001", result.Errors[0].Message);
		}


		[Fact]
		[Trait("Category", "base")]
		public void Base_PropertyError()
		{
			var validator = Validator<Person>.Rules()
					.WithPropertyError(x => x.MyDateTimeNotNull, x => x?.MyStringNullable == null, "E001");

			var result = validator.Validate(null);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_.MyDateTimeNotNull", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.ErrorProperty, result.Errors[0].Type);
			Assert.Equal("E001", result.Errors[0].Message);
		}


		[Fact]
		[Trait("Category", "base")]
		public void Base_NavigationError()
		{
			var validator = Validator<Person>.Rules()
				.ForNavigation(x => x.ANotNull, x => x.WithPropertyError(e => e.ADecimalNotNull, c => true, "E001"));

			var result = validator.Validate(null);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_.ANotNull.ADecimalNotNull", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.ErrorProperty, result.Errors[0].Type);
			Assert.Equal("E001", result.Errors[0].Message);
		}


		[Fact]
		[Trait("Category", "base")]
		public void Base_EnumerableError()
		{
			var validator = Validator<Person>.Rules()
				.ForNavigation(x => x.MyAddressesNotNull, x => x.WithError(c => true, "E001"));

			var result = validator.Validate(null);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_.MyAddressesNotNull", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.ErrorObject, result.Errors[0].Type);
			Assert.Equal("E001", result.Errors[0].Message);
		}











		[Fact]
		[Trait("Category", "object")]
		public void ObjectError()
		{
			var person = new Person();

			var validator = Validator<Person>.Rules()
					.WithError(x => x?.MyStringNullable == null, "E001");

			var result = validator.Validate(person);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.ErrorObject, result.Errors[0].Type);
			Assert.Equal("E001", result.Errors[0].Message);
		}


		[Fact]
		[Trait("Category", "property")]
		public void PropertyError()
		{
			var person = new Person();

			var validator = Validator<Person>.Rules()
					.WithPropertyError(x => x.MyDateTimeNotNull, x => x?.MyStringNullable == null, "E001");

			var result = validator.Validate(person);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_.MyDateTimeNotNull", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.ErrorProperty, result.Errors[0].Type);
			Assert.Equal("E001", result.Errors[0].Message);
		}


		[Fact]
		[Trait("Category", "navigation")]
		public void NavigationError()
		{
			var person = new Person();

			var validator = Validator<Person>.Rules()
				.ForNavigation(x => x.ANotNull, x => x.WithPropertyError(e => e.ADecimalNotNull, c => true, "E001"));

			var result = validator.Validate(person);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_.ANotNull.ADecimalNotNull", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.ErrorProperty, result.Errors[0].Type);
			Assert.Equal("E001", result.Errors[0].Message);
		}


		[Fact]
		[Trait("Category", "enumerable")]
		public void EnumerableError()
		{
			var person = new Person();

			var validator = Validator<Person>.Rules()
				.ForNavigation(x => x.MyAddressesNotNull, x => x.WithError(c => true, "E001"));

			var result = validator.Validate(person);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_.MyAddressesNotNull", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.ErrorObject, result.Errors[0].Type);
			Assert.Equal("E001", result.Errors[0].Message);
		}


		[Fact]
		[Trait("Category", "enumerable")]
		public void ForEachEnumerableError()
		{
			var person = new Person
			{
				MyAddressesNotNull = new System.Collections.Generic.List<Address>
				{
					new Address(),
					new Address(),
				}
			};

			var validator = Validator<Person>.Rules()
				.ForEach(x => x.MyAddressesNotNull, x => x.WithPropertyError(e => e.AddDecimalNotNull, c => true, "E001"));

			var result = validator.Validate(person);

			Assert.Equal(2, result.Errors.Count);
			Assert.Equal("_.MyAddressesNotNull[0].AddDecimalNotNull", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.ErrorProperty, result.Errors[0].Type);
			Assert.Equal("E001", result.Errors[0].Message);

			Assert.Equal("_.MyAddressesNotNull[1].AddDecimalNotNull", result.Errors[1].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.ErrorProperty, result.Errors[1].Type);
			Assert.Equal("E001", result.Errors[1].Message);
		}
	}
}
