using NpgsqlTypes;
using Raider.Database.PostgreSql;
using Raider.Infrastructure;
using Raider.ServiceBus.Messages;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.PostgreSql.Messages.Storage.Model
{
	internal class DbMessageHeader : Raider.Serializer.IDictionaryObject
	{
		public Guid IdMessage { get; set; }
		public Guid IdMessageType { get; set; }
		public Guid? IdCorrespondingMessage { get; set; }
		public Guid IdSession { get; set; }
		public Guid IdSourceComponent { get; set; }
		public Guid? IdSourceResponseQueue { get; set; }
		public Guid IdTargetComponent { get; set; }
		public Guid IdTargetQueue { get; set; }
		public Guid? IdMessageBody { get; set; }
		public string? Description { get; set; }
		public int IdPriority { get; set; }
		public DateTime TimeCreatedUtc { get; set; }
		public DateTime? TimeLastProcessedUtc { get; set; }
		public int IdMessageStatus { get; set; }
		public Guid RuntimeUniqueKey { get; set; }
		public int RetryCount { get; set; }
		public DateTime? DelayedToUtc { get; set; }
		public Guid SyncToken { get; set; }

		public readonly static List<string> PropertyNames;
		public readonly static Dictionary<string, NpgsqlDbType> PropertyTypeMapping;

		static DbMessageHeader()
		{
			PropertyNames = new List<string>
			{
				nameof(IdMessage),
				nameof(IdMessageType),
				nameof(IdCorrespondingMessage),
				nameof(IdSession),
				nameof(IdSourceComponent),
				nameof(IdSourceResponseQueue),
				nameof(IdTargetComponent),
				nameof(IdTargetQueue),
				nameof(IdMessageBody),
				nameof(Description),
				nameof(IdPriority),
				nameof(TimeCreatedUtc),
				nameof(TimeLastProcessedUtc),
				nameof(IdMessageStatus),
				nameof(RuntimeUniqueKey),
				nameof(RetryCount),
				nameof(DelayedToUtc),
				nameof(SyncToken)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(IdMessage), NpgsqlDbType.Uuid },
				{ nameof(IdMessageType), NpgsqlDbType.Uuid },
				{ nameof(IdCorrespondingMessage), NpgsqlDbType.Uuid },
				{ nameof(IdSession), NpgsqlDbType.Uuid },
				{ nameof(IdSourceComponent), NpgsqlDbType.Uuid },
				{ nameof(IdSourceResponseQueue), NpgsqlDbType.Uuid },
				{ nameof(IdTargetComponent), NpgsqlDbType.Uuid },
				{ nameof(IdTargetQueue), NpgsqlDbType.Uuid },
				{ nameof(IdMessageBody), NpgsqlDbType.Uuid },
				{ nameof(Description), NpgsqlDbType.Varchar },
				{ nameof(IdPriority), NpgsqlDbType.Integer },
				{ nameof(TimeCreatedUtc), NpgsqlDbType.TimestampTz },
				{ nameof(TimeLastProcessedUtc), NpgsqlDbType.TimestampTz },
				{ nameof(IdMessageStatus), NpgsqlDbType.Integer },
				{ nameof(RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(RetryCount), NpgsqlDbType.Integer },
				{ nameof(DelayedToUtc), NpgsqlDbType.TimestampTz },
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
				SchemaName = options.MessageHeaderDbSchemaName,
				TableName = options.MessageHeaderDbTableName,
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
					{ nameof(IdMessage), IdMessage },
					{ nameof(IdMessageType), IdMessageType },
					{ nameof(IdSession), IdSession },
					{ nameof(IdSourceComponent), IdSourceComponent },
					{ nameof(IdTargetComponent), IdTargetComponent },
					{ nameof(IdTargetQueue), IdTargetQueue },
					{ nameof(IdPriority), IdPriority },
					{ nameof(TimeCreatedUtc), TimeCreatedUtc },
					{ nameof(IdMessageStatus), IdMessageStatus },
					{ nameof(RuntimeUniqueKey), RuntimeUniqueKey },
					{ nameof(RetryCount), RetryCount },
					{ nameof(SyncToken), SyncToken }
				};

			if (IdCorrespondingMessage.HasValue)
				dict.Add(nameof(IdCorrespondingMessage), IdCorrespondingMessage);

			if (IdSourceResponseQueue.HasValue)
				dict.Add(nameof(IdSourceResponseQueue), IdSourceResponseQueue);

			if (IdMessageBody.HasValue)
				dict.Add(nameof(IdMessageBody), IdMessageBody);

			if (!string.IsNullOrWhiteSpace(Description))
				dict.Add(nameof(Description), Description);

			if (TimeLastProcessedUtc.HasValue)
				dict.Add(nameof(TimeLastProcessedUtc), TimeLastProcessedUtc);

			if (DelayedToUtc.HasValue)
				dict.Add(nameof(DelayedToUtc), DelayedToUtc);

			return dict;
		}
	}
}
