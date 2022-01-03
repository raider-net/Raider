using Raider.ServiceBus.Config;
using Raider.Validation;

namespace Raider.ServiceBus.PostgreSql
{
	public interface IPostgreSqlServiceBusOptions : IPostgreSqlBusOptions, IServiceBusOptions, IValidable
	{
		string ScenarioDbSchemaName { get; }
		string ScenarioDbTableName { get; }
		string ComponentDbSchemaName { get; }
		string ComponentDbTableName { get; }
		string ComponentLogDbSchemaName { get; }
		string ComponentLogDbTableName { get; }
		string ComponentQueueDbSchemaName { get; }
		string ComponentQueueDbTableName { get; }
		string MessageSessionDbSchemaName { get; }
		string MessageSessionDbTableName { get; }
		string MessageHeaderDbSchemaName { get; }
		string MessageHeaderDbTableName { get; }
		string MessageLogDbSchemaName { get; }
		string MessageLogDbTableName { get; }
	}
}
