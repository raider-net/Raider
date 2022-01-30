using Microsoft.Extensions.DependencyInjection;

namespace Raider.Plugins.DependencyInjection
{
	public interface IServiceCollectionConfigurator
	{
		bool ConfigureServiceCollection(IServiceCollection services, string pluginsDirectory, params object[] args);
	}
}
