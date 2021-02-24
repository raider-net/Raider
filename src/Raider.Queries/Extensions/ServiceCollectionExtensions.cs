using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raider.Queries.Internal;
using Raider.DependencyInjection;
using Raider.Exceptions;
using System;
using System.Linq;
using System.Reflection;

namespace Raider.Queries.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRaiderQueries<TSearchBaseAssembly>(this IServiceCollection services)
			=> AddRaiderQueries(services, typeof(TSearchBaseAssembly).Assembly);

		public static IServiceCollection AddRaiderQueries(this IServiceCollection services, params Assembly[] assemblies)
		{
			if (!assemblies.Any())
				throw new ArgumentNullException(nameof(assemblies), "At least one assembly is requred to scan for handlers.");

			services.TryAddTransient<ServiceFactory>(p => p.GetService);

			var registry = new QueryHandlerRegistry(services);

			var typesToScan =
				assemblies
					.Distinct()
					.SelectMany(a => a.DefinedTypes)
					.Where(type =>
						!type.IsInterface
						&& !type.IsAbstract);

			bool found = false;
			foreach (var typeInfo in typesToScan)
			{
				found = registry.TryRegisterHandler(typeInfo) || found;
			}

			//if (!found)
			//	throw new ConfigurationException("No query handler was found.");

			services.TryAddSingleton<IQueryHandlerRegistry>(registry);
			services.TryAddTransient<IQueryHandlerFactory, QueryHandlerFactory>();
			services.TryAddScoped<IQueryDispatcher, QueryDispatcher>();

			return services;
		}
	}
}
