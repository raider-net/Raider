using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.Identity;
using Raider.Localization;
using Raider.Logging;
using Raider.QueryServices.Queries;
using Raider.Trace;
using Raider.Web;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.QueryServices
{
	public class QueryServiceContext :IServiceContext, IQueryServiceContext, IDisposable, IAsyncDisposable
	{
		private QueryHandlerContext _queryHandlerContext;

		public IServiceProvider ServiceProvider => _queryHandlerContext.ServiceProvider;

		public ITraceInfo TraceInfo { get; private set; }
		public IApplicationContext ApplicationContext => _queryHandlerContext.ApplicationContext;
		public IApplicationResources ApplicationResources => _queryHandlerContext.ApplicationResources;
		public IRequestMetadata? RequestMetadata => _queryHandlerContext.RequestMetadata;
		public RaiderIdentity<int>? User => _queryHandlerContext.User;

		public string? QueryName => _queryHandlerContext.QueryName;

		public Guid? IdQueryEntry => _queryHandlerContext.IdQueryEntry;

		public ILogger Logger { get; private set; }

		public Dictionary<object, object?> CommandHandlerItems => _queryHandlerContext.CommandHandlerItems;
		public bool AllowCommit { get; set; }
		public Dictionary<object, object?> LocalItems { get; } = new Dictionary<object, object?>();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public QueryServiceContext() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		//public QueryServiceContext(ITraceFrame currentTraceFrame, QueryHandlerContext queryHandlerContext, Type serviceType)
		//{
		//	_queryHandlerContext = queryHandlerContext ?? throw new ArgumentNullException(nameof(queryHandlerContext));
		//	TraceInfo = new TraceInfoBuilder(currentTraceFrame, queryHandlerContext.TraceInfo).Build();

		//	var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
		//	var serviceLogger = loggerFactory.CreateLogger(serviceType);
		//	Logger = serviceLogger;
		//}

		//public QueryServiceContext(ITraceInfo traceInfo, QueryHandlerContext queryHandlerContext, Type serviceType)
		//{
		//	_queryHandlerContext = queryHandlerContext ?? throw new ArgumentNullException(nameof(queryHandlerContext));
		//	TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));

		//	var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
		//	var serviceLogger = loggerFactory.CreateLogger(serviceType);
		//	Logger = serviceLogger;
		//}
		internal void Init(ITraceFrame currentTraceFrame, QueryHandlerContext queryHandlerContext, Type serviceType)
		{
			_queryHandlerContext = queryHandlerContext ?? throw new ArgumentNullException(nameof(queryHandlerContext));
			OnSetQueryHandlerContext(_queryHandlerContext);
			TraceInfo = new TraceInfoBuilder(currentTraceFrame, queryHandlerContext.TraceInfo).Build();

			var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
			var serviceLogger = loggerFactory.CreateLogger(serviceType);
			Logger = serviceLogger;
		}

		internal void Init(ITraceInfo traceInfo, QueryHandlerContext queryHandlerContext, Type serviceType)
		{
			_queryHandlerContext = queryHandlerContext ?? throw new ArgumentNullException(nameof(queryHandlerContext));
			OnSetQueryHandlerContext(_queryHandlerContext);
			TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));

			var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
			var serviceLogger = loggerFactory.CreateLogger(serviceType);
			Logger = serviceLogger;
		}

		internal void Init(ITraceFrame currentTraceFrame, QueryHandlerContext queryHandlerContext, ILogger logger)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_queryHandlerContext = queryHandlerContext ?? throw new ArgumentNullException(nameof(queryHandlerContext));
			OnSetQueryHandlerContext(_queryHandlerContext);
			TraceInfo = new TraceInfoBuilder(currentTraceFrame, queryHandlerContext.TraceInfo).Build();
		}

		internal void Init(ITraceInfo traceInfo, QueryHandlerContext queryHandlerContext, ILogger logger)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_queryHandlerContext = queryHandlerContext ?? throw new ArgumentNullException(nameof(queryHandlerContext));
			OnSetQueryHandlerContext(_queryHandlerContext);
			TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
		}

		protected virtual void OnSetQueryHandlerContext(QueryHandlerContext queryHandlerContext)
		{
		}

		public TQueryService GetQueryService<TQueryService, TQueryServiceContext>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TQueryServiceContext : QueryServiceContext, new()
			where TQueryService : QueryServiceBase<TQueryServiceContext>
			=> _queryHandlerContext.GetQueryService<TQueryService, TQueryServiceContext>(memberName, sourceFilePath, sourceLineNumber);

		public Task<TQueryService> GetQueryServiceAsync<TQueryService, TQueryServiceContext>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0,
			CancellationToken cancellationToken = default)
			where TQueryServiceContext : QueryServiceContext, new()
			where TQueryService : QueryServiceBase<TQueryServiceContext>
			=> _queryHandlerContext.GetQueryServiceAsync<TQueryService, TQueryServiceContext>(memberName, sourceFilePath, sourceLineNumber, cancellationToken);

		public MethodLogScope CreateScope(
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> _queryHandlerContext.CreateScope(methodParameters, memberName, sourceFilePath, sourceLineNumber);

		public MethodLogScope CreateScope(
			MethodLogScope? methodLogScope,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> _queryHandlerContext.CreateScope(methodLogScope, methodParameters, memberName, sourceFilePath, sourceLineNumber);

		public MethodLogScope CreateScope(
			ITraceInfo? previousTraceInfo,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> _queryHandlerContext.CreateScope(previousTraceInfo, methodParameters, memberName, sourceFilePath, sourceLineNumber);


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

		public void Dispose()
		{
			_queryHandlerContext?.Dispose();
		}

		public ValueTask DisposeAsync()
		{
			if (_queryHandlerContext != null)
				return _queryHandlerContext.DisposeAsync();

			return ValueTask.CompletedTask;
		}

		public void SetIsDisposable()
		{
			if (_queryHandlerContext != null)
				_queryHandlerContext.IsDisposable = true;
		}
	}
}
