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
				nameof(QueryExit.IdCommandQueryEntry),
				nameof(QueryExit.ElapsedMilliseconds)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(QueryExit.IdCommandQueryEntry), NpgsqlDbType.Uuid },
				{ nameof(QueryExit.ElapsedMilliseconds), NpgsqlDbType.Numeric },
			};
		}
	}
}
