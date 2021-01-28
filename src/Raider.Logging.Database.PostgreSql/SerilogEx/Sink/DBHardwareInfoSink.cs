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
			.WriteTo.DBHardwareInfoSink(new Raider.Logging.DB.SerilogEx.Sink.DBHardwareInfoSinkOptions
			{
				ConnectionString = "Host=localhost;Database=..."
			})
			.WriteTo.Console())
	 */

	public class DBHardwareInfoSink : RaiderBaseSink, ILogEventSink, IDisposable
	{
		private readonly string _connectionString;
		private readonly BulkInsert _bulkInsert;

		public DBHardwareInfoSink(DBHardwareInfoSinkOptions options)
			: base(options)
		{
			if (options == null)
				options = new DBHardwareInfoSinkOptions();

			options.Validate();

			_connectionString = options.ConnectionString;
			_bulkInsert = new BulkInsert(options.ToBulkInsertOptions());
		}

		//public override bool Include(LogEvent logEvent)
		//	=> HardwareInfoHelper.IsHardwareInfo(logEvent);

		public override async Task WriteBatch(IEnumerable<LogEvent> batch)
		{
			//var HardwareInfos = batch.Select(logEvent => HardwareInfoHelper.Convert(logEvent)).Where(x => x != null);
			var hardwareInfos = batch.Select(logEvent => LogEventHelper.ConvertHardwareInfoToDictionary(logEvent)).Where(x => x != null).ToList();
			await _bulkInsert.WriteBatch(hardwareInfos, _connectionString);
		}
	}
}
