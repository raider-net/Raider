using Raider.Database.PostgreSql;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace Raider.AspNetCore.Logging.PostgreSql.Sink
{
	/*
	 USAGE:
		Serilog.LoggerConfiguration
			.MinimumLevel.Verbose()
			.WriteTo.DBRequestAuthenticationSink(new Raider.Logging.DB.SerilogEx.Sink.DBRequestAuthenticationSinkOptions
			{
				ConnectionString = "Host=localhost;Database=..."
			})
			.WriteTo.Console())
	 */

	public class DBRequestAuthenticationSink : DbBatchWriter<LogEvent>, ILogEventSink, IDisposable
	{
		public DBRequestAuthenticationSink(DBRequestAuthenticationSinkOptions options, Action<string, object?, object?, object?>? errorLogger = null)
			: base(options ?? new DBRequestAuthenticationSinkOptions(), errorLogger ?? SelfLog.WriteLine)
		{
		}

		public override IDictionary<string, object?>? ToDictionary(LogEvent logEvent)
			=> LogEventHelper.ConvertRequestAuthenticationToDictionary(logEvent);

		public void Emit(LogEvent logEvent)
			=> Write(logEvent);
	}
}
