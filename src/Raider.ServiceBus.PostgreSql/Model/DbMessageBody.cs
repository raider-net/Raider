using NpgsqlTypes;
using Raider.Database.PostgreSql;
using Raider.ServiceBus.PostgreSql.Messages.Providers;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.PostgreSql.Messages.Storage.Model
{
	internal class DbMessageBody : Raider.Serializer.IDictionaryObject
	{
		public Guid IdMessageBody { get; set; }
		public Guid IdMessageType { get; set; }
		public string Data { get; set; }

		public readonly static List<string> PropertyNames;
		public readonly static Dictionary<string, NpgsqlDbType> PropertyTypeMapping;

		static DbMessageBody()
		{
			PropertyNames = new List<string>
			{
				nameof(IdMessageBody),
				nameof(IdMessageType),
				nameof(Data)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(IdMessageBody), NpgsqlDbType.Uuid },
				{ nameof(IdMessageType), NpgsqlDbType.Uuid },
				{ nameof(Data), NpgsqlDbType.Jsonb }
			};
		}

		private static DictionaryTable? _dictionaryTable;
		public static DictionaryTable GetDictionaryTable(IPostgreSqlBusOptions options)
		{
			if (_dictionaryTable != null)
				return _dictionaryTable;

			_dictionaryTable = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = options.MessageBodyDbSchemaName,
				TableName = options.MessageBodyDbTableName,
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
					{ nameof(IdMessageBody), IdMessageBody },
					{ nameof(IdMessageType), IdMessageType },
					{ nameof(Data), Data }
				};

			return dict;
		}
	}
}
