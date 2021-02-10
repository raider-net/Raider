using Microsoft.Extensions.DependencyInjection;
using Raider.Validation.Extensions;
using Raider.Validation.Test.Model;
using System;
using Xunit;

namespace Raider.Validation.Test
{
	public class ServiceProviderTest
	{
		[Theory]
		[InlineData(typeof(TestCommand), true)]
		[InlineData(typeof(TestCommand2), false)]
		public void TestValidationManager(Type commandType, bool valid)
		{
			var services = new ServiceCollection();
			services.AddRaiderValidation<ServiceProviderTest>();
			var serviceProvider = services.BuildServiceProvider();

			var validationManager = serviceProvider.GetRequiredService<IValidationManager>();

			var descriptor = validationManager.GetValidationDescriptorFor(typeof(CItem), commandType);

			if (valid)
				Assert.NotNull(descriptor);
			else
				Assert.Null(descriptor);
		}
	}
}
