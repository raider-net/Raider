﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.Identity;
using Raider.Localization;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.Queries;
using Raider.Trace;
using Raider.Web;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.QueryServices.Queries
{
	public abstract class QueryHandlerContext : IQueryHandlerContext, IQueryServiceContext, IDisposable, IAsyncDisposable
	{
		public IServiceProvider ServiceProvider { get; }
		public ITraceInfo TraceInfo { get; protected set; }
		public IApplicationContext ApplicationContext { get; private set; }
		public IApplicationResources ApplicationResources => ApplicationContext.ApplicationResources;
		public IRequestMetadata? RequestMetadata => ApplicationContext.RequestMetadata;
		public RaiderIdentity<int>? User => ApplicationContext.TraceInfo.User;
		public string? QueryName { get; private set; }
		public Guid? IdQueryEntry { get; private set; }
		public ILogger Logger { get; private set; }
		public Dictionary<object, object?> CommandHandlerItems { get; } = new Dictionary<object, object?>();

		public bool IsDisposable { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public QueryHandlerContext(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public virtual TQueryService GetQueryService<TQueryService, TQueryServiceContext>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TQueryServiceContext : QueryServiceContext, new()
			where TQueryService : QueryServiceBase<TQueryServiceContext>
		{
			//if (typeof(IQueryableBase).IsAssignableFrom(typeof(TService)))
			//	throw new NotSupportedException($"For {nameof(IQueryableBase)} use Constructor {nameof(IQueryableBase)}({nameof(QueryHandlerContext)}) or {nameof(IQueryableBase)}({nameof(QueryServiceContext)}) isntead");

			var service = ServiceProvider.GetRequiredService<TQueryService>();

			var traceFrameBuilder = new TraceFrameBuilder(TraceInfo.TraceFrame)
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber);

			service.QueryServiceContext = new TQueryServiceContext();
			service.QueryServiceContext.Init(traceFrameBuilder.Build(), this, typeof(TQueryService));
			service.Initialize();

			return service;
		}

		public virtual async Task<TQueryService> GetQueryServiceAsync<TQueryService, TQueryServiceContext>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0,
			CancellationToken cancellationToken = default)
			where TQueryServiceContext : QueryServiceContext, new()
			where TQueryService : QueryServiceBase<TQueryServiceContext>
		{
			//if (typeof(IQueryableBase).IsAssignableFrom(typeof(TService)))
			//	throw new NotSupportedException($"For {nameof(IQueryableBase)} use Constructor {nameof(IQueryableBase)}({nameof(QueryHandlerContext)}) or {nameof(IQueryableBase)}({nameof(QueryServiceContext)}) isntead");

			var service = ServiceProvider.GetRequiredService<TQueryService>();

			var traceFrameBuilder = new TraceFrameBuilder(TraceInfo.TraceFrame)
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber);

			service.QueryServiceContext = new TQueryServiceContext();
			service.QueryServiceContext.Init(traceFrameBuilder.Build(), this, typeof(TQueryService));
			await service.InitializeAsync(cancellationToken);

			return service;
		}

		public TQueryServiceContext GetQueryServiceContext<TQueryServiceContext>(
			Type typeOfQueryService,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TQueryServiceContext : QueryServiceContext, new()
		{
			var traceFrameBuilder = new TraceFrameBuilder(TraceInfo.TraceFrame)
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber);

			var queryServiceContext = new TQueryServiceContext();
			queryServiceContext.Init(traceFrameBuilder.Build(), this, typeOfQueryService);
			return queryServiceContext;
		}

		public TQueryServiceContext GetQueryServiceContext<TQueryServiceContext>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TQueryServiceContext : QueryServiceContext, new()
		{
			var traceFrameBuilder = new TraceFrameBuilder(TraceInfo.TraceFrame)
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber);

			var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger<TQueryServiceContext>();

			var serviceContext = new TQueryServiceContext();
			serviceContext.Init(traceFrameBuilder.Build(), this, logger);
			return serviceContext;
		}

		public MethodLogScope CreateScope(
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> CreateScope(TraceInfo, methodParameters, memberName, sourceFilePath, sourceLineNumber);

		public MethodLogScope CreateScope(
			MethodLogScope? methodLogScope,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> CreateScope(methodLogScope?.TraceInfo ?? TraceInfo, methodParameters, memberName, sourceFilePath, sourceLineNumber);

		public MethodLogScope CreateScope(
			ITraceInfo? previousTraceInfo,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var traceInfo =
				new TraceInfoBuilder(
					new TraceFrameBuilder(previousTraceInfo?.TraceFrame ?? TraceInfo.TraceFrame)
						.CallerMemberName(memberName)
						.CallerFilePath(sourceFilePath)
						.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
						.MethodParameters(methodParameters)
						.Build(),
					previousTraceInfo ?? TraceInfo)
					.Build();

			var disposable = Logger.BeginScope(new Dictionary<string, Guid?>
			{
				[nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId)] = traceInfo.TraceFrame.MethodCallId,
				[nameof(ILogMessage.TraceInfo.CorrelationId)] = traceInfo.CorrelationId
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
				message.CommandQueryName = QueryName;

			Logger.LogTraceMessage(message);
		}

		public ILogMessage? LogTraceMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogTraceMessage(scope, (x => x.CommandQueryName(QueryName)) + messageBuilder);
		}

		public void LogDebugMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = QueryName;

			Logger.LogDebugMessage(message);
		}

		public ILogMessage? LogDebugMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogDebugMessage(scope, (x => x.CommandQueryName(QueryName)) + messageBuilder);
		}

		public void LogInformationMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = QueryName;

			Logger.LogInformationMessage(message);
		}

		public ILogMessage? LogInformationMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogInformationMessage(scope, (x => x.CommandQueryName(QueryName)) + messageBuilder);
		}

		public void LogWarningMessage(ILogMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = QueryName;

			Logger.LogWarningMessage(message);
		}

		public ILogMessage? LogWarningMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogWarningMessage(scope, (x => x.CommandQueryName(QueryName)) + messageBuilder);
		}

		public void LogErrorMessage(IErrorMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = QueryName;

			Logger.LogErrorMessage(message);
		}

		public IErrorMessage LogErrorMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogErrorMessage(scope, (x => x.CommandQueryName(QueryName)) + messageBuilder);
		}

		public void LogCriticalMessage(IErrorMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrWhiteSpace(message.CommandQueryName))
				message.CommandQueryName = QueryName;

			Logger.LogCriticalMessage(message);
		}

		public IErrorMessage LogCriticalMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
		{
			if (scope?.TraceInfo == null)
				throw new ArgumentNullException($"{nameof(scope)}.{nameof(scope.TraceInfo)}");

			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			return Logger.LogCriticalMessage(scope, (x => x.CommandQueryName(QueryName)) + messageBuilder);
		}

		public virtual bool HasTransaction()
			=> false;

		internal protected virtual void CommitSafe() { }

		internal protected virtual void RollbackSafe() { }

		public virtual void Commit() { }

		public virtual void Rollback() { }

		public virtual void DisposeTransaction() { }

		internal protected virtual Task CommitSafeAsync(CancellationToken cancellationToken = default)
			=> Task.CompletedTask;

		internal protected virtual Task RollbackSafeAsync(CancellationToken cancellationToken = default)
			=> Task.CompletedTask;

		public virtual Task CommitAsync(CancellationToken cancellationToken = default)
			=> Task.CompletedTask;

		public virtual Task RollbackAsync(CancellationToken cancellationToken = default)
			=> Task.CompletedTask;

		public virtual ValueTask DisposeTransactionAsync()
			=> ValueTask.CompletedTask;

		public abstract void Dispose();
		public abstract ValueTask DisposeAsync();

		public abstract class Builder<TContext>
			where TContext : QueryHandlerContext
		{
			public TContext Context { get; }

			public IServiceProvider ServiceProvider { get; }

			public Builder(IServiceProvider serviceProvider)
			{
				ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
				Context = Create();
			}

			internal Builder<TContext> TraceInfo(ITraceInfo traceInfo)
			{
				Context.TraceInfo = traceInfo;
				return this;
			}

			internal Builder<TContext> ApplicationContext(IApplicationContext applicationContext)
			{
				Context.ApplicationContext = applicationContext ?? throw new ArgumentNullException(nameof(applicationContext));
				return this;
			}

			internal Builder<TContext> IdQueryEntry(Guid? idQueryEntry)
			{
				Context.IdQueryEntry = idQueryEntry;
				return this;
			}

			internal Builder<TContext> QueryName(string? queryName)
			{
				Context.QueryName = queryName;
				return this;
			}

			internal Builder<TContext> Logger(ILogger logger)
			{
				Context.Logger = logger;
				return this;
			}

			public abstract TContext Create();
		}
	}
}
