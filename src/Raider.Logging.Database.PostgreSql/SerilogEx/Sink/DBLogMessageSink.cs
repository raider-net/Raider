using Raider.Database.PostgreSql;
using Raider.Logging.SerilogEx;
using Raider.Logging.SerilogEx.Sink;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace Raider.Logging.Database.PostgreSql.SerilogEx.Sink
{
	/*
	 USAGE:
		Serilog.LoggerConfiguration
			.MinimumLevel.Verbose()
			//.Enrich.WithLogMessage()
			.WriteTo.DBLogMessageSink(new Raider.Logging.DB.SerilogEx.Sink.DBLogMessageSinkOptions
			{
				ConnectionString = "Host=localhost;Database=..."
			})
			.WriteTo.Console())
	 */

	public class DBLogMessageSink : RaiderBaseSink, ILogEventSink, IDisposable
	{
		private readonly string _connectionString;
		private readonly BulkInsert _bulkInsert;

		public DBLogMessageSink(DBLogMessageSinkOptions options)
			: base(options)
		{
			if (options == null)
				options = new DBLogMessageSinkOptions();

			options.Validate();

			_connectionString = options.ConnectionString;
			_bulkInsert = new BulkInsert(options.ToBulkInsertOptions());
		}

		//public override bool Include(LogEvent logEvent)
		//	=> LogMessageHelper.IsLogMessage(logEvent);

		public override async Task WriteBatch(IEnumerable<LogEvent> batch)
		{
			//var logMessages = batch.Select(logEvent => LogMessageHelper.Convert(logEvent)).Where(x => x != null);
			var logMessages = batch.Select(logEvent => LogEventHelper.ConvertLogMessageToDictionary(logEvent)).Where(x => x != null).ToList();
			await _bulkInsert.WriteBatch(logMessages, _connectionString);
		}
	}
}
