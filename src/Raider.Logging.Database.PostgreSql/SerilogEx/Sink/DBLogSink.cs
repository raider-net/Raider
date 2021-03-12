using Raider.Database.PostgreSql;
using Raider.Logging.SerilogEx;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace Raider.Logging.Database.PostgreSql.SerilogEx.Sink
{
	/*
	 USAGE:
		Serilog.LoggerConfiguration
			.MinimumLevel.Verbose()
			.WriteTo.DBLogSink(new Raider.Logging.DB.SerilogEx.Sink.DBLogSinkOptions
			{
				ConnectionString = "Host=localhost;Database=..."
			})
			.WriteTo.Console())
	 */

	public class DBLogSink : DbBatchWriter<LogEvent>, ILogEventSink, IDisposable
	{
		public DBLogSink(DBLogSinkOptions options, Action<string, object?, object?, object?>? errorLogger = null)
			: base(options ?? new DBLogSinkOptions(), errorLogger ?? SelfLog.WriteLine)
		{
		}

		public override IDictionary<string, object?>? ToDictionary(LogEvent logEvent)
			=> LogEventHelper.ConvertLogToDictionary(logEvent);

		public void Emit(LogEvent logEvent)
			=> Write(logEvent);
	}
}
