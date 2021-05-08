using Microsoft.Extensions.Logging;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

		public static MethodLogScope Create(
			ILogger logger,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> Create(logger, (ITraceInfo?)null, methodParameters, memberName, sourceFilePath, sourceLineNumber);

		public static MethodLogScope Create(
			ILogger logger,
			MethodLogScope? methodLogScope,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> Create(logger, methodLogScope?.TraceInfo, methodParameters, memberName, sourceFilePath, sourceLineNumber);

		public static MethodLogScope Create(
			ILogger logger,
			ITraceInfo? previousTraceInfo,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (logger == null)
				throw new ArgumentNullException(nameof(logger));

			var traceInfo =
				new TraceInfoBuilder(
					new TraceFrameBuilder()
						.CallerMemberName(memberName)
						.CallerFilePath(sourceFilePath)
						.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
						.MethodParameters(methodParameters)
						.Build(),
					previousTraceInfo)
					.Build();

			var disposable = logger.BeginScope(new Dictionary<string, Guid?>
			{
				[nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId)] = traceInfo.TraceFrame.MethodCallId,
				[nameof(ILogMessage.TraceInfo.CorrelationId)] = traceInfo.CorrelationId
			});

			var scope = new MethodLogScope(traceInfo, disposable);
			return scope;
		}
	}
}
