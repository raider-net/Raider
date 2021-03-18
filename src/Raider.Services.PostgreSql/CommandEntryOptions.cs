using NpgsqlTypes;
using Raider.Data;
using Raider.Database.PostgreSql;
using Raider.Services.Commands;
using System.Collections.Generic;

namespace Raider.Services.PostgreSql
{
	public class CommandEntryOptions : DbBatchWriterOptions, IBatchWriterOptions
	{
		public CommandEntryOptions()
		{
			TableName = "CommandQueryEntry";

			PropertyNames = new List<string>
			{
				nameof(ICommandEntry.IdCommandQueryEntry),
				nameof(ICommandEntry.Created),
				nameof(ICommandEntry.CommandQueryName),
				nameof(ICommandEntry.TraceFrame),
				nameof(ICommandEntry.IsQuery),
				nameof(ICommandEntry.Data),
				nameof(ICommandEntry.IdUser),
				nameof(ICommandEntry.CorrelationId)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(ICommandEntry.IdCommandQueryEntry), NpgsqlDbType.Uuid },
				{ nameof(ICommandEntry.Created), NpgsqlDbType.TimestampTz },
				{ nameof(ICommandEntry.CommandQueryName), NpgsqlDbType.Varchar },
				{ nameof(ICommandEntry.TraceFrame), NpgsqlDbType.Varchar },
				{ nameof(ICommandEntry.IsQuery), NpgsqlDbType.Boolean },
				{ nameof(ICommandEntry.Data), NpgsqlDbType.Varchar },
				{ nameof(ICommandEntry.IdUser), NpgsqlDbType.Integer },
				{ nameof(ICommandEntry.CorrelationId), NpgsqlDbType.Uuid }
			};
		}
	}
}
