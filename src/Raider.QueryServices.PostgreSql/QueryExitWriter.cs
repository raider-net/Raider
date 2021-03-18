using Raider.Database.PostgreSql;
using Raider.Logging;
using System;
using System.Collections.Generic;

namespace Raider.QueryServices.PostgreSql
{
	public class QueryExitWriter : DbBatchWriter<QueryExit>, IDisposable
	{
		public QueryExitWriter(QueryExitOptions options, Action<string, object?, object?, object?>? errorLogger = null)
			: base(options ?? new QueryExitOptions(), errorLogger ?? DefaultErrorLoggerDelegate.Log)
		{
		}

		public override IDictionary<string, object?>? ToDictionary(QueryExit commandExit)
			=> commandExit.ToDictionary();
	}
}
