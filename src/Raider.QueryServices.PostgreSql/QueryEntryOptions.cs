using NpgsqlTypes;
using Raider.Data;
using Raider.Database.PostgreSql;
using Raider.QueryServices.Queries;
using System.Collections.Generic;

namespace Raider.QueryServices.PostgreSql
{
	public class QueryEntryOptions : DbBatchWriterOptions, IBatchWriterOptions
	{
		public QueryEntryOptions()
		{
			TableName = "CommandQueryEntry";

			PropertyNames = new List<string>
			{
				nameof(IQueryEntry.IdCommandQueryEntry),
				nameof(IQueryEntry.Created),
				nameof(IQueryEntry.CommandQueryName),
				nameof(IQueryEntry.TraceFrame),
				nameof(IQueryEntry.IsQuery),
				nameof(IQueryEntry.Data),
				nameof(IQueryEntry.IdUser),
				nameof(IQueryEntry.CorrelationId)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(IQueryEntry.IdCommandQueryEntry), NpgsqlDbType.Uuid },
				{ nameof(IQueryEntry.Created), NpgsqlDbType.TimestampTz },
				{ nameof(IQueryEntry.CommandQueryName), NpgsqlDbType.Varchar },
				{ nameof(IQueryEntry.TraceFrame), NpgsqlDbType.Varchar },
				{ nameof(IQueryEntry.IsQuery), NpgsqlDbType.Boolean },
				{ nameof(IQueryEntry.Data), NpgsqlDbType.Varchar },
				{ nameof(IQueryEntry.IdUser), NpgsqlDbType.Integer },
				{ nameof(IQueryEntry.CorrelationId), NpgsqlDbType.Uuid }
			};
		}
	}
}
