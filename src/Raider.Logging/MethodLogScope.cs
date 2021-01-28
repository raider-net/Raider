using Raider.Trace;
using System;

namespace Raider.Logging
{
	public class MethodLogScope : IDisposable
	{
		private readonly IDisposable _logScope;

		public ITraceInfo TraceInfo { get; }

		public MethodLogScope(ITraceInfo traceInfo, IDisposable logScope)
		{
			TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
			_logScope = logScope;
		}

		public void Dispose()
		{
			_logScope?.Dispose();
		}
	}
}
