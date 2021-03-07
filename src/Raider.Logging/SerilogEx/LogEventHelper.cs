using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Logging.SerilogEx
{
	public static class LogEventHelper
	{
		public static bool IsLogType(string logType, LogEvent logEvent)
		=> logEvent != null
			&& logEvent.Properties.TryGetValue(logType, out LogEventPropertyValue? value)
			&& value is DictionaryValue;

		public static IDictionary<string, object?>? ConvertToDictionary(string logType, LogEvent logEvent)
		{
			if (logEvent == null || logEvent.Properties == null)
				return null;

			IDictionary<string, object?>? result = null;

			if (logEvent.Properties.TryGetValue(logType, out LogEventPropertyValue? value))
			{
				if (value is DictionaryValue dict && dict.Elements != null)
					result = dict.Elements.ToDictionary(x => (string)x.Key.Value, x => (x.Value as ScalarValue)?.Value);
			}

			if (result == null)
				return null;

			if (logEvent.Properties.TryGetValue(Serilog.Core.Constants.SourceContextPropertyName, out LogEventPropertyValue? sourceContextValue))
			{
				if (sourceContextValue is ScalarValue scalarValue)
				{
					if (scalarValue.Value is string sourceContext)
					{
						result.Add(Serilog.Core.Constants.SourceContextPropertyName, sourceContext);
					}
				}
			}

			return result;
		}

		public static bool IsLogMessage(LogEvent logEvent)
			=> IsLogType(LoggerSettings.LogMessage, logEvent);

		public static IDictionary<string, object?>? ConvertLogMessageToDictionary(LogEvent logEvent)
			=> ConvertToDictionary(LoggerSettings.LogMessage, logEvent);

		public static bool IsEnvironmentInfo(LogEvent logEvent)
			=> IsLogType(LoggerSettings.EnvironmentInfo, logEvent);

		public static IDictionary<string, object?>? ConvertEnvironmentInfoToDictionary(LogEvent logEvent)
			=> ConvertToDictionary(LoggerSettings.EnvironmentInfo, logEvent);

		public static bool IsHardwareInfo(LogEvent logEvent)
			=> IsLogType(LoggerSettings.HardwareInfo, logEvent);

		public static IDictionary<string, object?>? ConvertHardwareInfoToDictionary(LogEvent logEvent)
			=> ConvertToDictionary(LoggerSettings.HardwareInfo, logEvent);

		public static IDictionary<string, object?>? ConvertLogToDictionary(LogEvent logEvent)
		{
			if (logEvent == null)
				return null;

			var result = new Dictionary<string, object?>
			{
				{ nameof(logEvent.Level), logEvent.Level },
				{ nameof(logEvent.Timestamp), logEvent.Timestamp },
				{ nameof(logEvent.MessageTemplate), logEvent.RenderMessage(null) },
				{ nameof(logEvent.Properties), logEvent.Properties },
				{ nameof(logEvent.Exception), logEvent.Exception }
			};

			if (logEvent.Properties.TryGetValue("Scope", out LogEventPropertyValue? scopeValue))
			{
				if (scopeValue is SequenceValue sequenceValue)
				{
					var last = sequenceValue.Elements.LastOrDefault();
					if (last is DictionaryValue dict && dict.Elements != null)
						if (dict.Elements.TryGetValue(new ScalarValue(nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId)), out LogEventPropertyValue? scopeMmethodCallIdValue))
							if (scopeMmethodCallIdValue is ScalarValue scalarValue)
							{
								if (scalarValue.Value is Guid methodCallId)
								{
									result[nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId)] = methodCallId;
								}
							}
				}
			}

			if (logEvent.Properties.TryGetValue(nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId), out LogEventPropertyValue? methodCallIdValue))
			{
				if (methodCallIdValue is ScalarValue scalarValue)
				{
					if (scalarValue.Value is Guid methodCallId)
					{
						result[nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId)] = methodCallId;
					}
				}
			}

			if (logEvent.Properties.TryGetValue(Serilog.Core.Constants.SourceContextPropertyName, out LogEventPropertyValue? sourceContextValue))
			{
				if (sourceContextValue is ScalarValue scalarValue)
				{
					if (scalarValue.Value is string sourceContext)
					{
						result.Add(Serilog.Core.Constants.SourceContextPropertyName, sourceContext);
					}
				}
			}

			return result;
		}
	}
}
