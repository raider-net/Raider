using NpgsqlTypes;
using Raider.Database.PostgreSql;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.PostgreSql.Messages.Storage.Model
{
	internal class DbScenario : Raider.Serializer.IDictionaryObject
	{
		public Guid IdScenario { get; set; }
		public Guid IdHost { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }
		public bool Disabled { get; set; }
		public DateTime? LastStartTimeUtc { get; set; }
		public DateTime? LastHeartbeatUtc { get; set; }
		public Guid SyncToken { get; set; }

		public readonly static List<string> PropertyNames;
		public readonly static Dictionary<string, NpgsqlDbType> PropertyTypeMapping;

		static DbScenario()
		{
			PropertyNames = new List<string>
			{
				nameof(IdScenario),
				nameof(IdHost),
				nameof(Name),
				nameof(Description),
				nameof(Disabled),
				nameof(LastStartTimeUtc),
				nameof(LastHeartbeatUtc),
				nameof(SyncToken)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(IdScenario), NpgsqlDbType.Uuid },
				{ nameof(IdHost), NpgsqlDbType.Uuid },
				{ nameof(Name), NpgsqlDbType.Varchar },
				{ nameof(Description), NpgsqlDbType.Varchar },
				{ nameof(Disabled), NpgsqlDbType.Boolean },
				{ nameof(LastStartTimeUtc), NpgsqlDbType.TimestampTz },
				{ nameof(LastHeartbeatUtc), NpgsqlDbType.TimestampTz },
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
				SchemaName = options.ScenarioDbSchemaName,
				TableName = options.ScenarioDbTableName,
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
					{ nameof(IdScenario), IdScenario },
					{ nameof(IdHost), IdHost },
					{ nameof(Name), Name },
					{ nameof(Disabled), Disabled },
					{ nameof(SyncToken), SyncToken }
				};

			if (!string.IsNullOrWhiteSpace(Description))
				dict.Add(nameof(Description), Description);

			if (LastStartTimeUtc.HasValue)
				dict.Add(nameof(LastStartTimeUtc), LastStartTimeUtc);

			if (LastHeartbeatUtc.HasValue)
				dict.Add(nameof(LastHeartbeatUtc), LastHeartbeatUtc);

			return dict;
		}
	}
}
