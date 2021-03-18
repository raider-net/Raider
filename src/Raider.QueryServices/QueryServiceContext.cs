using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Raider.DependencyInjection;
using Raider.EntityFrameworkCore;
using Raider.Identity;
using Raider.Localization;
using Raider.Logging;
using Raider.QueryServices.Queries;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.QueryServices
{
	internal static class DbContextExtensions
	{
		public static DbContext CheckDbTransaction(this DbContext dbContext, TransactionUsage transactionUsage)
		{
			if (transactionUsage == TransactionUsage.NONE && dbContext.Database.CurrentTransaction != null)
				throw new InvalidOperationException($"DbContext has transaction, but expected {nameof(TransactionUsage)} is {transactionUsage}");

			if (transactionUsage == TransactionUsage.ReuseOrCreateNew && dbContext.Database.CurrentTransaction == null)
				throw new InvalidOperationException($"DbContext has no transaction, but expected {nameof(TransactionUsage)} is {transactionUsage}");

			return dbContext;
		}
	}

	public class QueryServiceContext : IServiceContext, IQueryServiceContext
	{
		private readonly QueryHandlerContext _queryHandlerContext;

		public ServiceFactory ServiceFactory => _queryHandlerContext.ServiceFactory;

		public ITraceInfo TraceInfo { get; }

		public RaiderIdentity<int>? User => _queryHandlerContext.User;

		public RaiderPrincipal<int>? Principal => _queryHandlerContext.Principal;

		public string? QueryName => _queryHandlerContext.QueryName;

		public Guid? IdQueryEntry => _queryHandlerContext.IdQueryEntry;

		public IDbContextTransaction? DbContextTransaction => _queryHandlerContext.DbContextTransaction;

		public ILogger Logger { get; }

		public IApplicationResources ApplicationResources => _queryHandlerContext.ApplicationResources;

		public Dictionary<object, object?> CommandHandlerItems => _queryHandlerContext.CommandHandlerItems;
		public Type ForServiceType { get; }
		public bool AllowCommit { get; set; }
		public Dictionary<object, object?> LocalItems { get; } = new Dictionary<object, object?>();

		public QueryServiceContext(ITraceFrame currentTraceFrame, QueryHandlerContext queryHandlerContext, Type serviceType)
		{
			_queryHandlerContext = queryHandlerContext ?? throw new ArgumentNullException(nameof(queryHandlerContext));
			TraceInfo = new TraceInfoBuilder(currentTraceFrame, queryHandlerContext.TraceInfo).Build();
			ForServiceType = serviceType;

			var loggerFactory = ServiceFactory.GetRequiredInstance<ILoggerFactory>();
			var serviceLogger = loggerFactory.CreateLogger(serviceType);
			Logger = serviceLogger;
		}

		public QueryServiceContext(ITraceInfo traceInfo, QueryHandlerContext queryHandlerContext, Type serviceType)
		{
			_queryHandlerContext = queryHandlerContext ?? throw new ArgumentNullException(nameof(queryHandlerContext));
			TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
			ForServiceType = serviceType;

			var loggerFactory = ServiceFactory.GetRequiredInstance<ILoggerFactory>();
			var serviceLogger = loggerFactory.CreateLogger(serviceType);
			Logger = serviceLogger;
		}

		public TContext CreateNewDbContext<TContext>(
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext
			=> _queryHandlerContext.CreateNewDbContext<TContext>(transactionUsage, transactionIsolationLevel);

		public TContext GetOrCreateDbContext<TContext>(
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null) 
			where TContext : DbContext
			=> _queryHandlerContext.GetOrCreateDbContext<TContext>(transactionUsage, transactionIsolationLevel);

		public TService GetQueryService<TService>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TService : QueryServiceBase
			=> _queryHandlerContext.GetQueryService<TService>(memberName, sourceFilePath, sourceLineNumber);

		public Task<TService> GetQueryServiceAsync<TService>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0,
			CancellationToken cancellationToken = default)
			where TService : QueryServiceBase
			=> _queryHandlerContext.GetQueryServiceAsync<TService>(memberName, sourceFilePath, sourceLineNumber, cancellationToken);

		public MethodLogScope CreateScope(
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> _queryHandlerContext.CreateScope(methodParameters, memberName, sourceFilePath, sourceLineNumber);

		public bool TryGetCommandHandlerItem<TKey, TValue>(TKey key, out TValue? value)
			=> _queryHandlerContext.TryGetCommandHandlerItem(key, out value);

		public void LogTraceMessage(ILogMessage message)
			=> _queryHandlerContext.LogTraceMessage(message);

		public ILogMessage? LogTraceMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
			=> _queryHandlerContext.LogTraceMessage(scope, messageBuilder);

		public void LogDebugMessage(ILogMessage message)
			=> _queryHandlerContext.LogDebugMessage(message);

		public ILogMessage? LogDebugMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
			=> _queryHandlerContext.LogDebugMessage(scope, messageBuilder);

		public void LogInformationMessage(ILogMessage message)
			=> _queryHandlerContext.LogInformationMessage(message);

		public ILogMessage? LogInformationMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
			=> _queryHandlerContext.LogInformationMessage(scope, messageBuilder);

		public void LogWarningMessage(ILogMessage message)
			=> _queryHandlerContext.LogWarningMessage(message);

		public ILogMessage? LogWarningMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
			=> _queryHandlerContext.LogWarningMessage(scope, messageBuilder);

		public void LogErrorMessage(IErrorMessage message)
			=> _queryHandlerContext.LogErrorMessage(message);

		public IErrorMessage LogErrorMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
			=> _queryHandlerContext.LogErrorMessage(scope, messageBuilder);

		public void LogCriticalMessage(IErrorMessage message)
			=> _queryHandlerContext.LogCriticalMessage(message);

		public IErrorMessage LogCriticalMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
			=> _queryHandlerContext.LogCriticalMessage(scope, messageBuilder);

		public bool TryGetLocalItem<TKey, TValue>(TKey key, out TValue? value)
		{
			value = default;

			if (key == null)
				return false;

			if (LocalItems.TryGetValue(key, out object? obj))
			{
				if (obj is TValue val)
				{
					value = val;
					return true;
				}
				else
				{
					throw new InvalidOperationException($"TryGetItem: Key = {typeof(TKey).FullName} && {obj?.GetType().FullName} != {typeof(TValue).FullName}");
				}
			}

			return false;
		}
	}
}
