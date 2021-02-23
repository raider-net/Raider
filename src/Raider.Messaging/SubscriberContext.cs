using Microsoft.Extensions.Logging;
using Raider.DependencyInjection;
using Raider.Localization;
using Raider.Logging;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raider.Messaging
{
	public class SubscriberContext
	{
		private readonly string _commandName = $"{nameof(Raider)}.{nameof(Messaging)}";

		public ServiceFactory ServiceFactory { get; }
		public ITraceInfo TraceInfo { get; internal set; }
		public ILogger Logger { get; internal set; }
		public IApplicationResources ApplicationResources { get; internal set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public SubscriberContext(ServiceFactory serviceFactory)
		{
			ServiceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public MethodLogScope CreateScope(
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var traceInfo =
				new TraceInfoBuilder(
					new TraceFrameBuilder(TraceInfo?.TraceFrame)
						.CallerMemberName(memberName)
						.CallerFilePath(sourceFilePath)
						.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
						.MethodParameters(methodParameters)
						.Build(),
					TraceInfo)
					.Build();

			var disposable = Logger.BeginScope(new Dictionary<string, Guid>
			{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
				[nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId)] = traceInfo.TraceFrame.MethodCallId
#pragma warning restore CS8602 // Dereference of a possibly null reference.
			});

			var scope = new MethodLogScope(traceInfo, disposable);
			return scope;
		}

		public void LogTraceMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (!Logger.IsEnabled(LogLevel.Trace))
				return;

			message.LogLevel = LogLevel.Trace;
			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = _commandName;

			Logger.LogTrace($"{LoggerSettings.FWK_LogMessage_Template}", message.ToDictionary());
		}

		public ILogMessage? LogTraceMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			if (!Logger.IsEnabled(LogLevel.Trace))
				return null;

			var builder = new LogMessageBuilder(scope.TraceInfo)
				.LogLevel(LogLevel.Trace)
				.CommandQueryName(_commandName);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			Logger.LogTrace($"{LoggerSettings.FWK_LogMessage_Template}", message.ToDictionary());

			return message;
		}

		public void LogDebugMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (!Logger.IsEnabled(LogLevel.Debug))
				return;

			message.LogLevel = LogLevel.Debug;
			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = _commandName;

			Logger.LogDebug($"{LoggerSettings.FWK_LogMessage_Template}", message.ToDictionary());
		}

		public ILogMessage? LogDebugMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			if (!Logger.IsEnabled(LogLevel.Debug))
				return null;

			var builder = new LogMessageBuilder(scope.TraceInfo)
				.LogLevel(LogLevel.Debug)
				.CommandQueryName(_commandName);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			Logger.LogDebug($"{LoggerSettings.FWK_LogMessage_Template}", message.ToDictionary());

			return message;
		}

		public void LogInformationMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (!Logger.IsEnabled(LogLevel.Information))
				return;

			message.LogLevel = LogLevel.Information;
			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = _commandName;

			Logger.LogInformation($"{LoggerSettings.FWK_LogMessage_Template}", message.ToDictionary());
		}

		public ILogMessage? LogInformationMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			if (!Logger.IsEnabled(LogLevel.Information))
				return null;

			var builder = new LogMessageBuilder(scope.TraceInfo)
				.LogLevel(LogLevel.Information)
				.CommandQueryName(_commandName);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			Logger.LogInformation($"{LoggerSettings.FWK_LogMessage_Template}", message.ToDictionary());

			return message;
		}

		public void LogWarningMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (!Logger.IsEnabled(LogLevel.Warning))
				return;

			message.LogLevel = LogLevel.Warning;
			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = _commandName;

			Logger.LogWarning($"{LoggerSettings.FWK_LogMessage_Template}", message.ToDictionary());
		}

		public ILogMessage? LogWarningMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			if (!Logger.IsEnabled(LogLevel.Warning))
				return null;

			var builder = new LogMessageBuilder(scope.TraceInfo)
				.LogLevel(LogLevel.Warning)
				.CommandQueryName(_commandName);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			Logger.LogWarning($"{LoggerSettings.FWK_LogMessage_Template}", message.ToDictionary());

			return message;
		}

		public void LogErrorMessage(IErrorMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			message.LogLevel = LogLevel.Error;
			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = _commandName;

			Logger.LogError($"{LoggerSettings.FWK_LogMessage_Template}", message.ToDictionary());
		}

		public IErrorMessage LogErrorMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			var builder = new ErrorMessageBuilder(scope.TraceInfo)
				.LogLevel(LogLevel.Error)
				.CommandQueryName(_commandName);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			Logger.LogError($"{LoggerSettings.FWK_LogMessage_Template}", message.ToDictionary());

			return message;
		}

		public void LogCriticalMessage(IErrorMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			message.LogLevel = LogLevel.Critical;
			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = _commandName;

			Logger.LogCritical($"{LoggerSettings.FWK_LogMessage_Template}", message.ToDictionary());
		}

		public IErrorMessage LogCriticalMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			var builder = new ErrorMessageBuilder(scope.TraceInfo)
				.LogLevel(LogLevel.Critical)
				.CommandQueryName(_commandName);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			Logger.LogCritical($"{LoggerSettings.FWK_LogMessage_Template}", message.ToDictionary());

			return message;
		}
	}
}
