using NpgsqlTypes;
using Raider.Database.PostgreSql;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.PostgreSql.Messages.Storage.Model
{
	internal class DbComponentQueue : Raider.Serializer.IDictionaryObject
	{
		public Guid IdComponentQueue { get; set; }
		public Guid IdComponent { get; set; }
		public Guid IdMessageType { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }
		public DateTime? LastMessageDeliveryUtc { get; set; }
		public bool IsFIFO { get; set; }
		public int ProcessingTimeoutInSeconds { get; set; }
		public int MaxRetryCount { get; set; }
		public Guid SyncToken { get; set; }

		public readonly static List<string> PropertyNames;
		public readonly static Dictionary<string, NpgsqlDbType> PropertyTypeMapping;

		static DbComponentQueue()
		{
			PropertyNames = new List<string>
			{
				nameof(IdComponentQueue),
				nameof(IdComponent),
				nameof(IdMessageType),
				nameof(Name),
				nameof(Description),
				nameof(LastMessageDeliveryUtc),
				nameof(IsFIFO),
				nameof(ProcessingTimeoutInSeconds),
				nameof(MaxRetryCount),
				nameof(SyncToken)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(IdComponentQueue), NpgsqlDbType.Uuid },
				{ nameof(IdComponent), NpgsqlDbType.Uuid },
				{ nameof(IdMessageType), NpgsqlDbType.Uuid },
				{ nameof(Name), NpgsqlDbType.Varchar },
				{ nameof(Description), NpgsqlDbType.Varchar },
				{ nameof(LastMessageDeliveryUtc), NpgsqlDbType.TimestampTz },
				{ nameof(IsFIFO), NpgsqlDbType.Boolean },
				{ nameof(ProcessingTimeoutInSeconds), NpgsqlDbType.Integer },
				{ nameof(MaxRetryCount), NpgsqlDbType.Integer },
				{ nameof(SyncToken), NpgsqlDbType.Uuid }
			};
		}

		private static DictionaryTable? _dictionaryTable;
		public static DictionaryTable GetDictionaryTable(IPostgreSqlServiceBusOptions options)
		{
			if (_dictionaryTable != null)
				return _dictionaryTable;

			_dictionaryTable = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = options.ComponentQueueDbSchemaName,
				TableName = options.ComponentQueueDbTableName,
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
					{ nameof(IdComponentQueue), IdComponentQueue },
					{ nameof(IdComponent), IdComponent },
					{ nameof(IdMessageType), IdMessageType },
					{ nameof(Name), Name },
					{ nameof(IsFIFO), IsFIFO },
					{ nameof(ProcessingTimeoutInSeconds), ProcessingTimeoutInSeconds },
					{ nameof(MaxRetryCount), MaxRetryCount },
					{ nameof(SyncToken), SyncToken }
				};

			if (!string.IsNullOrWhiteSpace(Description))
				dict.Add(nameof(Description), Description);

			if (LastMessageDeliveryUtc.HasValue)
				dict.Add(nameof(LastMessageDeliveryUtc), LastMessageDeliveryUtc);

			return dict;
		}
	}
}
