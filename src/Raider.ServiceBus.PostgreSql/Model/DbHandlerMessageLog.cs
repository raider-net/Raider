using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using Raider.Database.PostgreSql;
using Raider.Infrastructure;
using Raider.Logging;
using Raider.ServiceBus.Messages;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.PostgreSql.Messages.Storage.Model
{
	internal class DbHandlerMessageLog : Raider.Serializer.IDictionaryObject
	{
		public Guid IdHandlerMessage { get; set; }
		public int IdLogLevel { get; set; }
		public Guid RuntimeUniqueKey { get; set; }
		public DateTime TimeCreatedUtc { get; set; }
		public int IdMessageStatus { get; set; }
		public ILogMessage? LogMessage { get; set; }
		public string? LogDetail { get; set; }

		public readonly static List<string> PropertyNames;
		public readonly static Dictionary<string, NpgsqlDbType> PropertyTypeMapping;

		static DbHandlerMessageLog()
		{
			PropertyNames = new List<string>
			{
				nameof(IdHandlerMessage),
				nameof(IdLogLevel),
				nameof(RuntimeUniqueKey),
				nameof(TimeCreatedUtc),
				nameof(IdMessageStatus),
				nameof(LogMessage),
				nameof(LogDetail)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(IdHandlerMessage), NpgsqlDbType.Uuid },
				{ nameof(IdLogLevel), NpgsqlDbType.Integer },
				{ nameof(RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(TimeCreatedUtc), NpgsqlDbType.TimestampTz },
				{ nameof(IdMessageStatus), NpgsqlDbType.Integer },
				{ nameof(LogMessage), NpgsqlDbType.Jsonb },
				{ nameof(LogDetail), NpgsqlDbType.Varchar }
			};
		}

		private static DictionaryTable? _dictionaryTable;
		public static DictionaryTable GetDictionaryTable(IPostgreSqlBusOptions options)
		{
			if (_dictionaryTable != null)
				return _dictionaryTable;

			_dictionaryTable = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = options.HandlerMessageLogDbSchemaName,
				TableName = options.HandlerMessageLogDbTableName,
				PropertyNames = PropertyNames,
				PropertyTypeMapping = PropertyTypeMapping
			});

			return _dictionaryTable;
		}

		private static string? _insertSql;
		public static string GetInsertSql(IPostgreSqlBusOptions options)
		{
			if (!string.IsNullOrWhiteSpace(_insertSql))
				return _insertSql;

			_insertSql = GetDictionaryTable(options).ToInsertSql();
			return _insertSql;
		}

		public IDictionary<string, object?> ToDictionary(Raider.Serializer.ISerializer? serializer = null)
		{
			var dict = new Dictionary<string, object?>
				{
					{ nameof(IdLogLevel), IdLogLevel },
					{ nameof(IdHandlerMessage), IdHandlerMessage },
					{ nameof(RuntimeUniqueKey), RuntimeUniqueKey },
					{ nameof(TimeCreatedUtc), TimeCreatedUtc },
					{ nameof(IdMessageStatus), IdMessageStatus }
				};

			if (LogMessage != null)
				dict.Add(nameof(LogMessage), serializer == null ? LogMessage.ToString() : serializer.SerializeAsString(LogMessage));

			if (!string.IsNullOrWhiteSpace(LogDetail))
				dict.Add(nameof(LogDetail), LogDetail);

			return dict;
		}
	}
}
