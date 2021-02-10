using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class NotDefaultOrEmptyStructValidatorTest
	{
		private readonly ITestOutputHelper _output;

		public NotDefaultOrEmptyStructValidatorTest(ITestOutputHelper output)
		{
			_output = output ?? throw new ArgumentNullException(nameof(output));
		}

		[Theory]
		[Trait("Category", "int")]
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		public void IntNullable(ValidationValueType type, bool isValid)
		{
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
					person.MyIntNullable = 8;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable());

			var result = validator.Validate(person);

			if (isValid)
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
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		public void DecimalNullable(ValidationValueType type, bool isValid)
		{
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
					person.MyDecimalNullable = 8;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyDecimalNullable, x => x.NotDefaultOrEmptyNullable());

			var result = validator.Validate(person);

			if (isValid)
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
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		public void BoolNullable(ValidationValueType type, bool isValid)
		{
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
					person.MyBoolNullable = true;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyBoolNullable, x => x.NotDefaultOrEmptyNullable());

			var result = validator.Validate(person);

			if (isValid)
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
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		public void DateTimeNullable(ValidationValueType type, bool isValid)
		{
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
					person.MyDateTimeNullable = DateTime.Now;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmptyNullable());

			var result = validator.Validate(person);

			if (isValid)
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
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		public void GuidNullable(ValidationValueType type, bool isValid)
		{
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
					person.MyGuidNullable = Guid.NewGuid();
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyGuidNullable, x => x.NotDefaultOrEmptyNullable());

			var result = validator.Validate(person);

			if (isValid)
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
		[Trait("Category", "enum")]
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		public void EnumNullable(ValidationValueType type, bool isValid)
		{
			var person = new Person();
			switch (type)
			{
				case ValidationValueType.Null:
					person.MyEnumNullable = null;
					break;
				case ValidationValueType.Default:
					person.MyEnumNullable = default(MyTestEnum);
					break;
				case ValidationValueType.Correct:
					person.MyEnumNullable = MyTestEnum.Value1;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyEnumNullable, x => x.NotDefaultOrEmptyNullable());

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyEnumNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
			}
		}













		[Theory]
		[Trait("Category", "int")]
		//[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		public void IntNotNull(ValidationValueType type, bool isValid)
		{
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
					person.MyIntNotNull = 8;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyIntNotNull, x => x.NotDefaultOrEmpty());

			var result = validator.Validate(person);

			if (isValid)
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
		//[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		public void DecimalNotNull(ValidationValueType type, bool isValid)
		{
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
					person.MyDecimalNotNull = 8;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyDecimalNotNull, x => x.NotDefaultOrEmpty());

			var result = validator.Validate(person);

			if (isValid)
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
		//[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		public void BoolNotNull(ValidationValueType type, bool isValid)
		{
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
					person.MyBoolNotNull = true;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyBoolNotNull, x => x.NotDefaultOrEmpty());

			var result = validator.Validate(person);

			if (isValid)
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
		//[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		public void DateTimeNotNull(ValidationValueType type, bool isValid)
		{
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
					person.MyDateTimeNotNull = DateTime.Now;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyDateTimeNotNull, x => x.NotDefaultOrEmpty());

			var result = validator.Validate(person);

			if (isValid)
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
		//[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		public void GuidNotNull(ValidationValueType type, bool isValid)
		{
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
					person.MyGuidNotNull = Guid.NewGuid();
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyGuidNotNull, x => x.NotDefaultOrEmpty());

			var result = validator.Validate(person);

			if (isValid)
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
		[Trait("Category", "enum")]
		//[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		public void EnumNotNull(ValidationValueType type, bool isValid)
		{
			var person = new Person();
			switch (type)
			{
				//case ValidationValueType.Null:
				//	person.MyEnumNotNull = null;
				//	break;
				case ValidationValueType.Default:
					person.MyEnumNotNull = default(MyTestEnum);
					break;
				case ValidationValueType.Correct:
					person.MyEnumNotNull = MyTestEnum.Value1;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyEnumNotNull, x => x.NotDefaultOrEmpty());

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyEnumNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
			}
		}
	}
}
