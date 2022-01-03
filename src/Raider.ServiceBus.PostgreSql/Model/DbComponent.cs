using NpgsqlTypes;
using Raider.Database.PostgreSql;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.PostgreSql.Messages.Storage.Model
{
	internal class DbComponent : Raider.Serializer.IDictionaryObject
	{
		public Guid IdComponent { get; set; }
		public Guid IdScenario { get; set; }
		public string Name { get; set; }
		public string CrlType { get; set; }
		public string? Description { get; set; }
		public int ThrottleDelayInMilliseconds { get; set; }
		public int InactivityTimeoutInSeconds { get; set; }
		public int ShutdownTimeoutInSeconds { get; set; }
		public Guid? IdCurrentSession { get; set; }
		public int IdComponentStatus { get; set; }
		public DateTime? LastStartTimeUtc { get; set; }
		public DateTime? LastHeartbeatUtc { get; set; }
		public Guid SyncToken { get; set; }

		public readonly static List<string> PropertyNames;
		public readonly static Dictionary<string, NpgsqlDbType> PropertyTypeMapping;

		static DbComponent()
		{
			PropertyNames = new List<string>
			{
				nameof(IdComponent),
				nameof(IdScenario),
				nameof(Name),
				nameof(CrlType),
				nameof(Description),
				nameof(ThrottleDelayInMilliseconds),
				nameof(InactivityTimeoutInSeconds),
				nameof(ShutdownTimeoutInSeconds),
				nameof(IdCurrentSession),
				nameof(IdComponentStatus),
				nameof(LastStartTimeUtc),
				nameof(LastHeartbeatUtc),
				nameof(SyncToken)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(IdComponent), NpgsqlDbType.Uuid },
				{ nameof(IdScenario), NpgsqlDbType.Uuid },
				{ nameof(Name), NpgsqlDbType.Varchar },
				{ nameof(CrlType), NpgsqlDbType.Varchar },
				{ nameof(Description), NpgsqlDbType.Varchar },
				{ nameof(ThrottleDelayInMilliseconds), NpgsqlDbType.Integer },
				{ nameof(InactivityTimeoutInSeconds), NpgsqlDbType.Integer },
				{ nameof(ShutdownTimeoutInSeconds), NpgsqlDbType.Integer },
				{ nameof(IdCurrentSession), NpgsqlDbType.Uuid },
				{ nameof(IdComponentStatus), NpgsqlDbType.Integer },
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
				SchemaName = options.ComponentDbSchemaName,
				TableName = options.ComponentDbTableName,
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
					{ nameof(IdComponent), IdComponent },
					{ nameof(IdScenario), IdScenario },
					{ nameof(Name), Name },
					{ nameof(CrlType), CrlType },
					{ nameof(ThrottleDelayInMilliseconds), ThrottleDelayInMilliseconds },
					{ nameof(InactivityTimeoutInSeconds), InactivityTimeoutInSeconds },
					{ nameof(ShutdownTimeoutInSeconds), ShutdownTimeoutInSeconds },
					{ nameof(IdComponentStatus), IdComponentStatus },
					{ nameof(SyncToken), SyncToken }
				};

			if (!string.IsNullOrWhiteSpace(Description))
				dict.Add(nameof(Description), Description);

			if (IdCurrentSession.HasValue)
				dict.Add(nameof(IdCurrentSession), IdCurrentSession);

			if (LastStartTimeUtc.HasValue)
				dict.Add(nameof(LastStartTimeUtc), LastStartTimeUtc);

			if (LastHeartbeatUtc.HasValue)
				dict.Add(nameof(LastHeartbeatUtc), LastHeartbeatUtc);

			return dict;
		}
	}
}
