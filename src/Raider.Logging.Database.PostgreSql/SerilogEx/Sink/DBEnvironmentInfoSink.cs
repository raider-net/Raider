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
			.WriteTo.DBEnvironmentInfoSink(new Raider.Logging.DB.SerilogEx.Sink.DBEnvironmentInfoSinkOptions
			{
				ConnectionString = "Host=localhost;Database=..."
			})
			.WriteTo.Console())
	 */

	public class DBEnvironmentInfoSink : RaiderBaseSink, ILogEventSink, IDisposable
	{
		private readonly string _connectionString;
		private readonly BulkInsert _bulkInsert;

		public DBEnvironmentInfoSink(DBEnvironmentInfoSinkOptions options)
			: base(options)
		{
			if (options == null)
				options = new DBEnvironmentInfoSinkOptions();

			options.Validate();

			_connectionString = options.ConnectionString;
			_bulkInsert = new BulkInsert(options.ToBulkInsertOptions());
		}

		//public override bool Include(LogEvent logEvent)
		//	=> EnvironmentInfoHelper.IsEnvironmentInfo(logEvent);

		public override async Task WriteBatch(IEnumerable<LogEvent> batch)
		{
			//var environmentInfos = batch.Select(logEvent => EnvironmentInfoHelper.Convert(logEvent)).Where(x => x != null);
			var environmentInfos = batch.Select(logEvent => LogEventHelper.ConvertEnvironmentInfoToDictionary(logEvent)).Where(x => x != null).ToList();

			if (environmentInfos != null)
				await _bulkInsert.WriteBatch(environmentInfos, _connectionString);
		}
	}
}
