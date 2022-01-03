using NpgsqlTypes;
using Raider.Database.PostgreSql;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.PostgreSql.Messages.Storage.Model
{
	internal class DbMessageSession : Raider.Serializer.IDictionaryObject
	{
		public Guid IdMessageSession { get; set; }
		public Guid IdComponent { get; set; }
		public DateTime TimeCreatedUtc { get; set; }
		public Guid RuntimeUniqueKey { get; set; }
		public string? State { get; set; }
		public string? StateCrlType { get; set; }

		public readonly static List<string> PropertyNames;
		public readonly static Dictionary<string, NpgsqlDbType> PropertyTypeMapping;

		static DbMessageSession()
		{
			PropertyNames = new List<string>
			{
				nameof(IdMessageSession),
				nameof(IdComponent),
				nameof(TimeCreatedUtc),
				nameof(RuntimeUniqueKey),
				nameof(State),
				nameof(StateCrlType)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(IdMessageSession), NpgsqlDbType.Uuid },
				{ nameof(IdComponent), NpgsqlDbType.Uuid },
				{ nameof(TimeCreatedUtc), NpgsqlDbType.TimestampTz },
				{ nameof(RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(State), NpgsqlDbType.Jsonb },
				{ nameof(StateCrlType), NpgsqlDbType.Varchar }
			};
		}

		private static DictionaryTable? _dictionaryTable;
		public static DictionaryTable GetDictionaryTable(IPostgreSqlServiceBusOptions options)
		{
			if (_dictionaryTable != null)
				return _dictionaryTable;

			_dictionaryTable = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = options.MessageSessionDbSchemaName,
				TableName = options.MessageSessionDbTableName,
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
					{ nameof(IdMessageSession), IdMessageSession },
					{ nameof(IdComponent), IdComponent },
					{ nameof(TimeCreatedUtc), TimeCreatedUtc },
					{ nameof(RuntimeUniqueKey), RuntimeUniqueKey }
				};

			if (!string.IsNullOrWhiteSpace(State))
				dict.Add(nameof(State), State);

			if (!string.IsNullOrWhiteSpace(StateCrlType))
				dict.Add(nameof(StateCrlType), StateCrlType);

			return dict;
		}
	}
}
