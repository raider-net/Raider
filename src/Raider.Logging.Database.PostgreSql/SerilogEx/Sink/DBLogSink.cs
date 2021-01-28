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
			.WriteTo.DBLogSink(new Raider.Logging.DB.SerilogEx.Sink.DBLogSinkOptions
			{
				ConnectionString = "Host=localhost;Database=..."
			})
			.WriteTo.Console())
	 */

	public class DBLogSink : RaiderBaseSink, ILogEventSink, IDisposable
	{
		private readonly string _connectionString;
		private readonly BulkInsert _bulkInsert;

		public DBLogSink(DBLogSinkOptions options)
			: base(options)
		{
			if (options == null)
				options = new DBLogSinkOptions();

			options.Validate();

			_connectionString = options.ConnectionString;
			_bulkInsert = new BulkInsert(options.ToBulkInsertOptions());
		}

		//public override bool Include(LogEvent logEvent)
		//	=> LogMessageHelper.IsLogMessage(logEvent);

		public override async Task WriteBatch(IEnumerable<LogEvent> batch)
		{
			//var logs = batch.Select(logEvent => LogHelper.Convert(logEvent)).Where(x => x != null);
			var logs = batch.Select(logEvent => LogEventHelper.ConvertLogToDictionary(logEvent)).Where(x => x != null).ToList();
			await _bulkInsert.WriteBatch(logs, _connectionString);
		}
	}
}
