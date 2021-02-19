using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Raider.DependencyInjection;
using Raider.Identity;
using Raider.Localization;
using Raider.Logging;
using Raider.Services.Commands;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace Raider.Services
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

	public class ServiceContext : IServiceContext, ICommandServiceContext
	{
		private readonly CommandHandlerContext _commandHandlerContext;

		public ServiceFactory ServiceFactory => _commandHandlerContext.ServiceFactory;

		public ITraceInfo TraceInfo { get; }

		public RaiderIdentity<int>? User => _commandHandlerContext.User;

		public RaiderPrincipal<int>? Principal => _commandHandlerContext.Principal;

		public string? CommandName => _commandHandlerContext.CommandName;

		public long? IdCommandEntry => _commandHandlerContext.IdCommandEntry;

		public IDbContextTransaction? DbContextTransaction => _commandHandlerContext.DbContextTransaction;

		public ILogger Logger { get; }

		public IApplicationResources ApplicationResources => _commandHandlerContext.ApplicationResources;

		public Dictionary<object, object?> CommandHandlerItems => _commandHandlerContext.CommandHandlerItems;

		public Type ForServiceType { get; }
		public bool AllowCommit { get; set; }
		public Dictionary<object, object?> LocalItems { get; } = new Dictionary<object, object?>();

		public ServiceContext(ITraceFrame currentTraceFrame, CommandHandlerContext commandHandlerContext, Type serviceType)
		{
			_commandHandlerContext = commandHandlerContext ?? throw new ArgumentNullException(nameof(commandHandlerContext));
			TraceInfo = new TraceInfoBuilder(currentTraceFrame, commandHandlerContext.TraceInfo).Build();
			ForServiceType = serviceType;

			var loggerFactory = ServiceFactory.GetRequiredInstance<ILoggerFactory>();
			var serviceLogger = loggerFactory.CreateLogger(serviceType);
			Logger = serviceLogger;
		}

		internal ServiceContext(ITraceInfo traceInfo, CommandHandlerContext commandHandlerContext, Type serviceType)
		{
			_commandHandlerContext = commandHandlerContext ?? throw new ArgumentNullException(nameof(commandHandlerContext));
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
			=> _commandHandlerContext.CreateNewDbContext<TContext>(transactionUsage, transactionIsolationLevel);

		public TContext GetOrCreateDbContext<TContext>(
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null) 
			where TContext : DbContext
			=> _commandHandlerContext.GetOrCreateDbContext<TContext>(transactionUsage, transactionIsolationLevel);

		public TService GetService<TService>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TService : ServiceBase
			=> _commandHandlerContext.GetService<TService>(memberName, sourceFilePath, sourceLineNumber);

		public MethodLogScope CreateScope(
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> _commandHandlerContext.CreateScope(methodParameters, memberName, sourceFilePath, sourceLineNumber);

		public bool TryGetCommandHandlerItem<TKey, TValue>(TKey key, out TValue? value)
			=> _commandHandlerContext.TryGetCommandHandlerItem(key, out value);

		public void LogTraceMessage(ILogMessage message)
			=> _commandHandlerContext.LogTraceMessage(message);

		public ILogMessage? LogTraceMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
			=> _commandHandlerContext.LogTraceMessage(scope, messageBuilder);

		public void LogDebugMessage(ILogMessage message)
			=> _commandHandlerContext.LogDebugMessage(message);

		public ILogMessage? LogDebugMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
			=> _commandHandlerContext.LogDebugMessage(scope, messageBuilder);

		public void LogInformationMessage(ILogMessage message)
			=> _commandHandlerContext.LogInformationMessage(message);

		public ILogMessage? LogInformationMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
			=> _commandHandlerContext.LogInformationMessage(scope, messageBuilder);

		public void LogWarningMessage(ILogMessage message)
			=> _commandHandlerContext.LogWarningMessage(message);

		public ILogMessage? LogWarningMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
			=> _commandHandlerContext.LogWarningMessage(scope, messageBuilder);

		public void LogErrorMessage(IErrorMessage message)
			=> _commandHandlerContext.LogErrorMessage(message);

		public IErrorMessage LogErrorMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
			=> _commandHandlerContext.LogErrorMessage(scope, messageBuilder);

		public void LogCriticalMessage(IErrorMessage message)
			=> _commandHandlerContext.LogCriticalMessage(message);

		public IErrorMessage LogCriticalMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
			=> _commandHandlerContext.LogCriticalMessage(scope, messageBuilder);

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
