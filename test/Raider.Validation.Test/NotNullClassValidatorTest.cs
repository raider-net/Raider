using Raider.Validation.Test.Model;
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

		[Fact]
		[Trait("Category", "base")]
		public void Base_StringNullable()
		{
			var validator = new Validator<Person>()
					.ForProperty(x => x.MyStringNullable, x => x.NotNull());

			var result = validator.Validate((object?)null);

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

			var validator = new Validator<Person>()
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

			var validator = new Validator<Person>()
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
			var validator = new Validator<Person>()
					.ForProperty(x => x.MyProfileNullable, x => x.NotNull());

			var result = validator.Validate((object?)null);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_.MyProfileNullable", result.Errors[0].ValidationFrame.ToString());
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
					person.MyProfileNullable = null;
					break;
				case ValidationValueType.Empty:
					person.MyProfileNullable = new Profile();
					break;
				case ValidationValueType.Correct:
					person.MyProfileNullable = new Profile();
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyProfileNullable, x => x.NotNull());

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyProfileNullable", result.Errors[0].ValidationFrame.ToString());
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
					person.MyProfileNotNull = null;
					break;
				case ValidationValueType.Empty:
					person.MyProfileNotNull = new Profile();
					break;
				case ValidationValueType.Correct:
					person.MyProfileNotNull = new Profile();
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyProfileNotNull, x => x.NotNull());

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyProfileNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotNull, result.Errors[0].Type);
			}
		}

		[Fact]
		[Trait("Category", "base")]
		public void Base_EnumerabletNullable()
		{
			var validator = new Validator<Person>()
					.ForProperty(x => x.MyAddressesNullable, (ClassPropertyValidator<Person, System.Collections.Generic.IEnumerable<Address>> x) => x.NotNull());

			var result = validator.Validate((object?)null);

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

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyAddressesNullable, (ClassPropertyValidator<Person, System.Collections.Generic.IEnumerable<Address>> x) => x.NotNull());

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

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyAddressesNotNull, (ClassPropertyValidator<Person, System.Collections.Generic.IEnumerable<Address>> x) => x.NotNull());

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
