//using Raider.AspNetCore.Logging;
//using Raider.AspNetCore.Logging.PostgreSql.Sink;
//using Serilog.Configuration;
//using Serilog.Events;
//using System;

//namespace Serilog
//{
//	public static class SerilogExtensions
//	{
//		public static LoggerConfiguration RequestSinkToPostgreSql(
//			this LoggerSinkConfiguration loggerConfiguration,
//			DBRequestSinkOptions options,
//			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
//		{
//			if (loggerConfiguration == null)
//				throw new ArgumentNullException(nameof(loggerConfiguration));

//			var sink = new DBRequestSink(options);
//			return loggerConfiguration
//					.Conditional(
//						logEvent => LogEventHelper.IsRequest(logEvent),
//						cfg => cfg.Sink(sink, restrictedToMinimumLevel));
//		}

//		public static LoggerConfiguration ResponseSinkToPostgreSql(
//			this LoggerSinkConfiguration loggerConfiguration,
//			DBResponseSinkOptions options,
//			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
//		{
//			if (loggerConfiguration == null)
//				throw new ArgumentNullException(nameof(loggerConfiguration));

//			var sink = new DBResponseSink(options);
//			return loggerConfiguration
//					.Conditional(
//						logEvent => LogEventHelper.IsResponse(logEvent),
//						cfg => cfg.Sink(sink, restrictedToMinimumLevel));
//		}

//		public static LoggerConfiguration RequestAuthenticationSinkToPostgreSql(
//			this LoggerSinkConfiguration loggerConfiguration,
//			DBRequestAuthenticationSinkOptions options,
//			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
//		{
//			if (loggerConfiguration == null)
//				throw new ArgumentNullException(nameof(loggerConfiguration));

//			var sink = new DBRequestAuthenticationSink(options);
//			return loggerConfiguration
//					.Conditional(
//						logEvent => LogEventHelper.IsRequestAuthentication(logEvent),
//						cfg => cfg.Sink(sink, restrictedToMinimumLevel));
//		}
//	}
//}
