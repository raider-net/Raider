using NpgsqlTypes;
using Raider.Database.PostgreSql;
using Raider.Infrastructure;
using Raider.ServiceBus.Messages;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.PostgreSql.Messages.Storage.Model
{
	internal class DbHandlerMessage : Raider.Serializer.IDictionaryObject
	{
		public Guid IdHandlerMessage { get; set; }
		public Guid IdHost { get; set; }
		public Guid IdMessageType { get; set; }
		public Guid? IdCorrespondingMessage { get; set; }
		public Guid? IdMessageBody { get; set; }
		public DateTime TimeCreatedUtc { get; set; }
		public int IdMessageStatus { get; set; }
		public Guid RuntimeUniqueKey { get; set; }
		public Guid? IdSession { get; set; }
		public Guid SyncToken { get; set; }

		public readonly static List<string> PropertyNames;
		public readonly static Dictionary<string, NpgsqlDbType> PropertyTypeMapping;

		static DbHandlerMessage()
		{
			PropertyNames = new List<string>
			{
				nameof(IdHandlerMessage),
				nameof(IdHost),
				nameof(IdMessageType),
				nameof(IdCorrespondingMessage),
				nameof(IdMessageBody),
				nameof(TimeCreatedUtc),
				nameof(IdMessageStatus),
				nameof(RuntimeUniqueKey),
				nameof(IdSession),
				nameof(SyncToken)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(IdHandlerMessage), NpgsqlDbType.Uuid },
				{ nameof(IdHost), NpgsqlDbType.Uuid },
				{ nameof(IdMessageType), NpgsqlDbType.Uuid },
				{ nameof(IdCorrespondingMessage), NpgsqlDbType.Uuid },
				{ nameof(IdMessageBody), NpgsqlDbType.Uuid },
				{ nameof(TimeCreatedUtc), NpgsqlDbType.TimestampTz },
				{ nameof(IdMessageStatus), NpgsqlDbType.Integer },
				{ nameof(RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(IdSession), NpgsqlDbType.Uuid },
				{ nameof(SyncToken), NpgsqlDbType.Uuid }
			};
		}

		private static DictionaryTable? _dictionaryTable;
		public static DictionaryTable GetDictionaryTable(IPostgreSqlBusOptions options)
		{
			if (_dictionaryTable != null)
				return _dictionaryTable;

			_dictionaryTable = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = options.HandlerMessageDbSchemaName,
				TableName = options.HandlerMessageDbTableName,
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
					{ nameof(IdHandlerMessage), IdHandlerMessage },
					{ nameof(IdHost), IdHost },
					{ nameof(IdMessageType), IdMessageType },
					{ nameof(TimeCreatedUtc), TimeCreatedUtc },
					{ nameof(IdMessageStatus), IdMessageStatus },
					{ nameof(RuntimeUniqueKey), RuntimeUniqueKey },
					{ nameof(SyncToken), SyncToken }
				};

			if (IdCorrespondingMessage.HasValue)
				dict.Add(nameof(IdCorrespondingMessage), IdCorrespondingMessage);

			if (IdMessageBody.HasValue)
				dict.Add(nameof(IdMessageBody), IdMessageBody);

			if (IdSession.HasValue)
				dict.Add(nameof(IdSession), IdSession);

			return dict;
		}
	}
}
