using Raider.Database.PostgreSql;
using Raider.Logging;
using Raider.Services.Commands;
using System;
using System.Collections.Generic;

namespace Raider.Services.PostgreSql
{
	public class CommandEntryWriter : DbBatchWriter<ICommandEntry>, IDisposable
	{
		public CommandEntryWriter(CommandEntryOptions options, Action<string, object?, object?, object?>? errorLogger = null)
			: base(options ?? new CommandEntryOptions(), errorLogger ?? DefaultErrorLoggerDelegate.Log)
		{
		}

		public override IDictionary<string, object?>? ToDictionary(ICommandEntry commandEntry)
			=> commandEntry.ToDictionary();
	}
}
