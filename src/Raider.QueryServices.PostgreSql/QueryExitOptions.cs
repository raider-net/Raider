using NpgsqlTypes;
using Raider.Data;
using Raider.Database.PostgreSql;
using System.Collections.Generic;

namespace Raider.QueryServices.PostgreSql
{
	public class QueryExitOptions : DbBatchWriterOptions, IBatchWriterOptions
	{
		public QueryExitOptions()
		{
			TableName = "CommandQueryExit";

			PropertyNames = new List<string>
			{
				nameof(QueryExit.IdCommandQueryExit),
				nameof(QueryExit.ElapsedMilliseconds),
				nameof(QueryExit.IsError),
				nameof(QueryExit.Data)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(QueryExit.IdCommandQueryExit), NpgsqlDbType.Uuid },
				{ nameof(QueryExit.ElapsedMilliseconds), NpgsqlDbType.Numeric },
				{ nameof(QueryExit.IsError), NpgsqlDbType.Boolean },
				{ nameof(QueryExit.Data), NpgsqlDbType.Varchar }
			};
		}
	}
}
