﻿using Raider.Database.PostgreSql;
using Raider.Logging.SerilogEx.Sink;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace Raider.AspNetCore.Logging.PostgreSql.Sink
{
	/*
	 USAGE:
		Serilog.LoggerConfiguration
			.MinimumLevel.Verbose()
			.WriteTo.DBResponseSink(new Raider.Logging.DB.SerilogEx.Sink.DBResponseSinkOptions
			{
				ConnectionString = "Host=localhost;Database=..."
			})
			.WriteTo.Console())
	 */

	public class DBResponseSink : RaiderBaseSink, ILogEventSink, IDisposable
	{
		private readonly string _connectionString;
		private readonly BulkInsert _bulkInsert;

		public DBResponseSink(DBResponseSinkOptions options)
			: base(options)
		{
			if (options == null)
				options = new DBResponseSinkOptions();

			options.Validate();

			_connectionString = options.ConnectionString;
			_bulkInsert = new BulkInsert(options.ToBulkInsertOptions());
		}

		//public override bool Include(LogEvent logEvent)
		//	=> EnvironmentInfoHelper.IsEnvironmentInfo(logEvent);

		public override async Task WriteBatch(IEnumerable<LogEvent> batch)
		{
			//var environmentInfos = batch.Select(logEvent => EnvironmentInfoHelper.Convert(logEvent)).Where(x => x != null);
			var environmentInfos = batch.Select(logEvent => LogEventHelper.ConvertResponseToDictionary(logEvent)).Where(x => x != null).ToList();
			await _bulkInsert.WriteBatch(environmentInfos, _connectionString);
		}
	}
}