using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Raider.Trace
{
	public class TraceContext
	{
		private readonly List<KeyValuePair<string, ITraceInfo>> _trace = new List<KeyValuePair<string, ITraceInfo>>();
		
		public ITraceInfo? Current { get; private set; }

		private readonly object _lockTraceInfo = new object();
		public TraceContext Initialize(string key, ITraceInfo traceInfo)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if (Current != null)
				throw new NotSupportedException($"{nameof(TraceContext)} already initialized");

			lock (_lockTraceInfo)
			{
				if (Current != null)
					throw new NotSupportedException($"{nameof(TraceContext)} already initialized");

				Current = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
				_trace.Add(new KeyValuePair<string, ITraceInfo>(key, Current));
			}

			return this;
		}

		private readonly object _lock = new object();
		public TraceContext AddTraceFrame(string key, ITraceFrame traceFrame)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if (traceFrame == null)
				throw new ArgumentNullException(nameof(traceFrame));

			if (Current == null)
				throw new NotSupportedException($"{nameof(TraceContext)} was not initialized");

			lock (_lock)
			{
				Current = new TraceInfoBuilder(traceFrame, Current)
					.Build();

				_trace.Add(new KeyValuePair<string, ITraceInfo>(key, Current));
			}

			return this;
		}

		public bool TryGetTraceInfo(string key, [MaybeNullWhen(false)] out ITraceInfo? traceInfo)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			traceInfo = _trace.FirstOrDefault(x => x.Key == key).Value;
			return traceInfo != null;
		}
	}
}
