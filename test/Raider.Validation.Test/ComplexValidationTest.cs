using Raider.Validation.Test.Model;
using Raider.Validation.Test.Validators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
				ANullable = new A
				{
					AStringNullable = "test@email.com",
					AIntNullable = 5,
					ADateTimeNullable = DateTime.Now,
					ADecimalNullable = 12.34m,
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

			var validator = Validator<Person>.Rules()
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
					.ForProperty(x => x.MyIntNullable, x => x.NotDefaultOrEmptyNullable())
					.ForProperty(x => x.MyIntNullable, x => x.NotNull())

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmptyNullable())

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))


					.ForProperty(x => x.ANullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.ANullable, x => x.NotNull())
					.ForNavigation(x => x.ANullable, x => x
						.ForProperty(x => x.AStringNullable, x => x.EmailAddress())
						.ForProperty(x => x.AStringNullable, x => x.MaxLength(14))
						.ForProperty(x => x.AStringNullable, x => x.RegEx(@"^[a-z]{4}@[a-z]{5}.com$"))
						.ForProperty(x => x.AStringNullable, x => x.NotDefaultOrEmpty())
						.ForProperty(x => x.AStringNullable, x => x.NotNull())

						.ForProperty(x => x.AIntNullable, x => x.EqualsTo(5))
						.ForProperty(x => x.AIntNullable, x => x.NotEqualsTo(6))
						.ForProperty(x => x.AIntNullable, x => x.ExclusiveBetween(4, 6))
						.ForProperty(x => x.AIntNullable, x => x.InclusiveBetween(5, 5))
						.ForProperty(x => x.AIntNullable, x => x.GreaterThanOrEqual(5))
						.ForProperty(x => x.AIntNullable, x => x.GreaterThan(4))
						.ForProperty(x => x.AIntNullable, x => x.LessThanOrEqual(5))
						.ForProperty(x => x.AIntNullable, x => x.LessThan(6))
						.ForProperty(x => x.AIntNullable, x => x.NotDefaultOrEmptyNullable())
						.ForProperty(x => x.AIntNullable, x => x.NotNull())

						.ForProperty(x => x.ADateTimeNullable, x => x.NotDefaultOrEmptyNullable())

						.ForProperty(x => x.ADecimalNullable, x => x.PrecisionScale(4, 2, false)))

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
						.ForProperty(x => x.AddIntNullable, x => x.NotDefaultOrEmptyNullable())
						.ForProperty(x => x.AddIntNullable, x => x.NotNull())

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmptyNullable())

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
				ANullable = new A
				{
					AStringNullable = "test@email.com",
					AIntNullable = 5,
					ADateTimeNullable = DateTime.Now,
					ADecimalNullable = 12.34m,
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

			var validator = Validator<Person>.Rules()
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
						.NotDefaultOrEmptyNullable()
						.NotNull())

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmptyNullable())

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))


					.ForProperty(x => x.ANullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.ANullable, x => x.NotNull())
					.ForNavigation(x => x.ANullable, x => x
						.ForProperty(x => x.AStringNullable, x => x
							.EmailAddress()
							.MaxLength(14)
							.RegEx(@"^[a-z]{4}@[a-z]{5}.com$")
							.NotDefaultOrEmpty()
							.NotNull())

						.ForProperty(x => x.AIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(6)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(5)
							.GreaterThan(4)
							.LessThanOrEqual(5)
							.LessThan(6)
							.NotDefaultOrEmptyNullable()
							.NotNull())

						.ForProperty(x => x.ADateTimeNullable, x => x.NotDefaultOrEmptyNullable())

						.ForProperty(x => x.ADecimalNullable, x => x.PrecisionScale(4, 2, false)))

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
							.NotDefaultOrEmptyNullable()
							.NotNull())

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmptyNullable())

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
				ANullable = new A
				{
					AIntNullable = 10,
					ADecimalNullable = 160
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

			var validator = Validator<Person>.Rules()
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

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmptyNullable())

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))


					.ForProperty(x => x.ANullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.ANullable, x => x.NotNull())
					.ForNavigation(x => x.ANullable, x => x
						.ForProperty(x => x.AStringNullable, x => x.NotDefaultOrEmpty())
						.ForProperty(x => x.AStringNullable, x => x.NotNull())

						.ForProperty(x => x.AIntNullable, x => x.EqualsTo(5))
						.ForProperty(x => x.AIntNullable, x => x.NotEqualsTo(10))
						.ForProperty(x => x.AIntNullable, x => x.ExclusiveBetween(4, 6))
						.ForProperty(x => x.AIntNullable, x => x.InclusiveBetween(5, 5))
						.ForProperty(x => x.AIntNullable, x => x.GreaterThanOrEqual(15))
						.ForProperty(x => x.AIntNullable, x => x.GreaterThan(14))
						.ForProperty(x => x.AIntNullable, x => x.LessThanOrEqual(5))
						.ForProperty(x => x.AIntNullable, x => x.LessThan(6))

						.ForProperty(x => x.ADateTimeNullable, x => x.NotDefaultOrEmptyNullable())

						.ForProperty(x => x.ADecimalNullable, x => x.PrecisionScale(4, 2, false)))

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

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmptyNullable())

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
				ANullable = new A
				{
					AIntNullable = 10,
					ADecimalNullable = 160
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

			var validator = Validator<Person>.Rules()
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

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmptyNullable())

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))


					.ForProperty(x => x.ANullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.ANullable, x => x.NotNull())
					.ForNavigation(x => x.ANullable, x => x
						.ForProperty(x => x.AStringNullable, x => x.NotDefaultOrEmpty().NotNull())

						.ForProperty(x => x.AIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(10)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(15)
							.GreaterThan(14)
							.LessThanOrEqual(5)
							.LessThan(6))

						.ForProperty(x => x.ADateTimeNullable, x => x.NotDefaultOrEmptyNullable())

						.ForProperty(x => x.ADecimalNullable, x => x.PrecisionScale(4, 2, false)))

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

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmptyNullable())

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
				ANullable = new A
				{
					AStringNullable = "test@email.com",
					AIntNullable = 5,
					ADateTimeNullable = DateTime.Now,
					ADecimalNullable = 12.34m,
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

			var validator = Validator<Person>.Rules()
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
						.NotDefaultOrEmptyNullable()
						.NotNull(),
						c => c.MyBoolNotNull == false)

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmptyNullable())

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))


					.ForProperty(x => x.ANullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.ANullable, x => x.NotNull())
					.ForNavigation(x => x.ANullable, x => x
						.ForProperty(x => x.AStringNullable, x => x
							.EmailAddress()
							.MaxLength(14)
							.RegEx(@"^[a-z]{4}@[a-z]{5}.com$")
							.NotDefaultOrEmpty()
							.NotNull(),
							c => c.ABoolNotNull == false)

						.ForProperty(x => x.AIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(6)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(5)
							.GreaterThan(4)
							.LessThanOrEqual(5)
							.LessThan(6)
							.NotDefaultOrEmptyNullable()
							.NotNull(),
							c => c.ABoolNotNull == false)

						.ForProperty(x => x.ADateTimeNullable, x => x.NotDefaultOrEmptyNullable())

						.ForProperty(x => x.ADecimalNullable, x => x.PrecisionScale(4, 2, false)))

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
							.NotDefaultOrEmptyNullable()
							.NotNull(),
							c => c.AddBoolNotNull == false)

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmptyNullable())

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
				ANullable = new A
				{
					AIntNullable = 10,
					ADecimalNullable = 160,
					ABoolNotNull = true
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

			var validator = Validator<Person>.Rules()
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

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmptyNullable(),
						c => c.MyBoolNotNull == false)

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false),
						c => c.MyBoolNotNull == false)


					.ForProperty(x => x.ANullable, x => x.NotDefaultOrEmpty(),
						c => c.MyBoolNotNull == false)
					.ForProperty(x => x.ANullable, x => x.NotNull(),
						c => c.MyBoolNotNull == false)
					.ForNavigation(x => x.ANullable, x => x
						.ForProperty(x => x.AStringNullable, x => x.NotDefaultOrEmpty().NotNull(),
						c => c.ABoolNotNull == false)

						.ForProperty(x => x.AIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(10)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(15)
							.GreaterThan(14)
							.LessThanOrEqual(5)
							.LessThan(6),
						c => c.ABoolNotNull == false)

						.ForProperty(x => x.ADateTimeNullable, x => x.NotDefaultOrEmptyNullable(),
						c => c.ABoolNotNull == false)

						.ForProperty(x => x.ADecimalNullable, x => x.PrecisionScale(4, 2, false),
						c => c.ABoolNotNull == false))

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

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmptyNullable(),
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
				ANullable = new A
				{
					AIntNullable = 10,
					ADecimalNullable = 160
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

			var validator = Validator<Person>.Rules()
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

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmptyNullable())

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))


					.ForProperty(x => x.ANullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.ANullable, x => x.NotNull())
					.ForNavigation(x => x.ANullable, x => x
						.ForProperty(x => x.AStringNullable, x => x.NotDefaultOrEmpty().NotNull())

						.ForProperty(x => x.AIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(10)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(15)
							.GreaterThan(14)
							.LessThanOrEqual(5)
							.LessThan(6))

						.ForProperty(x => x.ADateTimeNullable, x => x.NotDefaultOrEmptyNullable())

						.ForProperty(x => x.ADecimalNullable, x => x.PrecisionScale(4, 2, false)))

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

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmptyNullable())

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
				ANullable = new A
				{
					AIntNullable = 10,
					ADecimalNullable = 160
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

			var validator = Validator<Person>.Rules()
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

					.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmptyNullable())

					.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))


					.ForProperty(x => x.ANullable, x => x.NotDefaultOrEmpty())
					.ForProperty(x => x.ANullable, x => x.NotNull())
					.ForNavigation(x => x.ANullable, x => x
						.ForProperty(x => x.AStringNullable, x => x.NotDefaultOrEmpty().NotNull())

						.ForProperty(x => x.AIntNullable, x => x
							.EqualsTo(5)
							.NotEqualsTo(10)
							.ExclusiveBetween(4, 6)
							.InclusiveBetween(5, 5)
							.GreaterThanOrEqual(15)
							.GreaterThan(14)
							.LessThanOrEqual(5)
							.LessThan(6))

						.ForProperty(x => x.ADateTimeNullable, x => x.NotDefaultOrEmptyNullable())

						.ForProperty(x => x.ADecimalNullable, x => x.PrecisionScale(4, 2, false)))

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

						.ForProperty(x => x.AddDateTimeNullable, x => x.NotDefaultOrEmptyNullable())

						.ForProperty(x => x.AddDecimalNullable, x => x.PrecisionScale(4, 2, false))),
						y => y
							.ForProperty(x => x.MyIntNullable, v => v.LessThan(0)))
					;

			var result = validator.Validate(person);

			Assert.Equal(mainCondition ? 48 : 1, result.Errors.Count);
		}


		[Fact]
		public void MultipleValidators()
		{
			var person = new Person
			{
				MyIntNullable = 10,
				MyDecimalNullable = 160,
				ANullable = new A
				{
					AIntNullable = 10,
					ADecimalNullable = 160,
					BNullable = new B
					{
						BIntNullable = 10,
						BDecimalNullable = 160,
						CNullable = new C
						{
							CIntNullable = 10,
							CDecimalNullable = 160,
							CItemsNullable = new System.Collections.Generic.List<CItem>
							{
								new CItem
								{
									CItemIntNullable = 10,
									CItemDecimalNullable = 160
								},
								new CItem
								{
									CItemIntNullable = 10,
									CItemDecimalNullable = 160
								}
							}
						},
						BItemsNullable = new System.Collections.Generic.List<BItem>
						{
							new BItem
							{
								BItemIntNullable = 10,
								BItemDecimalNullable = 160
							},
							new BItem
							{
								BItemIntNullable = 10,
								BItemDecimalNullable = 160
							}
						}
					},
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


			var barProfileItemValidator = Validator<CItem>.Rules()
				.ForProperty(x => x.CItemStringNullable, x => x.NotDefaultOrEmpty().NotNull())
				.ForProperty(x => x.CItemIntNullable, x => x
					.EqualsTo(5)
					.NotEqualsTo(10)
					.ExclusiveBetween(4, 6)
					.InclusiveBetween(5, 5)
					.GreaterThanOrEqual(15)
					.GreaterThan(14)
					.LessThanOrEqual(5)
					.LessThan(6))
				.ForProperty(x => x.CItemDateTimeNullable, x => x.NotDefaultOrEmptyNullable())
				.ForProperty(x => x.CItemDecimalNullable, x => x.PrecisionScale(4, 2, false));




			var barProfileValidator = Validator<C>.Rules()
				.ForProperty(x => x.CStringNullable, x => x.NotDefaultOrEmpty().NotNull())
				.ForProperty(x => x.CIntNullable, x => x
					.EqualsTo(5)
					.NotEqualsTo(10)
					.ExclusiveBetween(4, 6)
					.InclusiveBetween(5, 5)
					.GreaterThanOrEqual(15)
					.GreaterThan(14)
					.LessThanOrEqual(5)
					.LessThan(6))
				.ForProperty(x => x.CDateTimeNullable, x => x.NotDefaultOrEmptyNullable())
				.ForProperty(x => x.CDecimalNullable, x => x.PrecisionScale(4, 2, false))
				.ForEach(x => x.CItemsNullable, x => barProfileItemValidator.AttachTo(x));




			var fooProfileItemValidator = Validator<BItem>.Rules()
				.ForProperty(x => x.BItemStringNullable, x => x.NotDefaultOrEmpty().NotNull())
				.ForProperty(x => x.BItemIntNullable, x => x
					.EqualsTo(5)
					.NotEqualsTo(10)
					.ExclusiveBetween(4, 6)
					.InclusiveBetween(5, 5)
					.GreaterThanOrEqual(15)
					.GreaterThan(14)
					.LessThanOrEqual(5)
					.LessThan(6))
				.ForProperty(x => x.BItemDateTimeNullable, x => x.NotDefaultOrEmptyNullable())
				.ForProperty(x => x.BItemDecimalNullable, x => x.PrecisionScale(4, 2, false));




			var fooProfileValidator = Validator<B>.Rules()
				.ForProperty(x => x.BStringNullable, x => x.NotDefaultOrEmpty().NotNull())
				.ForProperty(x => x.BIntNullable, x => x
					.EqualsTo(5)
					.NotEqualsTo(10)
					.ExclusiveBetween(4, 6)
					.InclusiveBetween(5, 5)
					.GreaterThanOrEqual(15)
					.GreaterThan(14)
					.LessThanOrEqual(5)
					.LessThan(6))
				.ForProperty(x => x.BDateTimeNullable, x => x.NotDefaultOrEmptyNullable())
				.ForProperty(x => x.BDecimalNullable, x => x.PrecisionScale(4, 2, false))
				.ForNavigation(x => x.CNullable, x => barProfileValidator.AttachTo(x))
				.ForEach(x => x.BItemsNullable, x => fooProfileItemValidator.AttachTo(x));




			var profileValidator = Validator<A>.Rules()
				.ForProperty(x => x.AStringNullable, x => x.NotDefaultOrEmpty().NotNull())
				.ForProperty(x => x.AIntNullable, x => x
					.EqualsTo(5)
					.NotEqualsTo(10)
					.ExclusiveBetween(4, 6)
					.InclusiveBetween(5, 5)
					.GreaterThanOrEqual(15)
					.GreaterThan(14)
					.LessThanOrEqual(5)
					.LessThan(6))
				.ForProperty(x => x.ADateTimeNullable, x => x.NotDefaultOrEmptyNullable())
				.ForProperty(x => x.ADecimalNullable, x => x.PrecisionScale(4, 2, false))
				.ForNavigation(x => x.BNullable, x => fooProfileValidator.AttachTo(x));




			var personValidator = Validator<Person>.Rules()
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
				.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmptyNullable())
				.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))
				.ForNavigation(x => x.ANullable, x => profileValidator.AttachTo(x));




			var result = personValidator.Validate(person);

			Assert.Equal(96, result.Errors.Count);
		}


		[Fact]
		public void MultipleValidators2()
		{
			var person = new Person
			{
				MyIntNullable = 10,
				MyDecimalNullable = 160,
				ANullable = new A
				{
					AIntNullable = 10,
					ADecimalNullable = 160,
					BNullable = new B
					{
						BIntNullable = 10,
						BDecimalNullable = 160,
						CNullable = new C
						{
							CIntNullable = 10,
							CDecimalNullable = 160,
							CItemsNullable = new System.Collections.Generic.List<CItem>
							{
								new CItem
								{
									CItemIntNullable = 10,
									CItemDecimalNullable = 160
								},
								new CItem
								{
									CItemIntNullable = 10,
									CItemDecimalNullable = 160
								}
							}
						},
						BItemsNullable = new System.Collections.Generic.List<BItem>
						{
							new BItem
							{
								BItemIntNullable = 10,
								BItemDecimalNullable = 160
							},
							new BItem
							{
								BItemIntNullable = 10,
								BItemDecimalNullable = 160
							}
						}
					},
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


			var barProfileItemValidator = Validator<CItem>.Rules()
				.ForProperty(x => x.CItemStringNullable, x => x.NotDefaultOrEmpty());




			var barProfileValidator = Validator<C>.Rules()
				.ForProperty(x => x.CStringNullable, x => x.NotDefaultOrEmpty())
				.ForEach(x => x.CItemsNullable, x => barProfileItemValidator.AttachTo(x));




			var fooProfileItemValidator = Validator<BItem>.Rules()
				.ForProperty(x => x.BItemStringNullable, x => x.NotDefaultOrEmpty());




			var fooProfileValidator = Validator<B>.Rules()
				.ForProperty(x => x.BStringNullable, x => x.NotDefaultOrEmpty())
				.ForNavigation(x => x.CNullable, x => barProfileValidator.AttachTo(x))
				.ForEach(x => x.BItemsNullable, x => fooProfileItemValidator.AttachTo(x));




			var profileValidator = Validator<A>.Rules()
				.ForProperty(x => x.AStringNullable, x => x.NotDefaultOrEmpty())
				.ForNavigation(x => x.BNullable, x => fooProfileValidator.AttachTo(x));




			var personValidator = Validator<Person>.Rules()
				.ForProperty(x => x.MyStringNullable, x => x.NotDefaultOrEmpty())
				.ForNavigation(x => x.ANullable, x => profileValidator.AttachTo(x));




			var result = personValidator.Validate(person);

			Assert.Equal(8, result.Errors.Count);
		}

		private static List<IValidationDescriptor> MultipleValidators_Builder_DESCRIPTORS = new List<IValidationDescriptor>();

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void MultipleValidators_Builder(bool condition)
		{
			var person = new Person
			{
				MyIntNullable = 10,
				MyDecimalNullable = 160,
				ANullable = new A
				{
					AIntNullable = 10,
					ADecimalNullable = 160,
					BNullable = new B
					{
						BIntNullable = 10,
						BDecimalNullable = 160,
						CNullable = new C
						{
							CIntNullable = 10,
							CDecimalNullable = 160,
							CItemsNullable = new System.Collections.Generic.List<CItem>
							{
								new CItem
								{
									CItemIntNullable = 10,
									CItemDecimalNullable = 160
								},
								new CItem
								{
									CItemIntNullable = 10,
									CItemDecimalNullable = 160
								}
							}
						},
						BItemsNullable = new System.Collections.Generic.List<BItem>
						{
							new BItem
							{
								BItemIntNullable = 10,
								BItemDecimalNullable = 160
							},
							new BItem
							{
								BItemIntNullable = 10,
								BItemDecimalNullable = 160
							}
						}
					},
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


			var barProfileValidator = Validator<C>.Rules()
				.ForProperty(x => x.CStringNullable, x => x.NotDefaultOrEmpty().NotNull())
				.ForProperty(x => x.CIntNullable, x => x
					.EqualsTo(5)
					.NotEqualsTo(10)
					.ExclusiveBetween(4, 6)
					.InclusiveBetween(5, 5)
					.GreaterThanOrEqual(15)
					.GreaterThan(14)
					.LessThanOrEqual(5)
					.LessThan(6))
				.ForProperty(x => x.CDateTimeNullable, x => x.NotDefaultOrEmptyNullable())
				.ForProperty(x => x.CDecimalNullable, x => x.PrecisionScale(4, 2, false))
				.ForEach(x => x.CItemsNullable, x => new BarProfileItemValidator().Configure(condition).BuildRules(null, x));




			var fooProfileItemValidator = Validator<BItem>.Rules()
				.ForProperty(x => x.BItemStringNullable, x => x.NotDefaultOrEmpty().NotNull())
				.ForProperty(x => x.BItemIntNullable, x => x
					.EqualsTo(5)
					.NotEqualsTo(10)
					.ExclusiveBetween(4, 6)
					.InclusiveBetween(5, 5)
					.GreaterThanOrEqual(15)
					.GreaterThan(14)
					.LessThanOrEqual(5)
					.LessThan(6))
				.ForProperty(x => x.BItemDateTimeNullable, x => x.NotDefaultOrEmptyNullable())
				.ForProperty(x => x.BItemDecimalNullable, x => x.PrecisionScale(4, 2, false));




			var fooProfileValidator = Validator<B>.Rules()
				.ForProperty(x => x.BStringNullable, x => x.NotDefaultOrEmpty().NotNull())
				.ForProperty(x => x.BIntNullable, x => x
					.EqualsTo(5)
					.NotEqualsTo(10)
					.ExclusiveBetween(4, 6)
					.InclusiveBetween(5, 5)
					.GreaterThanOrEqual(15)
					.GreaterThan(14)
					.LessThanOrEqual(5)
					.LessThan(6))
				.ForProperty(x => x.BDateTimeNullable, x => x.NotDefaultOrEmptyNullable(), c => true)
				.ForProperty(x => x.BDecimalNullable, x => x.PrecisionScale(4, 2, false))
				.ForNavigation(x => x.CNullable, x => barProfileValidator.AttachTo(x))
				.ForEach(x => x.BItemsNullable, x => fooProfileItemValidator.AttachTo(x));




			var profileValidator = Validator<A>.Rules()
				.ForProperty(x => x.AStringNullable, x => x.NotDefaultOrEmpty().NotNull())
				.ForProperty(x => x.AIntNullable, x => x
					.EqualsTo(5)
					.NotEqualsTo(10)
					.ExclusiveBetween(4, 6)
					.InclusiveBetween(5, 5)
					.GreaterThanOrEqual(15)
					.GreaterThan(14)
					.LessThanOrEqual(5)
					.LessThan(6))
				.ForProperty(x => x.ADateTimeNullable, x => x.NotDefaultOrEmptyNullable())
				.ForProperty(x => x.ADecimalNullable, x => x.PrecisionScale(4, 2, false))
				.ForNavigation(x => x.BNullable, x => fooProfileValidator.AttachTo(x));




			var personValidator = Validator<Person>.Rules()
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
				.ForProperty(x => x.MyDateTimeNullable, x => x.NotDefaultOrEmptyNullable())
				.ForProperty(x => x.MyDecimalNullable, x => x.PrecisionScale(4, 2, false))
				.ForNavigation(x => x.ANullable, x => profileValidator.AttachTo(x));




			var result = personValidator.Validate(person);

			var desc = personValidator.ToDescriptor();
			MultipleValidators_Builder_DESCRIPTORS.Add(desc);

			if (1 < MultipleValidators_Builder_DESCRIPTORS.Count)
			{
				var desc0 = MultipleValidators_Builder_DESCRIPTORS[0];

				var str = desc0.Print();
				System.Diagnostics.Debug.WriteLine(str);

				for (int i = 1; i < MultipleValidators_Builder_DESCRIPTORS.Count; i++)
				{
					var d = MultipleValidators_Builder_DESCRIPTORS[i];

					Assert.True(desc0.IsEqualTo(d));
				}
			}

			Assert.Equal(condition ? 96 : 76, result.Errors.Count);
		}

		[Fact]
		public void Attached_Builders()
		{
			var person = new Person();

			var cValidator = Validator<C>.Rules()
				.ForNavigation(
					x => x.DNullable,
					x => x.ForNavigation(
						y => y.ENullable,
						y => y.ForNavigation(
							z => z.FNullable,
							z => z.ForProperty(p => p.FIntNullable, c => c.NotDefaultOrEmptyNullable()))));

			var personValidator = Validator<Person>.Rules()
				.ForNavigation(
					x => x.ANullable,
					x => x.ForNavigation(
						y => y.BNullable,
						y => y.ForNavigation(
							z => z.CNullable,
							z => cValidator.AttachTo(z))));

			var desc = personValidator.ToDescriptor();
			var descInfo = desc.Print();
			Debug.WriteLine(descInfo);

			var result = personValidator.Validate(person);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_.ANullable.BNullable.CNullable.DNullable.ENullable.FNullable.FIntNullable", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
		}

		[Fact]
		public void Attached_Conditional_Builders()
		{
			var person = new Person();

			var cValidator = Validator<C>.Rules()
				.ForNavigation(
					x => x.DNullable,
					x => x.ForNavigation(
						y => y.ENullable,
						y => y.ForNavigation(
							z => z.FNullable,
							z => z.ForProperty(p => p.FIntNullable, c => c.NotDefaultOrEmptyNullable()),
							c => true),
						c => true),
					c => true);

			var personValidator = Validator<Person>.Rules()
				.ForNavigation(
					x => x.ANullable,
					x => x.ForNavigation(
						y => y.BNullable,
						y => y.ForNavigation(
							z => z.CNullable,
							z => cValidator.AttachTo(z),
							c => true),
						c => true),
					c => true);

			var desc = personValidator.ToDescriptor();
			var descInfo = desc.Print();
			Debug.WriteLine(descInfo);

			var result = personValidator.Validate(person);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_.ANullable.BNullable.CNullable.DNullable.ENullable.FNullable.FIntNullable", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.NotDefaultOrEmpty, result.Errors[0].Type);
		}
	}
}
