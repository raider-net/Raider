using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Raider.Extensions;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Events.Internal;
using Raider.ServiceBus.Events.Providers;
using Raider.ServiceBus.Resolver;
using System;
using System.Linq;
using System.Reflection;

namespace Raider.ServiceBus.Events.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddEventBusServices(this IServiceCollection services,
			Type eventHandlerContextType,
			ITypeResolver typeResolver,
			ServiceLifetime handlerLifetime,
			ServiceLifetime interceptorLifetime,
			params Assembly[] assembliesToScan)
		{
			if (!assembliesToScan.Any())
				throw new ArgumentNullException(nameof(assembliesToScan), "At least one assembly is requred to scan for event handlers.");

			var registry = new EventHandlerRegistry(services, eventHandlerContextType ,typeResolver, Logging.Logger.GetLogger<EventHandlerRegistry>(), handlerLifetime, interceptorLifetime);

			var typesToScan =
				assembliesToScan
					.Distinct()
					.SelectMany(a => a.DefinedTypes)
					.Where(type => type.IsInstanceable());

			bool found = false;
			foreach (var typeInfo in typesToScan)
			{
				found = registry.TryRegisterHandlerAndInterceptor(typeInfo) || found;
			}

			services.TryAddSingleton<IEventHandlerRegistry>(registry);
			services.TryAddSingleton<IEventTypeRegistry>(registry);
			return services;
		}

		public static IServiceCollection AddInMemoryEventBus<THandlersBaseAssembly>(this IServiceCollection services,
			Action<InMemoryEventBusBuilder> configure,
			ServiceLifetime handlerLifetime,
			ServiceLifetime interceptorLifetime)
			=> AddInMemoryEventBus(services, configure, handlerLifetime, interceptorLifetime, typeof(THandlersBaseAssembly).Assembly);

		public static IServiceCollection AddInMemoryEventBus(this IServiceCollection services,
			Action<InMemoryEventBusBuilder> configure,
			ServiceLifetime handlerLifetime,
			ServiceLifetime interceptorLifetime,
			params Assembly[] assembliesToScan)
		{
			var builder = new InMemoryEventBusBuilder()
				.EventHandlerContextType(typeof(IEventHandlerContext))
				.EventHandlerContextFactory(sp => new EventContext())
				.TypeResolver(new FullNameTypeResolver())
				.HostLogger(sp => new BaseHostLogger(sp.GetRequiredService<ILogger<BaseHostLogger>>()))
				.EventLogger(sp => new BaseHandlerMessageLogger(sp.GetRequiredService<ILogger<BaseHandlerMessageLogger>>()));

			configure?.Invoke(builder);
			var options = builder.GetOptions();
			services.AddSingleton<IInMemoryEventBusOptions>(options);

			services.AddEventBusServices(options.EventHandlerContextType, options.TypeResolver, handlerLifetime, interceptorLifetime, assembliesToScan);

			services.TryAddScoped(serviceProvider => builder.Build(serviceProvider));

			return services;
		}
	}
}
