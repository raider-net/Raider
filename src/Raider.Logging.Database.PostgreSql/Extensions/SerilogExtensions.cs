using Raider.Logging.Database.PostgreSql;
using Raider.Logging.Database.PostgreSql.SerilogEx.Sink;
using Raider.Logging.SerilogEx;
using Serilog.Configuration;
using Serilog.Events;
using System;

namespace Serilog
{
	public static class SerilogExtensions
	{
		[System.Diagnostics.DebuggerStepThrough]
		public static LoggerConfiguration LogMessageSinkToPostgreSql(
			this LoggerSinkConfiguration loggerConfiguration,
			DBLogMessageSinkOptions options,
			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
		{
			if (loggerConfiguration == null)
				throw new ArgumentNullException(nameof(loggerConfiguration));

			var sink = new DBLogMessageSink(options);
			return loggerConfiguration
					.Conditional(
						logEvent => LogEventHelper.IsLogMessage(logEvent),
						cfg => cfg.Sink(sink, restrictedToMinimumLevel));
		}

		[System.Diagnostics.DebuggerStepThrough]
		public static LoggerConfiguration LogSinkToPostgreSql(
			this LoggerSinkConfiguration loggerConfiguration,
			DBLogSinkOptions options,
			bool logAllLogEvents = false,
			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
		{
			if (loggerConfiguration == null)
				throw new ArgumentNullException(nameof(loggerConfiguration));

			var sink = new DBLogSink(options);
			return loggerConfiguration
					.Conditional(
						logEvent => logAllLogEvents 
							|| (!LogEventHelper.IsLogMessage(logEvent)
								&& !LogEventHelper.IsEnvironmentInfo(logEvent)
								&& !LogEventHelper.IsHardwareInfo(logEvent)),
						cfg => cfg.Sink(sink, restrictedToMinimumLevel));
		}

		//public static LoggerConfiguration HardwareInfoSinkToPostgreSql(
		//	this LoggerSinkConfiguration loggerConfiguration,
		//	DBHardwareInfoSinkOptions options,
		//	LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
		//{
		//	if (loggerConfiguration == null)
		//		throw new ArgumentNullException(nameof(loggerConfiguration));

		//	var sink = new DBHardwareInfoSink(options);
		//	return loggerConfiguration
		//			.Conditional(
		//				logEvent => LogEventHelper.IsHardwareInfo(logEvent),
		//				cfg => cfg.Sink(sink, restrictedToMinimumLevel));
		//}

		//public static LoggerConfiguration EnvironmentInfoSinkToPostgreSql(
		//	this LoggerSinkConfiguration loggerConfiguration,
		//	DBEnvironmentInfoSinkOptions options,
		//	LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
		//{
		//	if (loggerConfiguration == null)
		//		throw new ArgumentNullException(nameof(loggerConfiguration));

		//	var sink = new DBEnvironmentInfoSink(options);
		//	return loggerConfiguration
		//			.Conditional(
		//				logEvent => LogEventHelper.IsEnvironmentInfo(logEvent),
		//				cfg => cfg.Sink(sink, restrictedToMinimumLevel));
		//}
	}
}
