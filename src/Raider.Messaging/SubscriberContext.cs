﻿using Microsoft.Extensions.Logging;
using Raider.Identity;
using Raider.Localization;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.Trace;
using Raider.Web;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raider.Messaging
{
	public class SubscriberContext
	{
		private readonly string _commandName = $"{nameof(Raider)}.{nameof(Messaging)}";

		public IServiceProvider ServiceProvider { get; }
		public ILogger Logger { get; internal set; }
		public ITraceInfo TraceInfo { get; internal set; }
		public IApplicationContext ApplicationContext { get; }
		public IApplicationResources ApplicationResources => ApplicationContext.ApplicationResources;
		public IRequestMetadata? RequestMetadata => ApplicationContext.RequestMetadata;
		public RaiderPrincipal<int>? Principal => ApplicationContext.TraceInfo.Principal;
		public RaiderIdentity<int>? User => ApplicationContext.TraceInfo.User;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public SubscriberContext(IServiceProvider serviceProvider, IApplicationContext applicationContext)
		{
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			ApplicationContext = applicationContext ?? throw new ArgumentNullException(nameof(applicationContext));
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public MethodLogScope CreateScope(
			RaiderPrincipal<int>? principal,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var traceInfo =
				new TraceInfoBuilder(
					new TraceFrameBuilder(TraceInfo.TraceFrame)
						.CallerMemberName(memberName)
						.CallerFilePath(sourceFilePath)
						.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
						.MethodParameters(methodParameters)
						.Build(),
					TraceInfo)
					.Principal(principal)
					.Build();

			var disposable = Logger.BeginScope(new Dictionary<string, Guid?>
			{
				[nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId)] = traceInfo.TraceFrame.MethodCallId,
				[nameof(ILogMessage.TraceInfo.CorrelationId)] = traceInfo.CorrelationId
			});

			var scope = new MethodLogScope(traceInfo, disposable);
			return scope;
		}

		public void LogTraceMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = _commandName;

			Logger.LogTraceMessage(message);
		}

		public ILogMessage? LogTraceMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogTraceMessage(scope, (x => x.CommandQueryName(_commandName)) + messageBuilder);
		}

		public void LogDebugMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = _commandName;

			Logger.LogDebugMessage(message);
		}

		public ILogMessage? LogDebugMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogDebugMessage(scope, (x => x.CommandQueryName(_commandName)) + messageBuilder);
		}

		public void LogInformationMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = _commandName;

			Logger.LogInformationMessage(message);
		}

		public ILogMessage? LogInformationMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogInformationMessage(scope, (x => x.CommandQueryName(_commandName)) + messageBuilder);
		}

		public void LogWarningMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = _commandName;

			Logger.LogWarningMessage(message);
		}

		public ILogMessage? LogWarningMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogWarningMessage(scope, (x => x.CommandQueryName(_commandName)) + messageBuilder);
		}

		public void LogErrorMessage(IErrorMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = _commandName;

			Logger.LogErrorMessage(message);
		}

		public IErrorMessage LogErrorMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogErrorMessage(scope, (x => x.CommandQueryName(_commandName)) + messageBuilder);
		}

		public void LogCriticalMessage(IErrorMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = _commandName;

			Logger.LogCriticalMessage(message);
		}

		public IErrorMessage LogCriticalMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogCriticalMessage(scope, (x => x.CommandQueryName(_commandName)) + messageBuilder);
		}
	}
}
