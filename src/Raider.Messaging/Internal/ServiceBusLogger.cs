using Microsoft.Extensions.Logging;
using Raider.Logging.Extensions;
using Raider.Messaging.Logging;
using Raider.Trace;
using System;

namespace Raider.Messaging
{
	internal static class ServiceBusLogger
	{
		public static void SeriLogError(ILogger? logger, ITraceInfo traceInfo, string message, Exception? ex)
		{
			if (traceInfo == null)
				traceInfo = TraceInfo.Create();

			if (logger == null)
			{
				if (ex == null)
					Serilog.Log.Logger.Error($"{traceInfo}: {message}");
				else
					Serilog.Log.Logger.Error(ex, $"{traceInfo}: {message}");
			}
			else
			{
				if (ex == null)
					logger.LogErrorMessage(traceInfo, x => x.LogCode(ServiceBusLogCode.Ex_ESB.ToString()).InternalMessage(message));
				else
					logger.LogErrorMessage(traceInfo, x => x.LogCode(ServiceBusLogCode.Ex_ESB.ToString()).ExceptionInfo(ex).Detail(message));
			}
		}
	}
}
