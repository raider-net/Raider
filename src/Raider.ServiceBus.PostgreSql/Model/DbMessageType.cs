using NpgsqlTypes;
using Raider.Database.PostgreSql;
using Raider.ServiceBus.Model;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.PostgreSql.Messages.Storage.Model
{
	internal class DbMessageType : IMessageType, Raider.Serializer.IDictionaryObject
	{
		public Guid IdMessageType { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }
		public int IdMessageMetaType { get; set; }
		public string CrlType { get; set; }

		public readonly static List<string> PropertyNames;
		public readonly static Dictionary<string, NpgsqlDbType> PropertyTypeMapping;

		static DbMessageType()
		{
			PropertyNames = new List<string>
			{
				nameof(IdMessageType),
				nameof(Name),
				nameof(Description),
				nameof(IdMessageMetaType),
				nameof(CrlType)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(IdMessageType), NpgsqlDbType.Uuid },
				{ nameof(Name), NpgsqlDbType.Varchar },
				{ nameof(Description), NpgsqlDbType.Varchar },
				{ nameof(IdMessageMetaType), NpgsqlDbType.Integer },
				{ nameof(CrlType), NpgsqlDbType.Varchar }
			};
		}

		private static DictionaryTable? _dictionaryTable;
		public static DictionaryTable GetDictionaryTable(IPostgreSqlBusOptions options)
		{
			if (_dictionaryTable != null)
				return _dictionaryTable;

			_dictionaryTable = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = options.MessageTypeDbSchemaName,
				TableName = options.MessageTypeDbTableName,
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
				{ nameof(IdMessageType), IdMessageType },
				{ nameof(Name), Name },
				{ nameof(IdMessageMetaType), IdMessageMetaType },
				{ nameof(CrlType), CrlType }
			};

			if (!string.IsNullOrWhiteSpace(Description))
				dict.Add(nameof(Description), Description);

			return dict;
		}
	}
}
