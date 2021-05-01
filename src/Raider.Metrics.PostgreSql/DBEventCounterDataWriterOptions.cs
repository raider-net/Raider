using NpgsqlTypes;
using Raider.Data;
using Raider.Database.PostgreSql;
using System.Collections.Generic;

namespace Raider.Metrics.PostgreSql
{
	public class DBEventCounterDataWriterOptions : DbBatchWriterOptions, IBatchWriterOptions
	{
		public DBEventCounterDataWriterOptions()
		{
			TableName = nameof(DbEventCounterData);

			PropertyNames = new List<string>
			{
				nameof(DbEventCounterData.IdEventCounter),
				nameof(DbEventCounterData.RuntimeUniqueKey),
				nameof(DbEventCounterData.Created),
				nameof(DbEventCounterData.Increment),
				nameof(DbEventCounterData.Mean),
				nameof(DbEventCounterData.Count),
				nameof(DbEventCounterData.Min),
				nameof(DbEventCounterData.Max)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(DbEventCounterData.IdEventCounter), NpgsqlDbType.Integer },
				{ nameof(DbEventCounterData.RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(DbEventCounterData.Created), NpgsqlDbType.TimestampTz },
				{ nameof(DbEventCounterData.Increment), NpgsqlDbType.Double },
				{ nameof(DbEventCounterData.Mean), NpgsqlDbType.Double },
				{ nameof(DbEventCounterData.Count), NpgsqlDbType.Integer },
				{ nameof(DbEventCounterData.Min), NpgsqlDbType.Double },
				{ nameof(DbEventCounterData.Max), NpgsqlDbType.Double }
			};
		}
	}
}
