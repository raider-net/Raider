using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Raider.Commands;
using Raider.DependencyInjection;
using Raider.EntityFrameworkCore;
using Raider.Identity;
using Raider.Localization;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.Trace;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Raider.Services.Commands
{
	public abstract class CommandHandlerContext : ICommandHandlerContext, ICommandServiceContext
	{
		private readonly ConcurrentDictionary<Type, DbContext> _dbContextCache = new ConcurrentDictionary<Type, DbContext>();

		public ServiceFactory ServiceFactory { get; }
		public ITraceInfo TraceInfo { get; protected set; }
		public RaiderIdentity<int>? User { get; private set; }
		public RaiderPrincipal<int>? Principal { get; private set; }
		public string? CommandName { get; private set; }
		public long? IdCommandEntry { get; private set; }
		public IDbContextTransaction? DbContextTransaction { get; private set; }
		public string? DbContextTransactionId => DbContextTransaction?.TransactionId.ToString();
		public ILogger Logger { get; private set; }
		public IApplicationResources ApplicationResources { get; private set; }
		public Dictionary<object, object?> CommandHandlerItems { get; } = new Dictionary<object, object?>();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public CommandHandlerContext(ServiceFactory serviceFactory)
		{
			ServiceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public TContext CreateNewDbContext<TContext>(TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew, IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext
		{
			var dbContext = DbContextFactory.CreateNewDbContext<TContext>(ServiceFactory, DbContextTransaction, out IDbContextTransaction? newDbContextTransaction, transactionUsage, transactionIsolationLevel);
			DbContextTransaction = newDbContextTransaction;
			return dbContext;
		}

		public TContext GetOrCreateDbContext<TContext>(TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew, IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext
		{
			var result = _dbContextCache.GetOrAdd(typeof(TContext), (dbContextType) => CreateNewDbContext<TContext>(transactionUsage, transactionIsolationLevel)).CheckDbTransaction(transactionUsage);
			return (TContext)result;
		}

		public TService GetService<TService>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TService : ServiceBase
		{
			if (typeof(IQueryableBase).IsAssignableFrom(typeof(TService)))
				throw new NotSupportedException($"For {nameof(IQueryableBase)} use Constructor {nameof(IQueryableBase)}({nameof(CommandHandlerContext)}) or {nameof(IQueryableBase)}({nameof(ServiceContext)}) isntead");

			var service = ServiceFactory.GetRequiredInstance<TService>();

			var traceFrameBuilder = new TraceFrameBuilder(TraceInfo?.TraceFrame)
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber);

			service.ServiceContext = new ServiceContext(traceFrameBuilder.Build(), this, typeof(TService));

			return service;
		}

		internal ServiceContext GetServiceContext(
			Type typeOfService,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var traceFrameBuilder = new TraceFrameBuilder(TraceInfo?.TraceFrame)
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber);

			return new ServiceContext(traceFrameBuilder.Build(), this, typeOfService);
		}

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

		public bool TryGetCommandHandlerItem<TKey, TValue>(TKey key, out TValue? value)
		{
			value = default;

			if (key == null)
				return false;

			if (CommandHandlerItems.TryGetValue(key, out object? obj))
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

		public void LogTraceMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = CommandName;

			Logger.LogTraceMessage(message);
		}

		public ILogMessage? LogTraceMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogTraceMessage(scope, (x => x.CommandQueryName(CommandName)) + messageBuilder);
		}

		public void LogDebugMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = CommandName;

			Logger.LogDebugMessage(message);
		}

		public ILogMessage? LogDebugMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogDebugMessage(scope, (x => x.CommandQueryName(CommandName)) + messageBuilder);
		}

		public void LogInformationMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = CommandName;

			Logger.LogInformationMessage(message);
		}

		public ILogMessage? LogInformationMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogInformationMessage(scope, (x => x.CommandQueryName(CommandName)) + messageBuilder);
		}

		public void LogWarningMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = CommandName;

			Logger.LogWarningMessage(message);
		}

		public ILogMessage? LogWarningMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogWarningMessage(scope, (x => x.CommandQueryName(CommandName)) + messageBuilder);
		}

		public void LogErrorMessage(IErrorMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = CommandName;

			Logger.LogErrorMessage(message);
		}

		public IErrorMessage LogErrorMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogErrorMessage(scope, (x => x.CommandQueryName(CommandName)) + messageBuilder);
		}

		public void LogCriticalMessage(IErrorMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = CommandName;

			Logger.LogCriticalMessage(message);
		}

		public IErrorMessage LogCriticalMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogCriticalMessage(scope, (x => x.CommandQueryName(CommandName)) + messageBuilder);
		}



		public abstract class Builder<TContext>
			where TContext : CommandHandlerContext
		{
			public TContext Context { get; }

			public ServiceFactory ServiceFactory { get; }

			public Builder(ServiceFactory serviceFactory)
			{
				ServiceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
				Context = Create();
			}

			internal Builder<TContext> TraceInfo(ITraceInfo traceInfo)
			{
				Context.TraceInfo = traceInfo;
				return this;
			}

			internal Builder<TContext> User(RaiderIdentity<int>? user)
			{
				Context.User = user;
				return this;
			}

			internal Builder<TContext> Principal(RaiderPrincipal<int>? principal)
			{
				Context.Principal = principal;
				return this;
			}

			internal Builder<TContext> IdCommandEntry(long? idCommandEntry)
			{
				Context.IdCommandEntry = idCommandEntry;
				return this;
			}

			internal Builder<TContext> CommandName(string? commandName)
			{
				Context.CommandName = commandName;
				return this;
			}

			internal Builder<TContext> Logger(ILogger logger)
			{
				Context.Logger = logger;
				return this;
			}

			internal Builder<TContext> ApplicationResources(IApplicationResources applicationResources)
			{
				Context.ApplicationResources = applicationResources;
				return this;
			}

			public abstract TContext Create();
		}
	}
}
