using NpgsqlTypes;
using Raider.Data;
using Raider.Database.PostgreSql;
using System.Collections.Generic;

namespace Raider.Services.PostgreSql
{
	public class CommandExitOptions : DbBatchWriterOptions, IBatchWriterOptions
	{
		public CommandExitOptions()
		{
			TableName = "CommandQueryExit";

			PropertyNames = new List<string>
			{
				nameof(CommandExit.IdCommandQueryExit),
				nameof(CommandExit.ElapsedMilliseconds),
				nameof(CommandExit.IsError),
				nameof(CommandExit.Data)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(CommandExit.IdCommandQueryExit), NpgsqlDbType.Uuid },
				{ nameof(CommandExit.ElapsedMilliseconds), NpgsqlDbType.Numeric },
				{ nameof(CommandExit.IsError), NpgsqlDbType.Boolean },
				{ nameof(CommandExit.Data), NpgsqlDbType.Varchar }
			};
		}
	}
}
