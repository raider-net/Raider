using Raider.Data;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Logging.SerilogEx.Sink
{
	/*
	 USAGE:
		Serilog.LoggerConfiguration
			.MinimumLevel.Verbose()
			//.Enrich.WithLogMessage()
			.WriteTo.RaiderBatchSink(events => Task.CompletedTask \/* TODO write to output  *\/, new RaiderBatchSinkOptions { EagerlyEmitFirstEvent = true })
			.WriteTo.Console())
	 */

	public class RaiderBatchSink : BatchWriter<LogEvent>, ILogEventSink, IDisposable
	{
		public RaiderBatchSink(
			Func<LogEvent, bool> includeCallBack,
			Func<IEnumerable<LogEvent>, CancellationToken, Task> writeBatchCallback,
			BatchWriterOptions? options,
			Action<string, object?, object?, object?>? errorLogger = null)
			: base(includeCallBack, writeBatchCallback, options, errorLogger ?? SelfLog.WriteLine)
		{
		}

		public void Emit(LogEvent logEvent)
			=> Write(logEvent);
	}
}
