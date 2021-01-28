using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
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

	public class RaiderBatchSink : RaiderBaseSink, ILogEventSink, IDisposable
	{
		private readonly Func<LogEvent, bool> _includeCallBack;
		private readonly Func<IEnumerable<LogEvent>, Task> _writeBatchCallback;

		public RaiderBatchSink(Func<LogEvent, bool> includeCallBack, Func<IEnumerable<LogEvent>, Task> writeBatchCallback, RaiderBatchSinkOptions? options)
			: base(options)
		{
			_writeBatchCallback = writeBatchCallback ?? throw new ArgumentNullException(nameof(writeBatchCallback));
			_includeCallBack = includeCallBack ?? (e => true);
		}

		public override bool Include(LogEvent logEvent)
			=> _includeCallBack(logEvent);

		public override Task WriteBatch(IEnumerable<LogEvent> batch)
			=> _writeBatchCallback(batch);
	}
}
