using Microsoft.Extensions.Logging;
using Raider.Localization;
using Raider.Logging;
using Raider.Trace;
using Raider.Web;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raider.QueryServices
{
	public interface IQueryServiceContext : IDisposable, IAsyncDisposable
	{
		IServiceProvider ServiceProvider { get; }
		ITraceInfo TraceInfo { get; }
		IApplicationContext ApplicationContext { get; }
		IApplicationResources ApplicationResources { get; }
		IRequestMetadata? RequestMetadata { get; }
		string? QueryName { get;  }
		Guid? IdQueryEntry { get;  }
		ILogger Logger { get;  }
		Dictionary<object, object?> CommandHandlerItems { get; }

		TQueryService GetQueryService<TQueryService, TQueryServiceContext>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TQueryServiceContext : QueryServiceContext, new()
			where TQueryService : QueryServiceBase<TQueryServiceContext>;

		MethodLogScope CreateScope(
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		MethodLogScope CreateScope(
			MethodLogScope? methodLogScope,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		MethodLogScope CreateScope(
			ITraceInfo? previousTraceInfo,
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
