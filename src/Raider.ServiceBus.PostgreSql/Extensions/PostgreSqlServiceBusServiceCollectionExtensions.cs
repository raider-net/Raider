using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raider.ServiceBus.PostgreSql.Config.Fluent;
using Raider.ServiceBus.PostgreSql.Storage;
using Raider.ServiceBus.Resolver;
using Raider.ServiceBus.Serializer;
using System;

namespace Raider.ServiceBus.PostgreSql.Extensions
{
	public static class PostgreSqlServiceBusServiceCollectionExtensions
	{
		public static IServiceCollection AddPostgreSqlServiceBus(this IServiceCollection services,
			Action<PostgreSqlServiceBusBuilder> configure)
		{
			var builder = new PostgreSqlServiceBusBuilder()
				.TypeResolver(new FullNameTypeResolver())
				.MessageSerializer(sp => new JsonSerializer())
				.HostTypeDbSchemaName("esb").HostTypeDbTableName("HostType")
				.HostDbSchemaName("esb").HostDbTableName("Host")
				.HostLogDbSchemaName("esb").HostLogDbTableName("HostLog")
				.MessageTypeDbSchemaName("esb").MessageTypeDbTableName("MessageType")
				.HandlerMessageDbSchemaName("esb").HandlerMessageDbTableName("HandlerMessage")
				.MessageBodyDbSchemaName("esb").MessageBodyDbTableName("MessageBody")
				.HandlerMessageLogDbSchemaName("esb").HandlerMessageLogDbTableName("HandlerMessageLog")
				.ScenarioDbSchemaName("esb").ScenarioDbTableName("Scenario")
				.ComponentDbSchemaName("esb").ComponentDbTableName("Component")
				.ComponentLogDbSchemaName("esb").ComponentLogDbTableName("ComponentLog")
				.ComponentQueueDbSchemaName("esb").ComponentQueueDbTableName("ComponentQueue")
				.MessageSessionDbSchemaName("esb").MessageSessionDbTableName("MessageSession")
				.MessageHeaderDbSchemaName("esb").MessageHeaderDbTableName("MessageHeader")
				.MessageLogDbSchemaName("esb").MessageLogDbTableName("MessageLog")
				.HostLogger(sp => sp.GetRequiredService<PostgreSqlServiceBusStorage>())
				//.MessageLogger(sp => sp.GetRequiredService<PostgreSqlServiceBusStorage>())
				;

			configure?.Invoke(builder);
			var options = builder.GetOptions();

			services.AddSingleton<IPostgreSqlServiceBusOptions>(options);

			services.TryAddSingleton(serviceProvider => new PostgreSqlServiceBusStorage(options, serviceProvider));

			services.TryAddScoped(serviceProvider => builder.Build(serviceProvider));

			return services;
		}
	}
}
