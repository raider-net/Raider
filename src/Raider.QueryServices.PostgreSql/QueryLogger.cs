using Raider.QueryServices.Queries;
using System;

namespace Raider.QueryServices.PostgreSql
{
	public class QueryLogger : IQueryLogger
	{
		private static QueryEntryWriter? _entryWriter;
		private static QueryExitWriter? _exitWriter;

		internal static void SetEntryWriter(QueryEntryWriter writer)
		{
			if (_entryWriter != null)
				throw new InvalidOperationException($"{nameof(QueryEntryWriter)} already set.");

			_entryWriter = writer ?? throw new ArgumentNullException(nameof(writer));
		}

		internal static void SetExitWriter(QueryExitWriter writer)
		{
			if (_exitWriter != null)
				throw new InvalidOperationException($"{nameof(QueryExitWriter)} already set.");

			_exitWriter = writer ?? throw new ArgumentNullException(nameof(writer));
		}

		public void WriteQueryEntry(IQueryEntry entry)
		{
			if (_entryWriter == null)
				throw new InvalidOperationException($"{nameof(QueryEntryWriter)} was not set.");

			_entryWriter.Write(entry);
		}

		public void WriteQueryExit(IQueryEntry entry, decimal elapsedMilliseconds)
		{
			if (_exitWriter == null)
				throw new InvalidOperationException($"{nameof(QueryExitWriter)} was not set.");

			_exitWriter.Write(new QueryExit { IdCommandQueryExit = entry.IdCommandQueryEntry, ElapsedMilliseconds = elapsedMilliseconds });
		}
	}
}
