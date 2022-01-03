using Microsoft.Extensions.Logging;
using Raider.Identity;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raider.Logging.Extensions
{
	public static class LoggerExtensions
	{
		public static IDisposable BeginMethodCallScope(this ILogger logger, ITraceInfo traceInfo)
			=> traceInfo?.TraceFrame == null
				? throw new ArgumentNullException(nameof(traceInfo))
				: logger.BeginScope(new Dictionary<string, Guid?>
				{
					[nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId)] = traceInfo.TraceFrame.MethodCallId,
					[nameof(ILogMessage.TraceInfo.CorrelationId)] = traceInfo.CorrelationId
				});

		public static IDisposable BeginMethodCallScope(this ILogger logger, ITraceFrame traceFrame)
			=> traceFrame == null
				? throw new ArgumentNullException(nameof(traceFrame))
				: logger.BeginScope(new Dictionary<string, Guid>
					{
						[nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId)] = traceFrame.MethodCallId
					});

		public static IDisposable BeginMethodCallScope(this ILogger logger, Guid methodCallId)
			=> logger.BeginScope(new Dictionary<string, Guid>
			{
				[nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId)] = methodCallId
			});

		public static void LogTraceMessage(this ILogger logger, ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (!logger.IsEnabled(LogLevel.Trace))
				return;

			message.LogLevel = LogLevel.Trace;

			logger.LogTrace($"{LoggerSettings.LogMessage_Template}", message.ToDictionary());
		}

		public static ILogMessage? LogTraceMessage(this ILogger logger, ITraceInfo traceInfo, Action<LogMessageBuilder> messageBuilder)
		{
			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			if (!logger.IsEnabled(LogLevel.Trace))
				return null;

			var builder = new LogMessageBuilder(traceInfo)
				.LogLevel(LogLevel.Trace);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			logger.LogTrace($"{LoggerSettings.LogMessage_Template}", message.ToDictionary());

			return message;
		}

		public static ILogMessage? LogTraceMessage(
			this ILogger logger,
			Action<LogMessageBuilder> messageBuilder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> LogTraceMessage(logger, TraceInfo.Create((RaiderPrincipal<int>?)null, null, null, memberName, sourceFilePath, sourceLineNumber), messageBuilder);

		public static ILogMessage? LogTraceMessage(this ILogger logger, MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			return LogTraceMessage(logger, scope.TraceInfo, messageBuilder);
		}

		public static void LogDebugMessage(this ILogger logger, ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (!logger.IsEnabled(LogLevel.Debug))
				return;

			message.LogLevel = LogLevel.Debug;

			logger.LogDebug($"{LoggerSettings.LogMessage_Template}", message.ToDictionary());
		}

		public static ILogMessage? LogDebugMessage(this ILogger logger, ITraceInfo traceInfo, Action<LogMessageBuilder> messageBuilder)
		{
			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			if (!logger.IsEnabled(LogLevel.Debug))
				return null;

			var builder = new LogMessageBuilder(traceInfo)
				.LogLevel(LogLevel.Debug);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			logger.LogDebug($"{LoggerSettings.LogMessage_Template}", message.ToDictionary());

			return message;
		}

		public static ILogMessage? LogDebugMessage(
			this ILogger logger,
			Action<LogMessageBuilder> messageBuilder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> LogDebugMessage(logger, TraceInfo.Create((RaiderPrincipal<int>?)null, null, null, memberName, sourceFilePath, sourceLineNumber), messageBuilder);

		public static ILogMessage? LogDebugMessage(this ILogger logger, MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			return LogDebugMessage(logger, scope.TraceInfo, messageBuilder);
		}

		public static void LogInformationMessage(this ILogger logger, ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (!logger.IsEnabled(LogLevel.Information))
				return;

			message.LogLevel = LogLevel.Information;

			logger.LogInformation($"{LoggerSettings.LogMessage_Template}", message.ToDictionary());
		}

		public static ILogMessage? LogInformationMessage(this ILogger logger, ITraceInfo traceInfo, Action<LogMessageBuilder> messageBuilder)
		{
			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			if (!logger.IsEnabled(LogLevel.Information))
				return null;

			var builder = new LogMessageBuilder(traceInfo)
				.LogLevel(LogLevel.Information);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			logger.LogInformation($"{LoggerSettings.LogMessage_Template}", message.ToDictionary());

			return message;
		}

		public static ILogMessage? LogInformationMessage(
			this ILogger logger,
			Action<LogMessageBuilder> messageBuilder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> LogInformationMessage(logger, TraceInfo.Create((RaiderPrincipal<int>?)null, null, null, memberName, sourceFilePath, sourceLineNumber), messageBuilder);

		public static ILogMessage? LogInformationMessage(this ILogger logger, MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			return LogInformationMessage(logger, scope.TraceInfo, messageBuilder);
		}

		public static void LogWarningMessage(this ILogger logger, ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (!logger.IsEnabled(LogLevel.Warning))
				return;

			message.LogLevel = LogLevel.Warning;

			logger.LogWarning($"{LoggerSettings.LogMessage_Template}", message.ToDictionary());
		}

		public static ILogMessage? LogWarningMessage(this ILogger logger, ITraceInfo traceInfo, Action<LogMessageBuilder> messageBuilder)
		{
			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			if (!logger.IsEnabled(LogLevel.Warning))
				return null;

			var builder = new LogMessageBuilder(traceInfo)
				.LogLevel(LogLevel.Warning);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			logger.LogWarning($"{LoggerSettings.LogMessage_Template}", message.ToDictionary());

			return message;
		}

		public static ILogMessage? LogWarningMessage(
			this ILogger logger,
			Action<LogMessageBuilder> messageBuilder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> LogWarningMessage(logger, TraceInfo.Create((RaiderPrincipal<int>?)null, null, null, memberName, sourceFilePath, sourceLineNumber), messageBuilder);

		public static ILogMessage? LogWarningMessage(this ILogger logger, MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			return LogWarningMessage(logger, scope.TraceInfo, messageBuilder);
		}

		public static void LogErrorMessage(this ILogger logger, IErrorMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			message.LogLevel = LogLevel.Error;

			logger.LogError($"{LoggerSettings.LogMessage_Template}", message.ToDictionary());
		}

		public static IErrorMessage LogErrorMessage(this ILogger logger, ITraceInfo traceInfo, Action<ErrorMessageBuilder> messageBuilder)
		{
			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			var builder = new ErrorMessageBuilder(traceInfo)
				.LogLevel(LogLevel.Error);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			logger.LogError($"{LoggerSettings.LogMessage_Template}", message.ToDictionary());

			return message;
		}

		public static IErrorMessage LogErrorMessage(
			this ILogger logger,
			Action<ErrorMessageBuilder> messageBuilder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> LogErrorMessage(logger, TraceInfo.Create((RaiderPrincipal<int>?)null, null, null, memberName, sourceFilePath, sourceLineNumber), messageBuilder);

		public static IErrorMessage LogErrorMessage(this ILogger logger, MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			return LogErrorMessage(logger, scope.TraceInfo, messageBuilder);
		}

		public static void LogCriticalMessage(this ILogger logger, IErrorMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			message.LogLevel = LogLevel.Critical;

			logger.LogCritical($"{LoggerSettings.LogMessage_Template}", message.ToDictionary());
		}

		public static IErrorMessage LogCriticalMessage(this ILogger logger, ITraceInfo traceInfo, Action<ErrorMessageBuilder> messageBuilder)
		{
			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			var builder = new ErrorMessageBuilder(traceInfo)
				.LogLevel(LogLevel.Critical);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			logger.LogCritical($"{LoggerSettings.LogMessage_Template}", message.ToDictionary());

			return message;
		}

		public static IErrorMessage LogCriticalMessage(
			this ILogger logger,
			Action<ErrorMessageBuilder> messageBuilder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> LogCriticalMessage(logger, TraceInfo.Create((RaiderPrincipal<int>?)null, null, null, memberName, sourceFilePath, sourceLineNumber), messageBuilder);

		public static IErrorMessage LogCriticalMessage(this ILogger logger, MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			return LogCriticalMessage(logger, scope.TraceInfo, messageBuilder);
		}

		//public static void LogEnvironmentInfo(this ILogger logger)
		//	=> LogEnvironmentInfo(logger, EnvironmentInfoProvider.GetEnvironmentInfo());

		//public static void LogEnvironmentInfo(this ILogger logger, EnvironmentInfo environmentInfo)
		//{
		//	logger.LogInformation($"{LoggerSettings.EnvironmentInfo_Template}", environmentInfo.ToDictionary());
		//}
	}
}
