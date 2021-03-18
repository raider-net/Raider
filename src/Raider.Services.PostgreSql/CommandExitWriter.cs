using Raider.Database.PostgreSql;
using Raider.Logging;
using System;
using System.Collections.Generic;

namespace Raider.Services.PostgreSql
{
	public class CommandExitWriter : DbBatchWriter<CommandExit>, IDisposable
	{
		public CommandExitWriter(CommandExitOptions options, Action<string, object?, object?, object?>? errorLogger = null)
			: base(options ?? new CommandExitOptions(), errorLogger ?? DefaultErrorLoggerDelegate.Log)
		{
		}

		public override IDictionary<string, object?>? ToDictionary(CommandExit commandExit)
			=> commandExit.ToDictionary();
	}
}
