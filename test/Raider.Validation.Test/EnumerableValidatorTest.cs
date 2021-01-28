using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class EnumerableValidatorTest
	{
		private readonly ITestOutputHelper _output;

		public EnumerableValidatorTest(ITestOutputHelper output)
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
		[Trait("Category", "enumerable")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Empty, true)]
		//[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
		public void EnumerableNullable(ValidationValueType type, bool isValid)
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
					person.MyAddressesNullable = new System.Collections.Generic.List<Address> { new Address { AddStringNullable = "test" } };
					break;
				case ValidationValueType.Incorrect:
					person.MyAddressesNullable = new System.Collections.Generic.List<Address> { new Address() };
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForEach(x => x.MyAddressesNullable, x => x.ForProperty(p => p.AddStringNullable, v => v.EqualsTo("test")))
					.ForProperty(x => x.MyAddressesNullable, (ClassPropertyValidator<Person, System.Collections.Generic.IEnumerable<Address>> x) => x.DefaultOrEmpty());

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(2, result.Errors.Count);
				Assert.Equal("_.MyAddressesNullable[0].AddStringNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.Equal, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "enumerable")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Empty, true)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
		public void EnumerableNotNull(ValidationValueType type, bool isValid)
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
					person.MyAddressesNotNull = new System.Collections.Generic.List<Address> { new Address { AddStringNullable = "test" } };
					break;
				case ValidationValueType.Incorrect:
					person.MyAddressesNotNull = new System.Collections.Generic.List<Address> { new Address() };
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForEach(x => x.MyAddressesNotNull, x => x.ForProperty(p => p.AddStringNullable, v => v.EqualsTo("test")));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyAddressesNotNull[0].AddStringNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.Equal, result.Errors[0].Type);
			}
		}
	}
}
