using Raider.AspNetCore.Logging.Dto;
using Raider.Database.PostgreSql;
using Raider.Logging;
using System;
using System.Collections.Generic;

namespace Raider.AspNetCore.Logging.PostgreSql.Sink
{
	public class RequestAuthenticationWriter : DbBatchWriter<RequestAuthentication>, IDisposable
	{
		public RequestAuthenticationWriter(DBRequestAuthenticationSinkOptions options, Action<string, object?, object?, object?>? errorLogger = null)
			: base(options ?? new DBRequestAuthenticationSinkOptions(), errorLogger ?? DefaultErrorLoggerDelegate.Log)
		{
		}

		public override IDictionary<string, object?>? ToDictionary(RequestAuthentication requestAuthentication)
			=> requestAuthentication.ToDictionary();
	}
}
