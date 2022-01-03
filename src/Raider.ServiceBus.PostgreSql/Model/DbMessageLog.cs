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
	internal class DbMessageLog : Raider.Serializer.IDictionaryObject
	{
		public Guid IdMessage { get; set; }
		public int IdLogLevel { get; set; }
		public Guid? IdComponent { get; set; }
		public Guid RuntimeUniqueKey { get; set; }
		public DateTime TimeCreatedUtc { get; set; }
		public int IdMessageStatus { get; set; }
		public ILogMessage? LogMessage { get; set; }
		public string? LogDetail { get; set; }
		public int? RetryCount { get; set; }
		public DateTime? DelayedToUtc { get; set; }

		public readonly static List<string> PropertyNames;
		public readonly static Dictionary<string, NpgsqlDbType> PropertyTypeMapping;

		static DbMessageLog()
		{
			PropertyNames = new List<string>
			{
				nameof(IdMessage),
				nameof(IdLogLevel),
				nameof(IdComponent),
				nameof(RuntimeUniqueKey),
				nameof(TimeCreatedUtc),
				nameof(IdMessageStatus),
				nameof(LogMessage),
				nameof(LogDetail),
				nameof(RetryCount),
				nameof(DelayedToUtc)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(IdMessage), NpgsqlDbType.Uuid },
				{ nameof(IdLogLevel), NpgsqlDbType.Integer },
				{ nameof(IdComponent), NpgsqlDbType.Uuid },
				{ nameof(RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(TimeCreatedUtc), NpgsqlDbType.TimestampTz },
				{ nameof(IdMessageStatus), NpgsqlDbType.Integer },
				{ nameof(LogMessage), NpgsqlDbType.Jsonb },
				{ nameof(LogDetail), NpgsqlDbType.Varchar },
				{ nameof(RetryCount), NpgsqlDbType.Integer },
				{ nameof(DelayedToUtc), NpgsqlDbType.TimestampTz },
			};
		}

		private static DictionaryTable? _dictionaryTable;
		public static DictionaryTable GetDictionaryTable(IPostgreSqlServiceBusOptions options)
		{
			if (_dictionaryTable != null)
				return _dictionaryTable;

			_dictionaryTable = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = options.MessageLogDbSchemaName,
				TableName = options.MessageLogDbTableName,
				PropertyNames = PropertyNames,
				PropertyTypeMapping = PropertyTypeMapping
			});

			return _dictionaryTable;
		}

		private static string? _insertSql;
		public static string GetInsertSql(IPostgreSqlServiceBusOptions options)
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
					{ nameof(IdMessage), IdMessage },
					{ nameof(RuntimeUniqueKey), RuntimeUniqueKey },
					{ nameof(TimeCreatedUtc), TimeCreatedUtc },
					{ nameof(IdMessageStatus), IdMessageStatus }
				};

			if (IdComponent.HasValue)
				dict.Add(nameof(IdComponent), IdComponent);

			if (LogMessage != null)
				dict.Add(nameof(LogMessage), serializer == null ? LogMessage.ToString() : serializer.SerializeAsString(LogMessage));

			if (!string.IsNullOrWhiteSpace(LogDetail))
				dict.Add(nameof(LogDetail), LogDetail);

			if (RetryCount.HasValue)
				dict.Add(nameof(RetryCount), RetryCount);

			if (DelayedToUtc.HasValue)
				dict.Add(nameof(DelayedToUtc), DelayedToUtc);

			return dict;
		}
	}
}
