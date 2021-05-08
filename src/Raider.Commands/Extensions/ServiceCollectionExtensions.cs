using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raider.Commands.Internal;
using System;
using System.Linq;
using System.Reflection;

namespace Raider.Commands.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRaiderCommands<TSearchBaseAssembly>(this IServiceCollection services)
			=> AddRaiderCommands(services, typeof(TSearchBaseAssembly).Assembly);

		public static IServiceCollection AddRaiderCommands(this IServiceCollection services, params Assembly[] assemblies)
		{
			if (!assemblies.Any())
				throw new ArgumentNullException(nameof(assemblies), "At least one assembly is requred to scan for handlers.");

			var registry = new CommandHandlerRegistry(services);

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
			//	throw new ConfigurationException("No command handler was found.");

			services.TryAddSingleton<ICommandHandlerRegistry>(registry);
			services.TryAddTransient<ICommandHandlerFactory, CommandHandlerFactory>();
			services.TryAddScoped<ICommandDispatcher, CommandDispatcher>();

			return services;
		}
	}
}
