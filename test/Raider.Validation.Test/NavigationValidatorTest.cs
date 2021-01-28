using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class NavigationValidatorTest
	{
		private readonly ITestOutputHelper _output;

		public NavigationValidatorTest(ITestOutputHelper output)
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
		[Trait("Category", "object")]
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Empty, false)]
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
					person.MyProfileNullable = new Profile { ProfStringNullable = "test" };
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForNavigation(x => x.MyProfileNullable, x => x.ForProperty(p => p.ProfStringNullable, v => v.EqualsTo("test")));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyProfileNullable.ProfStringNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.Equal, result.Errors[0].Type);
			}
		}

		[Theory]
		[Trait("Category", "object")]
		[InlineData(ValidationValueType.Null, false)]
		[InlineData(ValidationValueType.Empty, false)]
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
					person.MyProfileNotNull = new Profile { ProfStringNullable = "test" };
					break;
				default:
					throw new NotImplementedException();
			}

			var validator = new Validator<Person>()
					.ForNavigation(x => x.MyProfileNotNull, x => x.ForProperty(p => p.ProfStringNullable, v => v.EqualsTo("test")));

			var result = validator.Validate(person);

			if (isValid)
			{
				Assert.Equal(0, result.Errors.Count);
			}
			else
			{
				Assert.Equal(1, result.Errors.Count);
				Assert.Equal("_.MyProfileNotNull.ProfStringNullable", result.Errors[0].ValidationFrame.ToString());
				Assert.Equal(ValidatorType.Equal, result.Errors[0].Type);
			}
		}
	}
}
