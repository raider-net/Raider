using Raider.ServiceBus.Config;

namespace Raider.ServiceBus.PostgreSql
{
	public interface IPostgreSqlBusOptions : IBusOptions
	{
		string ConnectionString { get; }
		string HostTypeDbSchemaName { get; }
		string HostTypeDbTableName { get; }
		string HostDbSchemaName { get; }
		string HostDbTableName { get; }
		string HostLogDbSchemaName { get; }
		string HostLogDbTableName { get; }
		string MessageTypeDbSchemaName { get; }
		string MessageTypeDbTableName { get; }
		string HandlerMessageDbSchemaName { get; }
		string HandlerMessageDbTableName { get; }
		string MessageBodyDbSchemaName { get; }
		string MessageBodyDbTableName { get; }
		string HandlerMessageLogDbSchemaName { get; }
		string HandlerMessageLogDbTableName { get; }
	}
}
