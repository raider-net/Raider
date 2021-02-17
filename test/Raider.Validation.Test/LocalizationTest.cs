using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Raider.Validation.Test.Fixtures;
using Raider.Validation.Test.Model;
using System;
using System.Globalization;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Raider.Validation.Test
{
	public class LocalizationTest : IClassFixture<LocalizationFixture>
	{
		private readonly ITestOutputHelper _output;
		private readonly IStringLocalizerFactory _stringLocalizerFactory;

		public LocalizationTest(ITestOutputHelper output, LocalizationFixture fixture)
		{
			_output = output ?? throw new ArgumentNullException(nameof(output));
			_stringLocalizerFactory = fixture.ServiceProvider.GetRequiredService<IStringLocalizerFactory>();
		}

		[Fact]
		[Trait("Category", "EN")]
		public void English()
		{
			ValidatorConfiguration.Localizer = _stringLocalizerFactory.Create("Validation", "Raider.Validation");
			CultureInfo.CurrentUICulture = new CultureInfo("en-US");

			var person = new Person
			{
				MyStringNullable = "test"
			};

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyStringNullable, x => x.EmailAddress());

			var result = validator.Validate(person);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_.MyStringNullable", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.Email, result.Errors[0].Type);
			Assert.Equal($"'{nameof(person.MyStringNullable)}' is not a valid email address.", result.Errors[0].MessageWithPropertyName);
		}

		[Fact]
		[Trait("Category", "SK")]
		public void Slovak()
		{
			var assemblyName = new AssemblyName(typeof(ValidatorBase).Assembly.FullName).Name;
			ValidatorConfiguration.Localizer = _stringLocalizerFactory.Create("Resources.Validation", assemblyName);
			CultureInfo.CurrentUICulture = new CultureInfo("sk-SK");

			var person = new Person
			{
				MyStringNullable = "test"
			};

			var validator = Validator<Person>.Rules()
					.ForProperty(x => x.MyStringNullable, x => x.EmailAddress());

			var result = validator.Validate(person);

			Assert.Equal(1, result.Errors.Count);
			Assert.Equal("_.MyStringNullable", result.Errors[0].ValidationFrame.ToString());
			Assert.Equal(ValidatorType.Email, result.Errors[0].Type);
			Assert.Equal($"Pole '{nameof(person.MyStringNullable)}' musí obsahovať platnú emailovú adresu.", result.Errors[0].MessageWithPropertyName);
		}
	}
}
