using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Raider.Extensions;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Messages.Internal;
using Raider.ServiceBus.Messages.Providers;
using Raider.ServiceBus.Resolver;
using System;
using System.Linq;
using System.Reflection;

namespace Raider.ServiceBus.Messages.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddMessageBusServices(this IServiceCollection services,
			Type messageHandlerContextType,
			ITypeResolver typeResolver,
			ServiceLifetime handlerLifetime,
			ServiceLifetime interceptorLifetime,
			params Assembly[] assembliesToScan)
		{
			if (!assembliesToScan.Any())
				throw new ArgumentNullException(nameof(assembliesToScan), "At least one assembly is requred to scan for message handlers.");

			var registry = new MessageHandlerRegistry(services, messageHandlerContextType ,typeResolver, Logging.Logger.GetLogger<MessageHandlerRegistry>(), handlerLifetime, interceptorLifetime);

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

			services.TryAddSingleton<IMessageHandlerRegistry>(registry);
			services.TryAddSingleton<IMessageTypeRegistry>(registry);
			return services;
		}

		public static IServiceCollection AddInMemoryMessageBus<THandlersBaseAssembly>(this IServiceCollection services,
			Action<InMemoryMessageBusBuilder> configure,
			ServiceLifetime handlerLifetime,
			ServiceLifetime interceptorLifetime)
			=> AddInMemoryMessageBus(services, configure, handlerLifetime, interceptorLifetime, typeof(THandlersBaseAssembly).Assembly);

		public static IServiceCollection AddInMemoryMessageBus(this IServiceCollection services,
			Action<InMemoryMessageBusBuilder> configure,
			ServiceLifetime handlerLifetime,
			ServiceLifetime interceptorLifetime,
			params Assembly[] assembliesToScan)
		{
			var builder = new InMemoryMessageBusBuilder()
				.MessageHandlerContextType(typeof(IMessageHandlerContext))
				.MessageHandlerContextFactory(sp => new HandlerContext())
				.TypeResolver(new FullNameTypeResolver())
				.HostLogger(sp => new BaseHostLogger(sp.GetRequiredService<ILogger<BaseHostLogger>>()))
				.MessageLogger(sp => new BaseHandlerMessageLogger(sp.GetRequiredService<ILogger<BaseHandlerMessageLogger>>()));

			configure?.Invoke(builder);
			var options = builder.GetOptions();
			services.AddSingleton<IInMemoryMessageBusOptions>(options);

			services.AddMessageBusServices(options.MessageHandlerContextType, options.TypeResolver, handlerLifetime, interceptorLifetime, assembliesToScan);

			services.TryAddScoped(serviceProvider => builder.Build(serviceProvider));

			return services;
		}
	}
}
