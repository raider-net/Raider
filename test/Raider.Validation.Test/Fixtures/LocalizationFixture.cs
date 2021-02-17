using Microsoft.Extensions.DependencyInjection;

namespace Raider.Validation.Test.Fixtures
{
	public class LocalizationFixture
	{
		public ServiceProvider ServiceProvider { get; private set; }

		public LocalizationFixture()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			serviceCollection.AddLocalization();
			ServiceProvider = serviceCollection.BuildServiceProvider();
		}
	}
}
