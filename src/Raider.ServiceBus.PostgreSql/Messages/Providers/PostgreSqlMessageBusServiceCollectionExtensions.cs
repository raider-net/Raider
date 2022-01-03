using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raider.ServiceBus.Messages;
using Raider.ServiceBus.Messages.Extensions;
using Raider.ServiceBus.PostgreSql.Messages.Storage;
using Raider.ServiceBus.PostgreSql.Storage;
using Raider.ServiceBus.Resolver;
using Raider.ServiceBus.Serializer;
using System;
using System.Reflection;

namespace Raider.ServiceBus.PostgreSql.Messages.Providers
{
	public static class PostgreSqlMessageBusServiceCollectionExtensions
	{
		public static IServiceCollection AddPostgreSqlMessageBus<THandlersBaseAssembly>(this IServiceCollection services,
			Action<PostgreSqlMessageBusBuilder> configure,
			ServiceLifetime handlerLifetime,
			ServiceLifetime interceptorLifetime)
			=> AddPostgreSqlMessageBus(services, configure, handlerLifetime, interceptorLifetime, typeof(THandlersBaseAssembly).Assembly);

		public static IServiceCollection AddPostgreSqlMessageBus(this IServiceCollection services,
			Action<PostgreSqlMessageBusBuilder> configure,
			ServiceLifetime handlerLifetime,
			ServiceLifetime interceptorLifetime,
			params Assembly[] assembliesToScan)
		{
			var builder = new PostgreSqlMessageBusBuilder()
				.MessageHandlerContextType(typeof(IMessageHandlerContext))
				.MessageHandlerContextFactory(sp => new PostreHandlerContext())
				.TypeResolver(new FullNameTypeResolver())
				.MessageSerializer(sp => new JsonSerializer())
				.HostTypeDbSchemaName("esb").HostTypeDbTableName("HostType")
				.HostDbSchemaName("esb").HostDbTableName("Host")
				.HostLogDbSchemaName("esb").HostLogDbTableName("HostLog")
				.MessageTypeDbSchemaName("esb").MessageTypeDbTableName("MessageType")
				.HandlerMessageDbSchemaName("esb").HandlerMessageDbTableName("HandlerMessage")
				.MessageBodyDbSchemaName("esb").MessageBodyDbTableName("MessageBody")
				.HandlerMessageLogDbSchemaName("esb").HandlerMessageLogDbTableName("HandlerMessageLog")
				.HostLogger(sp => sp.GetRequiredService<PostgreSqlMessageBusStorage>())
				.MessageLogger(sp => sp.GetRequiredService<PostgreSqlMessageBusStorage>());

			configure?.Invoke(builder);
			var options = builder.GetOptions();

			services.AddSingleton<IPostgreSqlMessageBusOptions>(options);

			services.TryAddSingleton(serviceProvider => new PostgreSqlMessageBusStorage(options, serviceProvider));

			services.AddMessageBusServices(options.MessageHandlerContextType, options.TypeResolver, handlerLifetime, interceptorLifetime, assembliesToScan);

			services.TryAddScoped(serviceProvider => builder.Build(serviceProvider));

			return services;
		}
	}
}
