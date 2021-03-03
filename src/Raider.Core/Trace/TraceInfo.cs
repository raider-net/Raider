using Raider.Infrastructure;
using System;

namespace Raider.Trace
{
	public class TraceInfo : ITraceInfo
	{
		public Guid RuntimeUniqueKey { get; internal set; }

		public ITraceFrame TraceFrame { get; }

		public int? IdUser { get; internal set; }

		public string? ExternalCorrelationId { get; internal set; }

		public Guid? CorrelationId { get; internal set; }

		internal TraceInfo(ITraceFrame traceFrame)
		{
			RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY;
			TraceFrame = traceFrame ?? throw new ArgumentNullException(nameof(traceFrame));
		}
	}
}
