using Raider.Database.PostgreSql;
using Raider.Logging;
using Raider.QueryServices.Queries;
using System;
using System.Collections.Generic;

namespace Raider.QueryServices.PostgreSql
{
	public class QueryEntryWriter : DbBatchWriter<IQueryEntry>, IDisposable
	{
		public QueryEntryWriter(QueryEntryOptions options, Action<string, object?, object?, object?>? errorLogger = null)
			: base(options ?? new QueryEntryOptions(), errorLogger ?? DefaultErrorLoggerDelegate.Log)
		{
		}

		public override IDictionary<string, object?>? ToDictionary(IQueryEntry commandEntry)
			=> commandEntry.ToDictionary();
	}
}
