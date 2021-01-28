using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raider.Queries.Aspects;
using Raider.Queries.Extensions;
using Raider.DependencyInjection;
using Raider.QueryServices.Aspects;
using System.Reflection;

namespace Raider.QueryServices.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRaiderQueryServices<TSearchBaseAssembly>(this IServiceCollection services)
			=> AddRaiderQueryServices(services, typeof(TSearchBaseAssembly).Assembly);

		public static IServiceCollection AddRaiderQueryServices(this IServiceCollection services, params Assembly[] assemblies)
		{
			services.TryAddTransient<ServiceFactory>(p => p.GetService);
			services.AddRaiderQueries(assemblies);

			services.TryAddTransient(typeof(IAsyncQueryInterceptor<,>), typeof(AsyncQueryInterceptor<,,,>));
			services.TryAddTransient(typeof(IQueryInterceptor<,>), typeof(QueryInterceptor<,,,>));

			services.TryAddTransient(typeof(AsyncQueryInterceptor<,,,>), typeof(AsyncQueryInterceptor<,,,>));
			services.TryAddTransient(typeof(QueryInterceptor<,,,>), typeof(QueryInterceptor<,,,>));

			return services;
		}
	}
}
