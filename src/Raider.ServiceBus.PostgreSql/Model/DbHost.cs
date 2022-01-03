using NpgsqlTypes;
using Raider.Converters;
using Raider.Database.PostgreSql;
using Raider.Infrastructure;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.PostgreSql.Messages.Storage.Model
{
	internal class DbHost : IHost, Raider.Serializer.IDictionaryObject
	{
		public Guid IdHost { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }
		public Guid IdHostType { get; set; }
		public bool Disabled { get; set; }
		public Guid CurrentRuntimeUniqueKey { get; set; }
		public DateTime LastStartTimeUtc { get; set; }
		public DateTime LastHeartbeatUtc { get; set; }
		public int IdHostStatus { get; set; }
		public Guid SyncToken { get; set; }

		public readonly static List<string> PropertyNames;
		public readonly static Dictionary<string, NpgsqlDbType> PropertyTypeMapping;

		static DbHost()
		{
			PropertyNames = new List<string>
			{
				nameof(IdHost),
				nameof(Name),
				nameof(Description),
				nameof(IdHostType),
				nameof(Disabled),
				nameof(CurrentRuntimeUniqueKey),
				nameof(LastStartTimeUtc),
				nameof(LastHeartbeatUtc),
				nameof(IdHostStatus),
				nameof(SyncToken)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(IdHost), NpgsqlDbType.Uuid },
				{ nameof(Name), NpgsqlDbType.Varchar },
				{ nameof(Description), NpgsqlDbType.Varchar },
				{ nameof(IdHostType), NpgsqlDbType.Uuid },
				{ nameof(Disabled), NpgsqlDbType.Boolean },
				{ nameof(CurrentRuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(LastStartTimeUtc), NpgsqlDbType.TimestampTz },
				{ nameof(LastHeartbeatUtc), NpgsqlDbType.TimestampTz },
				{ nameof(IdHostStatus), NpgsqlDbType.Integer },
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
				SchemaName = options.HostDbSchemaName,
				TableName = options.HostDbTableName,
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
					{ nameof(IdHost), IdHost },
					{ nameof(Name), Name },
					{ nameof(IdHostType), IdHostType },
					{ nameof(Disabled), Disabled },
					{ nameof(CurrentRuntimeUniqueKey), CurrentRuntimeUniqueKey },
					{ nameof(LastStartTimeUtc), LastStartTimeUtc },
					{ nameof(LastHeartbeatUtc), LastHeartbeatUtc },
					{ nameof(IdHostStatus), IdHostStatus },
					{ nameof(SyncToken), SyncToken }
				};

			if (!string.IsNullOrWhiteSpace(Description))
				dict.Add(nameof(Description), Description);

			return dict;
		}
	}
}
