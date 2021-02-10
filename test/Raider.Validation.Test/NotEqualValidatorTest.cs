using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class NotEqualValidatorTest
	{
		private readonly ITestOutputHelper _output;

		public NotEqualValidatorTest(ITestOutputHelper output)
		{
			_output = output ?? throw new ArgumentNullException(nameof(output));
		}

		[Theory]
		[Trait("Category", "int")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, true)]
		[InlineData(ValidationValueType.Correct, false)]
		[InlineData(ValidationValueType.Incorrect, true)]
		public void IntNullable(ValidationValueType type, bool isValid)
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
				case ValidationValueType.Incorrect:
					person.MyIntNullable = 2;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyIntNullable, x => x.NotEqualsTo(validValue));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyIntNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotEqual, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "decimal")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, true)]
		[InlineData(ValidationValueType.Correct, false)]
		[InlineData(ValidationValueType.Incorrect, true)]
		public void DecimalNullable(ValidationValueType type, bool isValid)
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
				case ValidationValueType.Incorrect:
					person.MyDecimalNullable = 2;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyDecimalNullable, x => x.NotEqualsTo(validValue));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDecimalNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotEqual, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "bool")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, true)]
		[InlineData(ValidationValueType.Correct, false)]
		[InlineData(ValidationValueType.Incorrect, true)]
		public void BoolNullable(ValidationValueType type, bool isValid)
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
				case ValidationValueType.Incorrect:
					person.MyBoolNullable = false;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyBoolNullable, x => x.NotEqualsTo(validValue));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyBoolNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotEqual, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "DateTime")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, true)]
		[InlineData(ValidationValueType.Correct, false)]
		[InlineData(ValidationValueType.Incorrect, true)]
		public void DateTimeNullable(ValidationValueType type, bool isValid)
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
				case ValidationValueType.Incorrect:
					person.MyDateTimeNullable = new DateTime(2020, 1, 1);
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyDateTimeNullable, x => x.NotEqualsTo(validValue));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDateTimeNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotEqual, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "Guid")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, true)]
		[InlineData(ValidationValueType.Correct, false)]
		[InlineData(ValidationValueType.Incorrect, true)]
		public void GuidNullable(ValidationValueType type, bool isValid)
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
				case ValidationValueType.Incorrect:
					person.MyGuidNullable = Guid.NewGuid(); ;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyGuidNullable, x => x.NotEqualsTo(validValue));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyGuidNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotEqual, result.Errors[0].Type);
			}
		}

















		[Theory]
		[Trait("Category", "int")]
		//[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, true)]
		[InlineData(ValidationValueType.Correct, false)]
		[InlineData(ValidationValueType.Incorrect, true)]
		public void IntNotNull(ValidationValueType type, bool isValid)
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
				case ValidationValueType.Incorrect:
					person.MyIntNotNull = 2;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyIntNotNull, x => x.NotEqualsTo(validValue));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyIntNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotEqual, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "decimal")]
		//[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, true)]
		[InlineData(ValidationValueType.Correct, false)]
		[InlineData(ValidationValueType.Incorrect, true)]
		public void DecimalNotNull(ValidationValueType type, bool isValid)
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
				case ValidationValueType.Incorrect:
					person.MyDecimalNotNull = 2;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyDecimalNotNull, x => x.NotEqualsTo(validValue));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDecimalNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotEqual, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "bool")]
		//[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, true)]
		[InlineData(ValidationValueType.Correct, false)]
		[InlineData(ValidationValueType.Incorrect, true)]
		public void BoolNotNull(ValidationValueType type, bool isValid)
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
				case ValidationValueType.Incorrect:
					person.MyBoolNotNull = false;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyBoolNotNull, x => x.NotEqualsTo(validValue));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyBoolNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotEqual, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "DateTime")]
		//[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, true)]
		[InlineData(ValidationValueType.Correct, false)]
		[InlineData(ValidationValueType.Incorrect, true)]
		public void DateTimeNotNull(ValidationValueType type, bool isValid)
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
				case ValidationValueType.Incorrect:
					person.MyDateTimeNotNull = new DateTime(2020, 1, 1);
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyDateTimeNotNull, x => x.NotEqualsTo(validValue));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDateTimeNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotEqual, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "Guid")]
		//[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, true)]
		[InlineData(ValidationValueType.Correct, false)]
		[InlineData(ValidationValueType.Incorrect, true)]
		public void GuidNotNull(ValidationValueType type, bool isValid)
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
				case ValidationValueType.Incorrect:
					person.MyGuidNotNull = Guid.NewGuid();
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyGuidNotNull, x => x.NotEqualsTo(validValue));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyGuidNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotEqual, result.Errors[0].Type);
			}
		}












		[Theory]
		[Trait("Category", "string")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Empty, true)]
		[InlineData(ValidationValueType.Correct, false)]
		public void StringNullable(ValidationValueType type, bool isValid)
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
				case ValidationValueType.Incorrect:
					person.MyStringNullable = "foo";
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyStringNullable, x => x.NotEqualsTo(validValue));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotEqual, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "string")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Empty, true)]
		[InlineData(ValidationValueType.Correct, false)]
		public void StringNotNull(ValidationValueType type, bool isValid)
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
				case ValidationValueType.Incorrect:
					person.MyStringNotNull = "foo";
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyStringNotNull, x => x.NotEqualsTo(validValue));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotEqual, result.Errors[0].Type);
			}
		}
	}
}
