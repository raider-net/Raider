using Microsoft.Extensions.Logging;
using Raider.Trace;
using System;

namespace Raider.Messaging
{
	public abstract class LogBase
	{
		private ITraceInfo? _traceInfo;
		public ITraceInfo? TraceInfo
		{
			get => _traceInfo ?? Trace.TraceInfo.Create();
			set => _traceInfo = value;
		}

		public int IdLogLevel { get; set; }
		public DateTime CreatedUtc { get; }
		public string LogMessageType { get; set; }
		public string Message { get; set; }
		public string? Detail { get; set; }
		public Guid? IdSubscriberMessage { get; set; }
		public Guid? IdSnapshot { get; set; }

		public LogBase(ITraceInfo? traceInfo, int idLogLevel, string logMessageType, string message)
		{
			_traceInfo = traceInfo;
			IdLogLevel = idLogLevel;
			CreatedUtc = DateTime.Now;
			LogMessageType = string.IsNullOrWhiteSpace(logMessageType)
				? throw new ArgumentNullException(nameof(logMessageType))
				: logMessageType;
			Message = string.IsNullOrWhiteSpace(message)
				? throw new ArgumentNullException(nameof(message))
				: message;
		}
	}

	public class LogTrace : LogBase
	{
		public LogTrace(ITraceInfo? traceInfo, string logMessageType, string message)
			: base(traceInfo, (int)LogLevel.Trace, logMessageType, message) { }

		public LogTrace(ITraceInfo? traceInfo, string logMessageType, string message, string? detail)
			: base(traceInfo, (int)LogLevel.Error, logMessageType, message) { Detail = detail; }
	}

	public class LogDebug : LogBase
	{
		public LogDebug(ITraceInfo? traceInfo, string logMessageType, string message)
			: base(traceInfo, (int)LogLevel.Debug, logMessageType, message) { }

		public LogDebug(ITraceInfo? traceInfo, string logMessageType, string message, string? detail)
			: base(traceInfo, (int)LogLevel.Error, logMessageType, message) { Detail = detail; }
	}

	public class LogInfo : LogBase
	{
		public LogInfo(ITraceInfo? traceInfo, string logMessageType, string message)
			: base(traceInfo, (int)LogLevel.Information, logMessageType, message) { }

		public LogInfo(ITraceInfo? traceInfo, string logMessageType, string message, string? detail)
			: base(traceInfo, (int)LogLevel.Error, logMessageType, message) { Detail = detail; }
	}

	public class LogWarning : LogBase
	{
		public LogWarning(ITraceInfo? traceInfo, string logMessageType, string message)
			: base(traceInfo, (int)LogLevel.Warning, logMessageType, message) { }

		public LogWarning(ITraceInfo? traceInfo, string logMessageType, string message, string? detail)
			: base(traceInfo, (int)LogLevel.Error, logMessageType, message) { Detail = detail; }
	}

	public class LogError : LogBase
	{
		public LogError(ITraceInfo? traceInfo, string logMessageType, string message)
			: base(traceInfo, (int)LogLevel.Error, logMessageType, message) { }

		public LogError(ITraceInfo? traceInfo, string logMessageType, string message, string? detail)
			: base(traceInfo, (int)LogLevel.Error, logMessageType, message) { Detail = detail; }
	}

	public class LogCritical : LogBase
	{
		public LogCritical(ITraceInfo? traceInfo, string logMessageType, string message)
			: base(traceInfo, (int)LogLevel.Critical, logMessageType, message) { }

		public LogCritical(ITraceInfo? traceInfo, string logMessageType, string message, string? detail)
			: base(traceInfo, (int)LogLevel.Error, logMessageType, message) { Detail = detail; }
	}
}
