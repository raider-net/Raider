﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Raider.Commands;
using Raider.DependencyInjection;
using Raider.Identity;
using Raider.Localization;
using Raider.Logging;
using Raider.Trace;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Raider.Services.Commands
{
	//delegate TContext DbContextFactory<TContext>(TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew, IsolationLevel? transactionIsolationLevel = null)
	//	where TContext : DbContext;

	public abstract class CommandHandlerContext : ICommandHandlerContext
	{
		private readonly ConcurrentDictionary<Type, DbContext> _dbContextCache = new ConcurrentDictionary<Type, DbContext>();

		public ServiceFactory ServiceFactory { get; }
		public ITraceInfo? TraceInfo { get; protected set; }
		public RaiderIdentity<int>? User { get; private set; }
		public RaiderPrincipal<int>? Principal { get; private set; }
		public string? CommandName { get; private set; }
		public long? IdCommandEntry { get; private set; }
		public IDbContextTransaction? DbContextTransaction { get; private set; }
		public string? DbContextTransactionId => DbContextTransaction?.TransactionId.ToString();
		public ILogger Logger { get; private set; }
		public IApplicationResources? ApplicationResources { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public CommandHandlerContext(ServiceFactory serviceFactory)
		{
			ServiceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public CommandHandlerContext(ITraceFrame currentTraceFrame, CommandHandlerContext commandHandlerContext, Type serviceType)
		{
			if (currentTraceFrame == null)
				throw new ArgumentNullException(nameof(currentTraceFrame));
			if (commandHandlerContext == null)
				throw new ArgumentNullException(nameof(commandHandlerContext));

			ServiceFactory = commandHandlerContext.ServiceFactory;
			TraceInfo = new TraceInfoBuilder(currentTraceFrame, commandHandlerContext.TraceInfo).Build();
			User = commandHandlerContext.User;
			Principal = commandHandlerContext.Principal;
			CommandName = commandHandlerContext.CommandName;
			IdCommandEntry = commandHandlerContext.IdCommandEntry;
			DbContextTransaction = commandHandlerContext.DbContextTransaction;
			ApplicationResources = commandHandlerContext.ApplicationResources;

			var loggerFactory = ServiceFactory.GetRequiredInstance<ILoggerFactory>();
			var serviceLogger = loggerFactory.CreateLogger(serviceType);
			Logger = serviceLogger;
		}

		public TContext CreateNewDbContext<TContext>(TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew, IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext
		{
			var dbContext = ServiceFactory.GetRequiredInstance<TContext>();

			if (transactionUsage == TransactionUsage.ReuseOrCreateNew)
			{
				if (DbContextTransaction == null)
				{
					if (transactionIsolationLevel.HasValue)
					{
						DbContextTransaction = dbContext.Database.BeginTransaction(transactionIsolationLevel.Value);
					}
					else
					{
						DbContextTransaction = dbContext.Database.BeginTransaction();
					}
				}
				else
				{
					dbContext.Database.UseTransaction((DbTransaction)DbContextTransaction);
				}
			}

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
			var service = ServiceFactory.GetRequiredInstance<TService>();

			var traceFrameBuilder = new TraceFrameBuilder(TraceInfo?.TraceFrame)
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber);

			service.ServiceContext = new ServiceContext(traceFrameBuilder.Build(), this, typeof(TService));

			return service;
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

		public void LogTraceMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (!Logger.IsEnabled(LogLevel.Trace))
				return;

			message.LogLevel = LogLevel.Trace;
			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = CommandName;

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
				.CommandQueryName(CommandName);

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
				message.CommandQueryName = CommandName;

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
				.CommandQueryName(CommandName);

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
				message.CommandQueryName = CommandName;

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
				.CommandQueryName(CommandName);

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
				message.CommandQueryName = CommandName;

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
				.CommandQueryName(CommandName);

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
				message.CommandQueryName = CommandName;

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
				.CommandQueryName(CommandName);

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
				message.CommandQueryName = CommandName;

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
				.CommandQueryName(CommandName);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			Logger.LogCritical($"{LoggerSettings.FWK_LogMessage_Template}", message.ToDictionary());

			return message;
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

			public Builder<TContext> TraceInfo(ITraceInfo? traceInfo)
			{
				Context.TraceInfo = traceInfo;
				return this;
			}

			public Builder<TContext> User(RaiderIdentity<int>? user)
			{
				Context.User = user;
				return this;
			}

			public Builder<TContext> Principal(RaiderPrincipal<int>? principal)
			{
				Context.Principal = principal;
				return this;
			}

			public Builder<TContext> IdCommandEntry(long? idCommandEntry)
			{
				Context.IdCommandEntry = idCommandEntry;
				return this;
			}

			public Builder<TContext> CommandName(string? commandName)
			{
				Context.CommandName = commandName;
				return this;
			}

			public Builder<TContext> Logger(ILogger logger)
			{
				Context.Logger = logger;
				return this;
			}

			public Builder<TContext> ApplicationResources(IApplicationResources? applicationResources)
			{
				Context.ApplicationResources = applicationResources;
				return this;
			}

			public abstract TContext Create();
		}
	}
}
