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
				nameof(CommandExit.IdCommandQueryEntry),
				nameof(CommandExit.ElapsedMilliseconds)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(CommandExit.IdCommandQueryEntry), NpgsqlDbType.Uuid },
				{ nameof(CommandExit.ElapsedMilliseconds), NpgsqlDbType.Numeric },
			};
		}
	}
}
