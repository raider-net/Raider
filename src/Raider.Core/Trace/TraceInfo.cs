using Raider.Infrastructure;
using System;

namespace Raider.Trace
{
	public class TraceInfo : ITraceInfo
	{
		public Guid RuntimeUniqueKey { get; set; }

		public ITraceFrame TraceFrame { get; set; }

		public int? IdUser { get; set; }

		public string? ExternalCorrelationId { get; set; }

		public Guid? CorrelationId { get; set; }

		internal TraceInfo(ITraceFrame traceFrame)
		{
			RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY;
			TraceFrame = traceFrame ?? throw new ArgumentNullException(nameof(traceFrame));
		}
	}
}
