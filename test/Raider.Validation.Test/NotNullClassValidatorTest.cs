﻿using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class NotNullClassValidatorTest
	{
		private readonly ITestOutputHelper _output;

		public NotNullClassValidatorTest(ITestOutputHelper output)
		{
			_output = output ?? throw new ArgumentNullException(nameof(output));
		}

		[Fact]
		[Trait("Category", "base")]
		public void Base_StringNullable()
		{
			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyStringNullable, x => x.NotNull());

			var result = validator.Validate(null);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_.MyStringNullable", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.NotNull, result.Errors[0].Type);
		}

		[Theory]
		[Trait("Category", "string")]
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Empty, true)]
		[InlineData(ValidationValueType.Correct, true)]
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
					person.MyStringNullable = "test";
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyStringNullable, x => x.NotNull());

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotNull, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "string")]
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Empty, true)]
		[InlineData(ValidationValueType.Correct, true)]
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
					person.MyStringNotNull = "test";
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyStringNotNull, x => x.NotNull());

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotNull, result.Errors[0].Type);
			}
		}

		[Fact]
		[Trait("Category", "base")]
		public void Base_ObjectNullable()
		{
			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.ANullable, x => x.NotNull());

			var result = validator.Validate(null);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_.ANullable", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.NotNull, result.Errors[0].Type);
		}

		[Theory]
		[Trait("Category", "object")]
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Empty, true)]
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
					person.ANullable = new A();
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.ANullable, x => x.NotNull());

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.ANullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotNull, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "object")]
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Empty, true)]
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
					person.ANotNull = new A();
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.ANotNull, x => x.NotNull());

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.ANotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotNull, result.Errors[0].Type);
			}
		}

		[Fact]
		[Trait("Category", "base")]
		public void Base_EnumerabletNullable()
		{
			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyAddressesNullable, x => x.NotNull());

			var result = validator.Validate(null);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_.MyAddressesNullable", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.NotNull, result.Errors[0].Type);
		}

		[Theory]
		[Trait("Category", "enumerable")]
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Empty, true)]
		[InlineData(ValidationValueType.Correct, true)]
		public void EnumerabletNullable(ValidationValueType type, bool isValid)
		{
			var person = new Person();
			switch (type)
			{
				case ValidationValueType.Null:
					person.MyAddressesNullable = null;
					break;
				case ValidationValueType.Empty:
					person.MyAddressesNullable = new System.Collections.Generic.List<Address>();
					break;
				case ValidationValueType.Correct:
					person.MyAddressesNullable = new System.Collections.Generic.List<Address> { new Address() };
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyAddressesNullable, x => x.NotNull());

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyAddressesNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotNull, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "enumerable")]
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Empty, true)]
		[InlineData(ValidationValueType.Correct, true)]
		public void EnumerabletNotNull(ValidationValueType type, bool isValid)
		{
			var person = new Person();
			switch (type)
			{
				case ValidationValueType.Null:
					person.MyAddressesNotNull = null;
					break;
				case ValidationValueType.Empty:
					person.MyAddressesNotNull = new System.Collections.Generic.List<Address>();
					break;
				case ValidationValueType.Correct:
					person.MyAddressesNotNull = new System.Collections.Generic.List<Address> { new Address() };
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyAddressesNotNull, x => x.NotNull());

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyAddressesNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotNull, result.Errors[0].Type);
			}
		}
	}
}
