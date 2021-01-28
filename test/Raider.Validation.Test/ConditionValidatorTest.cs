using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class ConditionValidatorTest
	{
		private readonly ITestOutputHelper _output;

		public ConditionValidatorTest(ITestOutputHelper output)
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
		[Trait("Category", "int")]
		[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Default, false, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Default, false, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void IntNullable(ValidationValueType type, bool isValid, bool condition)
		{
			int validValue = 8;
			var person = new Person();
			switch (type)
			{
				case ValidationValueType.Null:
					person.MyIntNullable = null;
					break;
				case ValidationValueType.Default:
					person.MyIntNullable = default(int);
					break;
				case ValidationValueType.Correct:
					person.MyIntNullable = validValue;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyIntNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyStruct, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "decimal")]
		[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Default, false, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Default, false, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void DecimalNullable(ValidationValueType type, bool isValid, bool condition)
		{
			decimal validValue = 8;
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
					person.MyDecimalNullable = validValue;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyDecimalNullable, x => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDecimalNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyStruct, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "bool")]
		[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Default, false, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Default, false, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void BoolNullable(ValidationValueType type, bool isValid, bool condition)
		{
			bool validValue = true;
			var person = new Person();
			switch (type)
			{
				case ValidationValueType.Null:
					person.MyBoolNullable = null;
					break;
				case ValidationValueType.Default:
					person.MyBoolNullable = default(bool);
					break;
				case ValidationValueType.Correct:
					person.MyBoolNullable = validValue;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyBoolNullable, x => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyBoolNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyStruct, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "DateTime")]
		[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Default, false, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Default, false, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void DateTimeNullable(ValidationValueType type, bool isValid, bool condition)
		{
			var validValue = DateTime.Now;
			var person = new Person();
			switch (type)
			{
				case ValidationValueType.Null:
					person.MyDateTimeNullable = null;
					break;
				case ValidationValueType.Default:
					person.MyDateTimeNullable = default(DateTime);
					break;
				case ValidationValueType.Correct:
					person.MyDateTimeNullable = validValue;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDateTimeNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyStruct, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "Guid")]
		[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Default, false, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Default, false, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void GuidNullable(ValidationValueType type, bool isValid, bool condition)
		{
			Guid validValue = Guid.NewGuid();
			var person = new Person();
			switch (type)
			{
				case ValidationValueType.Null:
					person.MyGuidNullable = null;
					break;
				case ValidationValueType.Default:
					person.MyGuidNullable = default(Guid);
					break;
				case ValidationValueType.Correct:
					person.MyGuidNullable = validValue;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyGuidNullable, x => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyGuidNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyStruct, result.Errors[0].Type);
			}
		}

















		[Theory]
		[Trait("Category", "int")]
		//[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Default, false, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		//[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Default, false, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void IntNotNull(ValidationValueType type, bool isValid, bool condition)
		{
			int validValue = 8;
			var person = new Person();
			switch (type)
			{
				//case ValidationValueType.Null:
				//	person.MyIntNotNull = null;
				//	break;
				case ValidationValueType.Default:
					person.MyIntNotNull = default(int);
					break;
				case ValidationValueType.Correct:
					person.MyIntNotNull = validValue;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyIntNotNull, x => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyIntNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyStruct, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "decimal")]
		//[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Default, false, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		//[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Default, false, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void DecimalNotNull(ValidationValueType type, bool isValid, bool condition)
		{
			decimal validValue = 8;
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
					person.MyDecimalNotNull = validValue;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyDecimalNotNull, x => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDecimalNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyStruct, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "bool")]
		//[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Default, false, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		//[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Default, false, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void BoolNotNull(ValidationValueType type, bool isValid, bool condition)
		{
			bool validValue = true;
			var person = new Person();
			switch (type)
			{
				//case ValidationValueType.Null:
				//	person.MyBoolNotNull = null;
				//	break;
				case ValidationValueType.Default:
					person.MyBoolNotNull = default(bool);
					break;
				case ValidationValueType.Correct:
					person.MyBoolNotNull = validValue;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyBoolNotNull, x => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyBoolNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyStruct, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "DateTime")]
		//[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Default, false, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		//[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Default, false, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void DateTimeNotNull(ValidationValueType type, bool isValid, bool condition)
		{
			DateTime validValue = DateTime.Now;
			var person = new Person();
			switch (type)
			{
				//case ValidationValueType.Null:
				//	person.MyDateTimeNotNull = null;
				//	break;
				case ValidationValueType.Default:
					person.MyDateTimeNotNull = default(DateTime);
					break;
				case ValidationValueType.Correct:
					person.MyDateTimeNotNull = validValue;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyDateTimeNotNull, x => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDateTimeNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyStruct, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "Guid")]
		//[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Default, false, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		//[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Default, false, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void GuidNotNull(ValidationValueType type, bool isValid, bool condition)
		{
			Guid validValue = Guid.NewGuid();
			var person = new Person();
			switch (type)
			{
				//case ValidationValueType.Null:
				//	person.MyGuidNotNull = null;
				//	break;
				case ValidationValueType.Default:
					person.MyGuidNotNull = default(Guid);
					break;
				case ValidationValueType.Correct:
					person.MyGuidNotNull = validValue;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyGuidNotNull, x => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyGuidNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyStruct, result.Errors[0].Type);
			}
		}







		[Theory]
		[Trait("Category", "string")]
		[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Empty, false, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Empty, false, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void StringNullable(ValidationValueType type, bool isValid, bool condition)
		{
			string validValue = "test";
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
					person.MyStringNullable = validValue;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyStringNullable, x => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyClass, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "string")]
		[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Empty, false, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Empty, false, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void StringNotNull(ValidationValueType type, bool isValid, bool condition)
		{
			string validValue = "test";
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
					person.MyStringNotNull = validValue;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyStringNotNull, x => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyClass, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "object")]
		[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Empty, true, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Empty, true, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void ObjectNullable(ValidationValueType type, bool isValid, bool condition)
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
					.ForProperty(x => x.MyProfileNullable, x => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyProfileNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyClass, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "object")]
		[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Empty, true, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Empty, true, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void ObjectNotNull(ValidationValueType type, bool isValid, bool condition)
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
					.ForProperty(x => x.MyProfileNotNull, x => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyProfileNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyClass, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "enumerable")]
		[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Empty, false, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Empty, false, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void EnumerableNullable(ValidationValueType type, bool isValid, bool condition)
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
					.ForProperty(x => x.MyAddressesNullable, (ClassPropertyValidator<Person, System.Collections.Generic.IEnumerable<Address>> x) => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyAddressesNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyClass, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "enumerable")]
		[InlineData(ValidationValueType.Null, false, true)]
		[InlineData(ValidationValueType.Empty, false, true)]
		[InlineData(ValidationValueType.Correct, true, true)]
		[InlineData(ValidationValueType.Null, false, false)]
		[InlineData(ValidationValueType.Empty, false, false)]
		[InlineData(ValidationValueType.Correct, true, false)]
		public void EnumerableNotNull(ValidationValueType type, bool isValid, bool condition)
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
					.ForProperty(x => x.MyAddressesNotNull, (ClassPropertyValidator<Person, System.Collections.Generic.IEnumerable<Address>> x) => x.NotDefaultOrEmpty(), x => condition);

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyAddressesNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmptyClass, result.Errors[0].Type);
			}
		}
	}
}
