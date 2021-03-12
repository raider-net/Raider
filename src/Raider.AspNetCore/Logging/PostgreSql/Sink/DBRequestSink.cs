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
			.WriteTo.DBRequestSink(new Raider.Logging.DB.SerilogEx.Sink.DBRequestSinkOptions
			{
				ConnectionString = "Host=localhost;Database=..."
			})
			.WriteTo.Console())
	 */

	public class DBRequestSink : DbBatchWriter<LogEvent>, ILogEventSink, IDisposable
	{
		public DBRequestSink(DBRequestSinkOptions options, Action<string, object?, object?, object?>? errorLogger = null)
			: base(options ?? new DBRequestSinkOptions(), errorLogger ?? SelfLog.WriteLine)
		{
		}

		public override IDictionary<string, object?>? ToDictionary(LogEvent logEvent)
			=> LogEventHelper.ConvertRequestToDictionary(logEvent);

		public void Emit(LogEvent logEvent)
			=> Write(logEvent);
	}
}
