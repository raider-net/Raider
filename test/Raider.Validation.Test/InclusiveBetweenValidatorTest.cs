using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class InclusiveBetweenValidatorTest
	{
		private readonly ITestOutputHelper _output;

		public InclusiveBetweenValidatorTest(ITestOutputHelper output)
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
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
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
					person.MyIntNullable = 10;
					break;
				case ValidationValueType.Incorrect:
					person.MyIntNullable = 11;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyIntNullable, x => x.InclusiveBetween(5, 10));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyIntNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.InclusiveBetween, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "decimal")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
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
					person.MyDecimalNullable = 10;
					break;
				case ValidationValueType.Incorrect:
					person.MyDecimalNullable = 11;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyDecimalNullable, x => x.InclusiveBetween(5, 10));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDecimalNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.InclusiveBetween, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "DateTime")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
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
					person.MyDateTimeNullable = new DateTime(2022, 1, 1);
					break;
				case ValidationValueType.Incorrect:
					person.MyDateTimeNullable = new DateTime(2024, 1, 1);
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyDateTimeNullable, x => x.InclusiveBetween(new DateTime(2019, 1, 1), new DateTime(2022, 1, 1)));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDateTimeNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.InclusiveBetween, result.Errors[0].Type);
			}
		}

















		[Theory]
		[Trait("Category", "int")]
		//[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
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
					person.MyIntNotNull = 10;
					break;
				case ValidationValueType.Incorrect:
					person.MyIntNotNull = 11;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyIntNotNull, x => x.InclusiveBetween(5, 10));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyIntNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.InclusiveBetween, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "decimal")]
		//[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
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
					person.MyDecimalNotNull = 10;
					break;
				case ValidationValueType.Incorrect:
					person.MyDecimalNotNull = 11;
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyDecimalNotNull, x => x.InclusiveBetween(5, 10));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDecimalNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.InclusiveBetween, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "DateTime")]
		//[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Default, false)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
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
					person.MyDateTimeNotNull = new DateTime(2022, 1, 1);
					break;
				case ValidationValueType.Incorrect:
					person.MyDateTimeNotNull = new DateTime(2024, 1, 1);
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyDateTimeNotNull, x => x.InclusiveBetween(new DateTime(2019, 1, 1), new DateTime(2022, 1, 1)));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyDateTimeNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.InclusiveBetween, result.Errors[0].Type);
			}
		}












		[Theory]
		[Trait("Category", "string")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Empty, false)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
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
					person.MyStringNullable = "z";
					break;
				case ValidationValueType.Incorrect:
					person.MyStringNullable = "1";
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyStringNullable, x => x.InclusiveBetween("a", "z"));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.InclusiveBetween, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "string")]
		[InlineData(ValidationValueType.Null, true)]
		[InlineData(ValidationValueType.Empty, false)]
		[InlineData(ValidationValueType.Correct, true)]
		[InlineData(ValidationValueType.Incorrect, false)]
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
					person.MyStringNotNull = "z";
					break;
				case ValidationValueType.Incorrect:
					person.MyStringNotNull = "1";
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyStringNotNull, x => x.InclusiveBetween("a", "z"));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyStringNotNull", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.InclusiveBetween, result.Errors[0].Type);
			}
		}
	}
}
