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
			.WriteTo.DBHardwareInfoSink(new Raider.Logging.DB.SerilogEx.Sink.DBHardwareInfoSinkOptions
			{
				ConnectionString = "Host=localhost;Database=..."
			})
			.WriteTo.Console())
	 */

	public class DBHardwareInfoSink : DbBatchWriter<LogEvent>, ILogEventSink, IDisposable
	{
		public DBHardwareInfoSink(DBHardwareInfoSinkOptions options, Action<string, object?, object?, object?>? errorLogger = null)
			: base(options ?? new DBHardwareInfoSinkOptions(), errorLogger ?? SelfLog.WriteLine)
		{
		}

		public override IDictionary<string, object?>? ToDictionary(LogEvent logEvent)
			=> LogEventHelper.ConvertHardwareInfoToDictionary(logEvent);

		public void Emit(LogEvent logEvent)
			=> Write(logEvent);
	}
}
