using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raider.ServiceBus.Events;
using Raider.ServiceBus.Events.Extensions;
using Raider.ServiceBus.PostgreSql.Events.Storage;
using Raider.ServiceBus.Resolver;
using Raider.ServiceBus.Serializer;
using System;
using System.Reflection;

namespace Raider.ServiceBus.PostgreSql.Events.Providers
{
	public static class PostgreSqlEventBusServiceCollectionExtensions
	{
		public static IServiceCollection AddPostgreSqlEventBus<THandlersBaseAssembly>(this IServiceCollection services,
			Action<PostgreSqlEventBusBuilder> configure,
			ServiceLifetime handlerLifetime,
			ServiceLifetime interceptorLifetime)
			=> AddPostgreSqlEventBus(services, configure, handlerLifetime, interceptorLifetime, typeof(THandlersBaseAssembly).Assembly);

		public static IServiceCollection AddPostgreSqlEventBus(this IServiceCollection services,
			Action<PostgreSqlEventBusBuilder> configure,
			ServiceLifetime handlerLifetime,
			ServiceLifetime interceptorLifetime,
			params Assembly[] assembliesToScan)
		{
			var builder = new PostgreSqlEventBusBuilder()
				.EventHandlerContextType(typeof(IEventHandlerContext))
				.EventHandlerContextFactory(sp => new PostreHandlerContext())
				.TypeResolver(new FullNameTypeResolver())
				.EventSerializer(sp => new JsonSerializer())
				.HostTypeDbSchemaName("esb").HostTypeDbTableName("HostType")
				.HostDbSchemaName("esb").HostDbTableName("Host")
				.HostLogDbSchemaName("esb").HostLogDbTableName("HostLog")
				.EventTypeDbSchemaName("esb").EventTypeDbTableName("MessageType")
				.EventDbSchemaName("esb").EventDbTableName("HandlerMessage")
				.EventBodyDbSchemaName("esb").EventBodyDbTableName("MessageBody")
				.EventLogDbSchemaName("esb").EventLogDbTableName("HandlerMessageLog")
				.HostLogger(sp => sp.GetRequiredService<PostgreSqlEventBusStorage>())
				.EventLogger(sp => sp.GetRequiredService<PostgreSqlEventBusStorage>());

			configure?.Invoke(builder);
			var options = builder.GetOptions();

			services.AddSingleton<IPostgreSqlEventBusOptions>(options);

			services.TryAddSingleton(serviceProvider => new PostgreSqlEventBusStorage(options, serviceProvider));

			services.AddEventBusServices(options.EventHandlerContextType, options.TypeResolver, handlerLifetime, interceptorLifetime, assembliesToScan);

			services.TryAddScoped(serviceProvider => builder.Build(serviceProvider));

			return services;
		}
	}
}
