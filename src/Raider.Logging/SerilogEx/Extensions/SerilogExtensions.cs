using Raider.Data;
using Raider.Logging.SerilogEx.Sink;
using Serilog.Configuration;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Serilog
{
	public static class SerilogExtensions
	{
		//public static LoggerConfiguration WithLogMessage(this LoggerEnrichmentConfiguration enrich)
		//{
		//	if (enrich == null)
		//		throw new ArgumentNullException(nameof(enrich));

		//	return enrich.With<LogMessageEnricher>();
		//}

		public static LoggerConfiguration RaiderBatchSink(
			this LoggerSinkConfiguration loggerConfiguration,
			Func<IEnumerable<LogEvent>, CancellationToken, Task> writeBatchCallback,
			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
			=> RaiderBatchSink(
				loggerConfiguration,
				writeBatchCallback,
				null,
				restrictedToMinimumLevel);

		public static LoggerConfiguration RaiderBatchSink(
			this LoggerSinkConfiguration loggerConfiguration,
			Func<IEnumerable<LogEvent>, CancellationToken, Task> writeBatchCallback,
			BatchWriterOptions? options,
			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
			=> RaiderBatchSink(
				loggerConfiguration,
				e => true,
				writeBatchCallback,
				options,
				restrictedToMinimumLevel);

		public static LoggerConfiguration RaiderBatchSink(
			this LoggerSinkConfiguration loggerConfiguration,
			Func<LogEvent, bool> includeCallBack,
			Func<IEnumerable<LogEvent>, CancellationToken, Task> writeBatchCallback,
			BatchWriterOptions? options,
			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
		{
			if (loggerConfiguration == null)
				throw new ArgumentNullException(nameof(loggerConfiguration));

			var sink = new RaiderBatchSink(
				includeCallBack,
				writeBatchCallback,
				options);

			return loggerConfiguration.Sink(sink, restrictedToMinimumLevel);
		}
	}
}
