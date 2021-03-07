using Serilog.Events;
using System.Collections.Generic;

namespace Raider.AspNetCore.Logging
{
	public static class LogEventHelper
	{
		public static bool IsRequest(LogEvent logEvent)
			=> Raider.Logging.SerilogEx.LogEventHelper.IsLogType(LoggerSettings.Request, logEvent);

		public static IDictionary<string, object?>? ConvertRequestToDictionary(LogEvent logEvent)
			=> Raider.Logging.SerilogEx.LogEventHelper.ConvertToDictionary(LoggerSettings.Request, logEvent);

		public static bool IsResponse(LogEvent logEvent)
			=> Raider.Logging.SerilogEx.LogEventHelper.IsLogType(LoggerSettings.Response, logEvent);

		public static IDictionary<string, object?>? ConvertResponseToDictionary(LogEvent logEvent)
			=> Raider.Logging.SerilogEx.LogEventHelper.ConvertToDictionary(LoggerSettings.Response, logEvent);
	}
}
