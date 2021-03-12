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
			.WriteTo.DBEnvironmentInfoSink(new Raider.Logging.DB.SerilogEx.Sink.DBEnvironmentInfoSinkOptions
			{
				ConnectionString = "Host=localhost;Database=..."
			})
			.WriteTo.Console())
	 */

	public class DBEnvironmentInfoSink : DbBatchWriter<LogEvent>, ILogEventSink, IDisposable
	{
		public DBEnvironmentInfoSink(DBEnvironmentInfoSinkOptions options, Action<string, object?, object?, object?>? errorLogger = null)
			: base(options ?? new DBEnvironmentInfoSinkOptions(), errorLogger ?? SelfLog.WriteLine)
		{
		}

		public override IDictionary<string, object?>? ToDictionary(LogEvent logEvent)
			=> LogEventHelper.ConvertEnvironmentInfoToDictionary(logEvent);

		public void Emit(LogEvent logEvent)
			=> Write(logEvent);
	}
}
