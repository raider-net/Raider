using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Config;
using Raider.ServiceBus.Config.Fluent;
using Raider.ServiceBus.Resolver;
using Raider.Text;
using Raider.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raider.ServiceBus.PostgreSql.Config
{
	public class PostgreSqlServiceBusOptions : IPostgreSqlServiceBusOptions, IPostgreSqlBusOptions, IServiceBusOptions, IBusOptions, IValidable
	{
		public string Name { get; set; }
		public ITypeResolver TypeResolver { get; set; }
		public Func<IServiceProvider, ISerializer> MessageSerializer { get; set; }
		public Func<IServiceProvider, IHostLogger> HostLogger { get; set; }
		public Func<IServiceProvider, IHandlerMessageLogger> MessageLogger { get; set; }
		public bool EnableMessageSerialization { get; set; }

		public List<ScenarioBuilder> Scenarios { get; }

		public string ConnectionString { get; set; }

		public string HostTypeDbSchemaName { get; set; }
		public string HostTypeDbTableName { get; set; }
		public string HostDbSchemaName { get; set; }
		public string HostDbTableName { get; set; }
		public string HostLogDbSchemaName { get; set; }
		public string HostLogDbTableName { get; set; }

		public string MessageTypeDbSchemaName { get; set; }

		public string MessageTypeDbTableName { get; set; }

		public string HandlerMessageDbSchemaName { get; set; }

		public string HandlerMessageDbTableName { get; set; }

		public string MessageBodyDbSchemaName { get; set; }

		public string MessageBodyDbTableName { get; set; }

		public string HandlerMessageLogDbSchemaName { get; set; }

		public string HandlerMessageLogDbTableName { get; set; }

		public string ScenarioDbSchemaName { get; set; }

		public string ScenarioDbTableName { get; set; }

		public string ComponentDbSchemaName { get; set; }

		public string ComponentDbTableName { get; set; }

		public string ComponentLogDbSchemaName { get; set; }

		public string ComponentLogDbTableName { get; set; }

		public string ComponentQueueDbSchemaName { get; set; }

		public string ComponentQueueDbTableName { get; set; }

		public string MessageSessionDbSchemaName { get; set; }

		public string MessageSessionDbTableName { get; set; }

		public string MessageHeaderDbSchemaName { get; set; }
				
		public string MessageHeaderDbTableName { get; set; }

		public string MessageLogDbSchemaName { get; set; }

		public string MessageLogDbTableName { get; set; }

		public PostgreSqlServiceBusOptions()
		{
			Scenarios = new List<ScenarioBuilder>();
		}

		public StringBuilder? Validate(string? propertyPrefix = null, StringBuilder? parentErrorBuffer = null, Dictionary<string, object>? validationContext = null)
		{
			if (string.IsNullOrWhiteSpace(Name))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(Name))} == null");
			}

			if (Scenarios.Count == 0)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(Scenarios))} is empty");
			}

			return parentErrorBuffer;
		}
	}
}
