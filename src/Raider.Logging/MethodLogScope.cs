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

		private bool disposed;
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					_logScope?.Dispose();
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
