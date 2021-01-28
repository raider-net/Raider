using Raider.Validation.Test.Model;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class ComplexValidationTest
	{
		private readonly ITestOutputHelper _output;

		public ComplexValidationTest(ITestOutputHelper output)
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
		public void Nullable_Flat_NoError()
		{
			var person = new Person
			{
				MyStringNullable = "test@email.com",
				MyIntNullable = 5,
				MyDateTimeNullable = DateTime.Now,
				MyDecimalNullable = 12.34m,
				MyProfileNullable = new Profile
				{
					ProfStringNullable = "test@email.com",
					ProfIntNullable = 5,
					ProfDateTimeNullable = DateTime.Now,
					ProfDecimalNullable = 12.34m,
				},
				MyAddressesNullable = new System.Collections.Generic.List<Address>
				{
					new Address
					{
						AddStringNullable = "test@email.com",
						AddIntNullable = 5,
						AddDateTimeNullable = DateTime.Now,
						AddDecimalNullable = 12.34m
					},
					new Address
					{
						AddStringNullable = "test@email.com",
						AddIntNullable = 5,
						AddDateTimeNullable = DateTime.Now,
						AddDecimalNullable = 12.34m
					},
				}
			};

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyStringNullable, x => x.EmailAddress())
					.ForProperty(x => x.MyStringNullable, x => x.MaxLength(14))
					.ForProperty(x => x.MyStringNullable, x => x.RegEx(@"^[a-z]{4}@[a-z]{5}.com$"))
					.ForProperty(x => x.MyStringNullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.MyStringNullable, x => x.NotNull())

					.ForProperty(x => x.MyIntNullable, x => x.EqualsTo(5))
					.ForProperty(x => x.MyIntNullable, x => x.NotEqualsTo(6))
					.ForProperty(x => x.MyIntNullable, x => x.ExclusiveBetween(4, 6))
					.ForProperty(x => x.MyIntNullable, x => x.InclusiveBetween(5, 5))
					.ForProperty(x => x.MyIntNullable, x => x.GreaterThanOrEqual(5))
					.ForProperty(x => x.MyIntNullable, x => x.GreaterThan(4))
					.ForProperty(x => x.MyIntNullable, x => x.LessThanOrEqual(5))
					.ForProperty(x => x.MyIntNullable, x => x.LessThan(6))
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.MyIntNullable, x => x.NotNull())

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmpty())

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))


					.ForProperty(x => x.MyProfileNullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.MyProfileNullable, x => x.NotNull())
					.ForNavigation(x => x.MyProfileNullable, x => x
						.ForProperty(x => x.ProfStringNullable, x => x.EmailAddress())
						.ForProperty(x => x.ProfStringNullable, x => x.MaxLength(14))
						.ForProperty(x => x.ProfStringNullable, x => x.RegEx(@"^[a-z]{4}@[a-z]{5}.com$"))
						.ForProperty(x => x.ProfStringNullable, x => x.NotDefaultOrEmpty())
						.ForProperty(x => x.ProfStringNullable, x => x.NotNull())

						.ForProperty(x => x.ProfIntNullable, x => x.EqualsTo(5))
						.ForProperty(x => x.ProfIntNullable, x => x.NotEqualsTo(6))
						.ForProperty(x => x.ProfIntNullable, x => x.ExclusiveBetween(4, 6))
						.ForProperty(x => x.ProfIntNullable, x => x.InclusiveBetween(5, 5))
						.ForProperty(x => x.ProfIntNullable, x => x.GreaterThanOrEqual(5))
						.ForProperty(x => x.ProfIntNullable, x => x.GreaterThan(4))
						.ForProperty(x => x.ProfIntNullable, x => x.LessThanOrEqual(5))
						.ForProperty(x => x.ProfIntNullable, x => x.LessThan(6))
						.ForProperty(x => x.ProfIntNullable, x => x.NotDefaultOrEmpty())
						.ForProperty(x => x.ProfIntNullable, x => x.NotNull())

						.ForProperty(x => x.ProfDateTimeNullable, x => x.NotDefaultOrEmpty())

						.ForProperty(x => x.ProfDecimalNullable, x => x.PrecisionScale(4, 2, false)))

					.ForProperty(x => x.MyAddressesNullable, x => x.NotNull())
					.ForProperty(x => x.MyAddressesNullable, x => x.NotDefaultOrEmpty())
					.ForEach(x => x.MyAddressesNullable, x => x
						.ForProperty(x => x.AddStringNullable, x => x.EmailAddress())
						.ForProperty(x => x.AddStringNullable, x => x.MaxLength(14))
						.ForProperty(x => x.AddStringNullable, x => x.RegEx(@"^[a-z]{4}@[a-z]{5}.com$"))
						.ForProperty(x => x.AddStringNullable, x => x.NotDefaultOrEmpty())
						.ForProperty(x => x.AddStringNullable, x => x.NotNull())

						.ForProperty(x => x.AddIntNullable, x => x.EqualsTo(5))
						.ForProperty(x => x.AddIntNullable, x => x.NotEqualsTo(6))
						.ForProperty(x => x.AddIntNullable, x => x.ExclusiveBetween(4, 6))
						.ForProperty(x => x.AddIntNullable, x => x.InclusiveBetween(5, 5))
						.ForProperty(x => x.AddIntNullable, x => x.GreaterThanOrEqual(5))
						.ForProperty(x => x.AddIntNullable, x => x.GreaterThan(4))
						.ForProperty(x => x.AddIntNullable, x => x.LessThanOrEqual(5))
						.ForProperty(x => x.AddIntNullable, x => x.LessThan(6))
						.ForProperty(x => x.AddIntNullable, x => x.NotDefaultOrEmpty())
						.ForProperty(x => x.AddIntNullable, x => x.NotNull())

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmpty())

						.ForProperty(x => x.AddDecimalNullable, x => x.PrecisionScale(4, 2, false)))
					;

			var result = validator.Validate(person);

			Assert.Equal(0, result.Errors.Count);
		}


		[Fact]
		public void Nullable_Chained_NoError()
		{
			var person = new Person
			{
				MyStringNullable = "test@email.com",
				MyIntNullable = 5,
				MyDateTimeNullable = DateTime.Now,
				MyDecimalNullable = 12.34m,
				MyProfileNullable = new Profile
				{
					ProfStringNullable = "test@email.com",
					ProfIntNullable = 5,
					ProfDateTimeNullable = DateTime.Now,
					ProfDecimalNullable = 12.34m,
				},
				MyAddressesNullable = new System.Collections.Generic.List<Address>
				{
					new Address
					{
						AddStringNullable = "test@email.com",
						AddIntNullable = 5,
						AddDateTimeNullable = DateTime.Now,
						AddDecimalNullable = 12.34m
					},
					new Address
					{
						AddStringNullable = "test@email.com",
						AddIntNullable = 5,
						AddDateTimeNullable = DateTime.Now,
						AddDecimalNullable = 12.34m
					},
				}
			};

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyStringNullable, x => x
						.EmailAddress()
						.MaxLength(14)
						.RegEx(@"^[a-z]{4}@[a-z]{5}.com$")
						.NotDefaultOrEmpty()
						.NotNull())

					.ForProperty(x => x.MyIntNullable, x => x
						.EqualsTo(5)
						.NotEqualsTo(6)
						.ExclusiveBetween(4, 6)
						.InclusiveBetween(5, 5)
						.GreaterThanOrEqual(5)
						.GreaterThan(4)
						.LessThanOrEqual(5)
						.LessThan(6)
						.NotDefaultOrEmpty()
						.NotNull())

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmpty())

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))


					.ForProperty(x => x.MyProfileNullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.MyProfileNullable, x => x.NotNull())
					.ForNavigation(x => x.MyProfileNullable, x => x
						.ForProperty(x => x.ProfStringNullable, x => x
							.EmailAddress()
							.MaxLength(14)
							.RegEx(@"^[a-z]{4}@[a-z]{5}.com$")
							.NotDefaultOrEmpty()
							.NotNull())

						.ForProperty(x => x.ProfIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(6)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(5)
							.GreaterThan(4)
							.LessThanOrEqual(5)
							.LessThan(6)
							.NotDefaultOrEmpty()
							.NotNull())

						.ForProperty(x => x.ProfDateTimeNullable, x => x.NotDefaultOrEmpty())

						.ForProperty(x => x.ProfDecimalNullable, x => x.PrecisionScale(4, 2, false)))

					.ForProperty(x => x.MyAddressesNullable, x => x.NotNull())
					.ForProperty(x => x.MyAddressesNullable, x => x.NotDefaultOrEmpty())
					.ForEach(x => x.MyAddressesNullable, x => x
						.ForProperty(x => x.AddStringNullable, x => x
							.EmailAddress()
							.MaxLength(14)
							.RegEx(@"^[a-z]{4}@[a-z]{5}.com$")
							.NotDefaultOrEmpty()
							.NotNull())

						.ForProperty(x => x.AddIntNullable, x => x.EqualsTo(5)
							.EqualsTo(5)
							.NotEqualsTo(6)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(5)
							.GreaterThan(4)
							.LessThanOrEqual(5)
							.LessThan(6)
							.NotDefaultOrEmpty()
							.NotNull())

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmpty())

						.ForProperty(x => x.AddDecimalNullable, x => x.PrecisionScale(4, 2, false)))
					;

			var result = validator.Validate(person);

			Assert.Equal(0, result.Errors.Count);
		}

		[Fact]
		public void Nullable_Flat_AllError()
		{
			var person = new Person
			{
				MyIntNullable = 10,
				MyDecimalNullable = 160,
				MyProfileNullable = new Profile
				{
					ProfIntNullable = 10,
					ProfDecimalNullable = 160
				},
				MyAddressesNullable = new System.Collections.Generic.List<Address>
				{
					new Address
					{
						AddIntNullable = 10,
						AddDecimalNullable = 160
					},
					new Address
					{
						AddIntNullable = 10,
						AddDecimalNullable = 160
					},
				}
			};

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyStringNullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.MyStringNullable, x => x.NotNull())

					.ForProperty(x => x.MyIntNullable, x => x.EqualsTo(5))
					.ForProperty(x => x.MyIntNullable, x => x.NotEqualsTo(10))
					.ForProperty(x => x.MyIntNullable, x => x.ExclusiveBetween(4, 6))
					.ForProperty(x => x.MyIntNullable, x => x.InclusiveBetween(5, 5))
					.ForProperty(x => x.MyIntNullable, x => x.GreaterThanOrEqual(15))
					.ForProperty(x => x.MyIntNullable, x => x.GreaterThan(14))
					.ForProperty(x => x.MyIntNullable, x => x.LessThanOrEqual(5))
					.ForProperty(x => x.MyIntNullable, x => x.LessThan(6))

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmpty())

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))


					.ForProperty(x => x.MyProfileNullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.MyProfileNullable, x => x.NotNull())
					.ForNavigation(x => x.MyProfileNullable, x => x
						.ForProperty(x => x.ProfStringNullable, x => x.NotDefaultOrEmpty())
						.ForProperty(x => x.ProfStringNullable, x => x.NotNull())

						.ForProperty(x => x.ProfIntNullable, x => x.EqualsTo(5))
						.ForProperty(x => x.ProfIntNullable, x => x.NotEqualsTo(10))
						.ForProperty(x => x.ProfIntNullable, x => x.ExclusiveBetween(4, 6))
						.ForProperty(x => x.ProfIntNullable, x => x.InclusiveBetween(5, 5))
						.ForProperty(x => x.ProfIntNullable, x => x.GreaterThanOrEqual(15))
						.ForProperty(x => x.ProfIntNullable, x => x.GreaterThan(14))
						.ForProperty(x => x.ProfIntNullable, x => x.LessThanOrEqual(5))
						.ForProperty(x => x.ProfIntNullable, x => x.LessThan(6))

						.ForProperty(x => x.ProfDateTimeNullable, x => x.NotDefaultOrEmpty())

						.ForProperty(x => x.ProfDecimalNullable, x => x.PrecisionScale(4, 2, false)))

					.ForProperty(x => x.MyAddressesNullable, x => x.NotNull())
					.ForProperty(x => x.MyAddressesNullable, x => x.NotDefaultOrEmpty())
					.ForEach(x => x.MyAddressesNullable, x => x
						.ForProperty(x => x.AddStringNullable, x => x.NotDefaultOrEmpty())
						.ForProperty(x => x.AddStringNullable, x => x.NotNull())

						.ForProperty(x => x.AddIntNullable, x => x.EqualsTo(5))
						.ForProperty(x => x.AddIntNullable, x => x.NotEqualsTo(10))
						.ForProperty(x => x.AddIntNullable, x => x.ExclusiveBetween(4, 6))
						.ForProperty(x => x.AddIntNullable, x => x.InclusiveBetween(5, 5))
						.ForProperty(x => x.AddIntNullable, x => x.GreaterThanOrEqual(15))
						.ForProperty(x => x.AddIntNullable, x => x.GreaterThan(14))
						.ForProperty(x => x.AddIntNullable, x => x.LessThanOrEqual(5))
						.ForProperty(x => x.AddIntNullable, x => x.LessThan(6))

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmpty())

						.ForProperty(x => x.AddDecimalNullable, x => x.PrecisionScale(4, 2, false)))
					;

			var result = validator.Validate(person);

			Assert.Equal(48, result.Errors.Count);
		}


		[Fact]
		public void Nullable_Chained_AllError()
		{
			var person = new Person
			{
				MyIntNullable = 10,
				MyDecimalNullable = 160,
				MyProfileNullable = new Profile
				{
					ProfIntNullable = 10,
					ProfDecimalNullable = 160
				},
				MyAddressesNullable = new System.Collections.Generic.List<Address>
				{
					new Address
					{
						AddIntNullable = 10,
						AddDecimalNullable = 160
					},
					new Address
					{
						AddIntNullable = 10,
						AddDecimalNullable = 160
					},
				}
			};

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyStringNullable, x => x.NotDefaultOrEmpty().NotNull())

					.ForProperty(x => x.MyIntNullable, x => x
						.EqualsTo(5)
						.NotEqualsTo(10)
						.ExclusiveBetween(4, 6)
						.InclusiveBetween(5, 5)
						.GreaterThanOrEqual(15)
						.GreaterThan(14)
						.LessThanOrEqual(5)
						.LessThan(6))

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmpty())

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))


					.ForProperty(x => x.MyProfileNullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.MyProfileNullable, x => x.NotNull())
					.ForNavigation(x => x.MyProfileNullable, x => x
						.ForProperty(x => x.ProfStringNullable, x => x.NotDefaultOrEmpty().NotNull())

						.ForProperty(x => x.ProfIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(10)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(15)
							.GreaterThan(14)
							.LessThanOrEqual(5)
							.LessThan(6))

						.ForProperty(x => x.ProfDateTimeNullable, x => x.NotDefaultOrEmpty())

						.ForProperty(x => x.ProfDecimalNullable, x => x.PrecisionScale(4, 2, false)))

					.ForProperty(x => x.MyAddressesNullable, x => x.NotNull())
					.ForProperty(x => x.MyAddressesNullable, x => x.NotDefaultOrEmpty())
					.ForEach(x => x.MyAddressesNullable, x => x
						.ForProperty(x => x.AddStringNullable, x => x.NotDefaultOrEmpty().NotNull())

						.ForProperty(x => x.AddIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(10)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(15)
							.GreaterThan(14)
							.LessThanOrEqual(5)
							.LessThan(6))

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmpty())

						.ForProperty(x => x.AddDecimalNullable, x => x.PrecisionScale(4, 2, false)))
					;

			var result = validator.Validate(person);

			Assert.Equal(48, result.Errors.Count);
		}


		[Fact]
		public void Complex1()
		{
			var person = new Person
			{
				MyStringNullable = "test@email.com",
				MyIntNullable = 5,
				MyDateTimeNullable = DateTime.Now,
				MyDecimalNullable = 12.34m,
				MyProfileNullable = new Profile
				{
					ProfStringNullable = "test@email.com",
					ProfIntNullable = 5,
					ProfDateTimeNullable = DateTime.Now,
					ProfDecimalNullable = 12.34m,
				},
				MyAddressesNullable = new System.Collections.Generic.List<Address>
				{
					new Address
					{
						AddStringNullable = "test@email.com",
						AddIntNullable = 5,
						AddDateTimeNullable = DateTime.Now,
						AddDecimalNullable = 12.34m
					},
					new Address
					{
						AddStringNullable = "test@email.com",
						AddIntNullable = 5,
						AddDateTimeNullable = DateTime.Now,
						AddDecimalNullable = 12.34m
					},
				}
			};

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyStringNullable, x => x
						.EmailAddress()
						.MaxLength(14)
						.RegEx(@"^[a-z]{4}@[a-z]{5}.com$")
						.NotDefaultOrEmpty()
						.NotNull(),
						c => c.MyBoolNotNull == false)

					.ForProperty(x => x.MyIntNullable, x => x
						.EqualsTo(5)
						.NotEqualsTo(6)
						.ExclusiveBetween(4, 6)
						.InclusiveBetween(5, 5)
						.GreaterThanOrEqual(5)
						.GreaterThan(4)
						.LessThanOrEqual(5)
						.LessThan(6)
						.NotDefaultOrEmpty()
						.NotNull(),
						c => c.MyBoolNotNull == false)

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmpty())

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))


					.ForProperty(x => x.MyProfileNullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.MyProfileNullable, x => x.NotNull())
					.ForNavigation(x => x.MyProfileNullable, x => x
						.ForProperty(x => x.ProfStringNullable, x => x
							.EmailAddress()
							.MaxLength(14)
							.RegEx(@"^[a-z]{4}@[a-z]{5}.com$")
							.NotDefaultOrEmpty()
							.NotNull(),
							c => c.ProfBoolNotNull == false)

						.ForProperty(x => x.ProfIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(6)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(5)
							.GreaterThan(4)
							.LessThanOrEqual(5)
							.LessThan(6)
							.NotDefaultOrEmpty()
							.NotNull(),
							c => c.ProfBoolNotNull == false)

						.ForProperty(x => x.ProfDateTimeNullable, x => x.NotDefaultOrEmpty())

						.ForProperty(x => x.ProfDecimalNullable, x => x.PrecisionScale(4, 2, false)))

					.ForProperty(x => x.MyAddressesNullable, x => x.NotNull())
					.ForProperty(x => x.MyAddressesNullable, x => x.NotDefaultOrEmpty())
					.ForEach(x => x.MyAddressesNullable, x => x
						.ForProperty(x => x.AddStringNullable, x => x
							.EmailAddress()
							.MaxLength(14)
							.RegEx(@"^[a-z]{4}@[a-z]{5}.com$")
							.NotDefaultOrEmpty()
							.NotNull(),
							c => c.AddBoolNotNull == false)

						.ForProperty(x => x.AddIntNullable, x => x.EqualsTo(5)
							.EqualsTo(5)
							.NotEqualsTo(6)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(5)
							.GreaterThan(4)
							.LessThanOrEqual(5)
							.LessThan(6)
							.NotDefaultOrEmpty()
							.NotNull(),
							c => c.AddBoolNotNull == false)

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmpty())

						.ForProperty(x => x.AddDecimalNullable, x => x.PrecisionScale(4, 2, false)))
					;

			var result = validator.Validate(person);

			Assert.Equal(0, result.Errors.Count);
		}


		[Fact]
		public void Complex2()
		{
			var person = new Person
			{
				MyIntNullable = 10,
				MyDecimalNullable = 160,
				MyBoolNotNull = true,
				MyProfileNullable = new Profile
				{
					ProfIntNullable = 10,
					ProfDecimalNullable = 160,
					ProfBoolNotNull = true
				},
				MyAddressesNullable = new System.Collections.Generic.List<Address>
				{
					new Address
					{
						AddIntNullable = 10,
						AddDecimalNullable = 160,
						AddBoolNotNull = true
					},
					new Address
					{
						AddIntNullable = 10,
						AddDecimalNullable = 160,
						AddBoolNotNull = true
					},
				}
			};

			var validator = new Validator<Person>()
					.ForProperty(x => x.MyStringNullable, x => x.NotDefaultOrEmpty().NotNull(),
						c => c.MyBoolNotNull == false)

					.ForProperty(x => x.MyIntNullable, x => x
						.EqualsTo(5)
						.NotEqualsTo(10)
						.ExclusiveBetween(4, 6)
						.InclusiveBetween(5, 5)
						.GreaterThanOrEqual(15)
						.GreaterThan(14)
						.LessThanOrEqual(5)
						.LessThan(6),
						c => c.MyBoolNotNull == false)

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmpty(),
						c => c.MyBoolNotNull == false)

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false),
						c => c.MyBoolNotNull == false)


					.ForProperty(x => x.MyProfileNullable, x => x.NotDefaultOrEmpty(),
						c => c.MyBoolNotNull == false)
					.ForProperty(x => x.MyProfileNullable, x => x.NotNull(),
						c => c.MyBoolNotNull == false)
					.ForNavigation(x => x.MyProfileNullable, x => x
						.ForProperty(x => x.ProfStringNullable, x => x.NotDefaultOrEmpty().NotNull(),
						c => c.ProfBoolNotNull == false)

						.ForProperty(x => x.ProfIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(10)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(15)
							.GreaterThan(14)
							.LessThanOrEqual(5)
							.LessThan(6),
						c => c.ProfBoolNotNull == false)

						.ForProperty(x => x.ProfDateTimeNullable, x => x.NotDefaultOrEmpty(),
						c => c.ProfBoolNotNull == false)

						.ForProperty(x => x.ProfDecimalNullable, x => x.PrecisionScale(4, 2, false),
						c => c.ProfBoolNotNull == false))

					.ForProperty(x => x.MyAddressesNullable, x => x.NotNull(),
						c => c.MyBoolNotNull == false)
					.ForProperty(x => x.MyAddressesNullable, x => x.NotDefaultOrEmpty(),
						c => c.MyBoolNotNull == false)
					.ForEach(x => x.MyAddressesNullable, x => x
						.ForProperty(x => x.AddStringNullable, x => x.NotDefaultOrEmpty().NotNull(),
						c => c.AddBoolNotNull == false)

						.ForProperty(x => x.AddIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(10)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(15)
							.GreaterThan(14)
							.LessThanOrEqual(5)
							.LessThan(6),
						c => c.AddBoolNotNull == false)

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmpty(),
						c => c.AddBoolNotNull == false)

						.ForProperty(x => x.AddDecimalNullable, x => x.PrecisionScale(4, 2, false),
						c => c.AddBoolNotNull == false))
					;

			var result = validator.Validate(person);

			Assert.Equal(0, result.Errors.Count);
		}


		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Nullable_Chained_AllError_If(bool mainCondition)
		{
			var person = new Person
			{
				MyIntNullable = 10,
				MyDecimalNullable = 160,
				MyProfileNullable = new Profile
				{
					ProfIntNullable = 10,
					ProfDecimalNullable = 160
				},
				MyAddressesNullable = new System.Collections.Generic.List<Address>
				{
					new Address
					{
						AddIntNullable = 10,
						AddDecimalNullable = 160
					},
					new Address
					{
						AddIntNullable = 10,
						AddDecimalNullable = 160
					},
				}
			};

			var validator = new Validator<Person>()
				.If(a => mainCondition, y => y
					.ForProperty(x => x.MyStringNullable, x => x.NotDefaultOrEmpty().NotNull())

					.ForProperty(x => x.MyIntNullable, x => x
						.EqualsTo(5)
						.NotEqualsTo(10)
						.ExclusiveBetween(4, 6)
						.InclusiveBetween(5, 5)
						.GreaterThanOrEqual(15)
						.GreaterThan(14)
						.LessThanOrEqual(5)
						.LessThan(6))

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmpty())

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))


					.ForProperty(x => x.MyProfileNullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.MyProfileNullable, x => x.NotNull())
					.ForNavigation(x => x.MyProfileNullable, x => x
						.ForProperty(x => x.ProfStringNullable, x => x.NotDefaultOrEmpty().NotNull())

						.ForProperty(x => x.ProfIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(10)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(15)
							.GreaterThan(14)
							.LessThanOrEqual(5)
							.LessThan(6))

						.ForProperty(x => x.ProfDateTimeNullable, x => x.NotDefaultOrEmpty())

						.ForProperty(x => x.ProfDecimalNullable, x => x.PrecisionScale(4, 2, false)))

					.ForProperty(x => x.MyAddressesNullable, x => x.NotNull())
					.ForProperty(x => x.MyAddressesNullable, x => x.NotDefaultOrEmpty())
					.ForEach(x => x.MyAddressesNullable, x => x
						.ForProperty(x => x.AddStringNullable, x => x.NotDefaultOrEmpty().NotNull())

						.ForProperty(x => x.AddIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(10)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(15)
							.GreaterThan(14)
							.LessThanOrEqual(5)
							.LessThan(6))

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmpty())

						.ForProperty(x => x.AddDecimalNullable, x => x.PrecisionScale(4, 2, false))))
					;

			var result = validator.Validate(person);

			Assert.Equal(mainCondition ? 48 : 0, result.Errors.Count);
		}


		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Nullable_Chained_AllError_IfElse(bool mainCondition)
		{
			var person = new Person
			{
				MyIntNullable = 10,
				MyDecimalNullable = 160,
				MyProfileNullable = new Profile
				{
					ProfIntNullable = 10,
					ProfDecimalNullable = 160
				},
				MyAddressesNullable = new System.Collections.Generic.List<Address>
				{
					new Address
					{
						AddIntNullable = 10,
						AddDecimalNullable = 160
					},
					new Address
					{
						AddIntNullable = 10,
						AddDecimalNullable = 160
					},
				}
			};

			var validator = new Validator<Person>()
				.IfElse(a => mainCondition, y => y
					.ForProperty(x => x.MyStringNullable, x => x.NotDefaultOrEmpty().NotNull())

					.ForProperty(x => x.MyIntNullable, x => x
						.EqualsTo(5)
						.NotEqualsTo(10)
						.ExclusiveBetween(4, 6)
						.InclusiveBetween(5, 5)
						.GreaterThanOrEqual(15)
						.GreaterThan(14)
						.LessThanOrEqual(5)
						.LessThan(6))

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmpty())

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))


					.ForProperty(x => x.MyProfileNullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.MyProfileNullable, x => x.NotNull())
					.ForNavigation(x => x.MyProfileNullable, x => x
						.ForProperty(x => x.ProfStringNullable, x => x.NotDefaultOrEmpty().NotNull())

						.ForProperty(x => x.ProfIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(10)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(15)
							.GreaterThan(14)
							.LessThanOrEqual(5)
							.LessThan(6))

						.ForProperty(x => x.ProfDateTimeNullable, x => x.NotDefaultOrEmpty())

						.ForProperty(x => x.ProfDecimalNullable, x => x.PrecisionScale(4, 2, false)))

					.ForProperty(x => x.MyAddressesNullable, x => x.NotNull())
					.ForProperty(x => x.MyAddressesNullable, x => x.NotDefaultOrEmpty())
					.ForEach(x => x.MyAddressesNullable, x => x
						.ForProperty(x => x.AddStringNullable, x => x.NotDefaultOrEmpty().NotNull())

						.ForProperty(x => x.AddIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(10)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(15)
							.GreaterThan(14)
							.LessThanOrEqual(5)
							.LessThan(6))

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmpty())

						.ForProperty(x => x.AddDecimalNullable, x => x.PrecisionScale(4, 2, false))),
						y => y
							.ForProperty(x => x.MyIntNullable, v => v.LessThan(0)))
					;

			var result = validator.Validate(person);

			Assert.Equal(mainCondition ? 48 : 1, result.Errors.Count);
		}
	}
}
