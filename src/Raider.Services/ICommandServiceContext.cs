using Microsoft.Extensions.Logging;
using Raider.Localization;
using Raider.Logging;
using Raider.Trace;
using Raider.Web;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raider.Services
{
	public interface ICommandServiceContext
	{
		IServiceProvider ServiceProvider { get; }
		ITraceInfo TraceInfo { get; }
		IApplicationContext ApplicationContext { get; }
		IApplicationResources ApplicationResources { get; }
		IRequestMetadata? RequestMetadata { get; }
		string? CommandName { get;  }
		Guid? IdCommandEntry { get;  }
		ILogger Logger { get;  }
		Dictionary<object, object?> CommandHandlerItems { get; }

		TService GetService<TService, TServiceContext>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TServiceContext : ServiceContext, new()
			where TService : ServiceBase<TServiceContext>;

		MethodLogScope CreateScope(
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		bool TryGetCommandHandlerItem<TKey, TValue>(TKey key, out TValue? value);

		void LogTraceMessage(ILogMessage message);

		ILogMessage? LogTraceMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder);

		void LogDebugMessage(ILogMessage message);

		ILogMessage? LogDebugMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder);

		void LogInformationMessage(ILogMessage message);

		ILogMessage? LogInformationMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder);

		void LogWarningMessage(ILogMessage message);

		ILogMessage? LogWarningMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder);

		void LogErrorMessage(IErrorMessage message);

		IErrorMessage LogErrorMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder);

		void LogCriticalMessage(IErrorMessage message);

		IErrorMessage LogCriticalMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder);
	}
}
