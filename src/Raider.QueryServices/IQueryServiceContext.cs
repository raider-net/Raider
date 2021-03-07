using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Raider.DependencyInjection;
using Raider.EntityFrameworkCore;
using Raider.Identity;
using Raider.Localization;
using Raider.Logging;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace Raider.QueryServices
{
	public interface IQueryServiceContext : IApplicationContext
	{
		ServiceFactory ServiceFactory { get; }
		new ITraceInfo TraceInfo { get; }
		string? QueryName { get;  }
		long? IdQueryEntry { get;  }
		IDbContextTransaction? DbContextTransaction { get;  }
		string? DbContextTransactionId => DbContextTransaction?.TransactionId.ToString();
		ILogger Logger { get;  }
		new IApplicationResources ApplicationResources { get;  }
		Dictionary<object, object?> CommandHandlerItems { get; }

		TContext CreateNewDbContext<TContext>(TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew, IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext;
		TContext GetOrCreateDbContext<TContext>(TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew, IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext;

		TService GetQueryService<TService>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TService : QueryServiceBase;

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
