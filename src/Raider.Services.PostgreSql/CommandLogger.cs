using Raider.Services.Commands;
using System;

namespace Raider.Services.PostgreSql
{
	public class CommandLogger : ICommandLogger
	{
		private static CommandEntryWriter? _entryWriter;
		private static CommandExitWriter? _exitWriter;

		internal static void SetEntryWriter(CommandEntryWriter writer)
		{
			if (_entryWriter == null)
				throw new InvalidOperationException($"{nameof(CommandEntryWriter)} already set.");

			_entryWriter = writer ?? throw new ArgumentNullException(nameof(writer));
		}

		internal static void SetExitWriter(CommandExitWriter writer)
		{
			if (_exitWriter == null)
				throw new InvalidOperationException($"{nameof(CommandExitWriter)} already set.");

			_exitWriter = writer ?? throw new ArgumentNullException(nameof(writer));
		}

		public void WriteCommandEntry(ICommandEntry entry)
		{
			if (_entryWriter == null)
				throw new InvalidOperationException($"{nameof(CommandEntryWriter)} was not set.");

			_entryWriter.Write(entry);
		}

		public void WriteCommandExit(ICommandEntry entry, decimal elapsedMilliseconds)
		{
			if (_exitWriter == null)
				throw new InvalidOperationException($"{nameof(CommandExitWriter)} was not set.");

			_exitWriter.Write(new CommandExit { IdCommandQueryEntry = entry.IdCommandQueryEntry, ElapsedMilliseconds = elapsedMilliseconds });
		}
	}
}
