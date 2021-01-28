using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raider.Commands.Aspects;
using Raider.Commands.Extensions;
using Raider.DependencyInjection;
using Raider.Services.Aspects;
using System.Reflection;

namespace Raider.Services.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRaiderServices<TSearchBaseAssembly>(this IServiceCollection services)
			=> AddRaiderServices(services, typeof(TSearchBaseAssembly).Assembly);

		public static IServiceCollection AddRaiderServices(this IServiceCollection services, params Assembly[] assemblies)
		{
			services.TryAddTransient<ServiceFactory>(p => p.GetService);
			services.AddRaiderCommands(assemblies);

			services.TryAddTransient(typeof(IAsyncCommandInterceptor<,>), typeof(AsyncCommandInterceptor<,,,>));
			services.TryAddTransient(typeof(IAsyncCommandInterceptor<>), typeof(AsyncCommandInterceptor<,,>));
			services.TryAddTransient(typeof(ICommandInterceptor<,>), typeof(CommandInterceptor<,,,>));
			services.TryAddTransient(typeof(ICommandInterceptor<>), typeof(CommandInterceptor<,,>));

			services.TryAddTransient(typeof(AsyncCommandInterceptor<,,,>), typeof(AsyncCommandInterceptor<,,,>));
			services.TryAddTransient(typeof(AsyncCommandInterceptor<,,>), typeof(AsyncCommandInterceptor<,,>));
			services.TryAddTransient(typeof(CommandInterceptor<,,,>), typeof(CommandInterceptor<,,,>));
			services.TryAddTransient(typeof(CommandInterceptor<,,>), typeof(CommandInterceptor<,,>));

			return services;
		}
	}
}
