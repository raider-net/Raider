using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class ClientConditionValidatorTest
	{
		private readonly ITestOutputHelper _output;

		public ClientConditionValidatorTest(ITestOutputHelper output)
		{
			_output = output ?? throw new ArgumentNullException(nameof(output));
		}

		[Fact]
		[Trait("Category", "COMPLEX")]
		public void Complex()
		{
			var person = new Person
			{
				MyStringNotNull = "bar",
				MyStringNullable = "b",
				MyDecimalNotNull = 8,
				MyDecimalNullable = 7
			};

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.EqualsTo(x => x.MyIntNotNull, default(int)))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.NotEqualsTo(x => x.MyIntNotNull, 1))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.LessThan(x => x.MyIntNotNull, 1))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.LessThanOrEqualTo(x => x.MyIntNotNull, default(int)))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.GreaterThan(x => x.MyIntNotNull, -1))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.GreaterThanOrEqualTo(x => x.MyIntNotNull, default(int)))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.StartsWith(x => x.MyStringNotNull, "b"))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.Contains(x => x.MyStringNotNull, "a"))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.EndsWith(x => x.MyStringNotNull, "r"))

					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.EqualsTo(x => x.MyDecimalNotNull, x => x.MyDecimalNotNull))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.NotEqualsTo(x => x.MyDecimalNotNull, x => x.MyDecimalNullable))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.LessThan(x => x.MyDecimalNullable, x => x.MyDecimalNotNull))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.LessThanOrEqualTo(x => x.MyDecimalNullable, x => x.MyDecimalNotNull))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.GreaterThan(x => x.MyDecimalNotNull, x => x.MyDecimalNullable))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.GreaterThanOrEqualTo(x => x.MyDecimalNotNull, x => x.MyDecimalNullable))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.StartsWith(x => x.MyStringNotNull, x => x.MyStringNullable))

					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null,
						x => x.And(
							y => y.EqualsTo(z => z.MyDecimalNotNull, z => z.MyDecimalNotNull),
							y => y.Or(
								a => a.EqualsTo(z => z.MyDecimalNotNull, 123),
								a => a.EqualsTo(z => z.MyDecimalNotNull, z => z.MyDecimalNotNull))))
					;

			var result = validator.Validate(person);
			var str = result.Errors[16].ClientConditionDefinition?.ToString();
			Assert.Equal(17, result.Errors.Count);
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

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyIntNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyDecimalNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDecimalNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyBoolNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyBoolNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDateTimeNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyGuidNullable, x => x.NotDefaultOrEmptyNullable(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyGuidNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyIntNotNull, x => x.NotDefaultOrEmpty(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyIntNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyDecimalNotNull, x => x.NotDefaultOrEmpty(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDecimalNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyBoolNotNull, x => x.NotDefaultOrEmpty(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyBoolNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyDateTimeNotNull, x => x.NotDefaultOrEmpty(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDateTimeNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyGuidNotNull, x => x.NotDefaultOrEmpty(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyGuidNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyStringNullable, x => x.NotDefaultOrEmpty(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyStringNotNull, x => x.NotDefaultOrEmpty(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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
					.ForProperty(x => x.ANullable, x => x.NotDefaultOrEmpty(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.ANullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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
					.ForProperty(x => x.ANotNull, x => x.NotDefaultOrEmpty(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.ANotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyAddressesNullable, x => x.NotDefaultOrEmpty(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyAddressesNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
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

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyAddressesNotNull, x => x.NotDefaultOrEmpty(), null, x => x.LessThan(x => x.MyIntNotNull, condition ? 55 : -55));

			var result = validator.Validate(person);

			if (!condition || isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyAddressesNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
			}
		}
	}
}
