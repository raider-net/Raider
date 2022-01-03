using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using Raider.Database.PostgreSql;
using Raider.Infrastructure;
using Raider.Logging;
using Raider.ServiceBus.Components;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.PostgreSql.Messages.Storage.Model
{
	internal class DbComponentLog : Raider.Serializer.IDictionaryObject
	{
		public Guid IdComponent { get; set; }
		public int IdLogLevel { get; set; }
		public Guid RuntimeUniqueKey { get; set; }
		public DateTime TimeCreatedUtc { get; set; }
		public int IdComponentStatus { get; set; }
		public ILogMessage? LogMessage { get; set; }
		public string? LogDetail { get; set; }

		public readonly static List<string> PropertyNames;
		public readonly static Dictionary<string, NpgsqlDbType> PropertyTypeMapping;

		static DbComponentLog()
		{
			PropertyNames = new List<string>
			{
				nameof(IdComponent),
				nameof(IdLogLevel),
				nameof(RuntimeUniqueKey),
				nameof(TimeCreatedUtc),
				nameof(IdComponentStatus),
				nameof(LogMessage),
				nameof(LogDetail)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(IdComponent), NpgsqlDbType.Uuid },
				{ nameof(IdLogLevel), NpgsqlDbType.Integer },
				{ nameof(RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(TimeCreatedUtc), NpgsqlDbType.TimestampTz },
				{ nameof(IdComponentStatus), NpgsqlDbType.Integer },
				{ nameof(LogMessage), NpgsqlDbType.Jsonb },
				{ nameof(LogDetail), NpgsqlDbType.Varchar }
			};
		}

		private static DictionaryTable? _dictionaryTable;
		public static DictionaryTable GetDictionaryTable(IPostgreSqlServiceBusOptions options)
		{
			if (_dictionaryTable != null)
				return _dictionaryTable;

			_dictionaryTable = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = options.ComponentLogDbSchemaName,
				TableName = options.ComponentLogDbTableName,
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
					{ nameof(IdComponent), IdComponent },
					{ nameof(RuntimeUniqueKey), RuntimeUniqueKey },
					{ nameof(TimeCreatedUtc), TimeCreatedUtc },
					{ nameof(IdComponentStatus), IdComponentStatus }
				};

			if (LogMessage != null)
				dict.Add(nameof(LogMessage), serializer == null ? LogMessage.ToString() : serializer.SerializeAsString(LogMessage));

			if (!string.IsNullOrWhiteSpace(LogDetail))
				dict.Add(nameof(LogDetail), LogDetail);

			return dict;
		}
	}
}
