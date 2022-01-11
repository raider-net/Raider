using Raider.Database.PostgreSql;
using Raider.Logging;
using Raider.Web.Logging;
using System;
using System.Collections.Generic;

namespace Raider.AspNetCore.Logging.PostgreSql.Sink
{
	public class RequestWriter : DbBatchWriter<RequestDto>, IDisposable
	{
		public RequestWriter(DBRequestSinkOptions options, Action<string, object?, object?, object?>? errorLogger = null)
			: base(options ?? new DBRequestSinkOptions(), errorLogger ?? DefaultErrorLoggerDelegate.Log)
		{
		}

		public override IDictionary<string, object?>? ToDictionary(RequestDto request)
			=> request.ToDictionary();
	}
}
