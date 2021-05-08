using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.Diagnostics;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.Queries.Aspects;
using Raider.Queries.Logging;
using Raider.Trace;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Queries.Internal
{
	internal class QueryDispatcher : IQueryDispatcher
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IApplicationContext _applicationContext;
		private readonly ILogger<QueryDispatcher> _logger;
		private readonly IQueryHandlerRegistry _handlerRegistry;
		private readonly IQueryHandlerFactory _handlerFactory;

		private static readonly ConcurrentDictionary<Type, QueryProcessorBase> _queryProcessors = new ConcurrentDictionary<Type, QueryProcessorBase>();
		private static readonly ConcurrentDictionary<Type, QueryProcessorBase> _asyncQueryProcessors = new ConcurrentDictionary<Type, QueryProcessorBase>();

		public QueryDispatcher(
			IServiceProvider serviceProvider,
			IQueryHandlerRegistry handlerRegistry,
			IQueryHandlerFactory handlerFactory,
			ILogger<QueryDispatcher> logger)
		{
			_handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
			_handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
			_logger = logger;
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_applicationContext = _serviceProvider.GetRequiredService<IApplicationContext>();
		}

		public IQueryResult<bool> CanExecute<TResult>(IQuery<TResult> query, IQueryInterceptorOptions? options = default)
		{
			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;

			Type? queryType = query?.GetType();
			QueryProcessor<TResult>? queryProcessor = null;
			IQueryHandler? handler = null;
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), _applicationContext.TraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.Method_In)
						.CommandQueryName(queryType?.FullName));

			try
			{
				if (query == null || queryType == null)
					throw new ArgumentNullException(nameof(query));

				queryProcessor = (QueryProcessor<TResult>)_queryProcessors.GetOrAdd(queryType,
					t => CreateQueryProcessor(typeof(QueryProcessor<,>).MakeGenericType(queryType, typeof(TResult))));

				handler = queryProcessor.CreateHandler(_handlerFactory);
				handler.Dispatcher = this;
				var result = queryProcessor.CanExecute(traceInfo, handler, query, options, _applicationContext);

				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				return result;
			}
			catch (Exception ex)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				var errorMessage =
					_logger.LogErrorMessage(
						traceInfo,
						x => x.ExceptionInfo(ex)
							.Detail($"{nameof(CanExecute)}<{nameof(TResult)}> error - Query = {query?.GetType().FullName ?? "NULL"}")
							.LogCode(QueryLogCode.Ex_QryDisp.ToString())
							.ClientMessage(_applicationContext.ApplicationResources?.GlobalExceptionMessage)
							.CommandQueryName(queryType?.FullName));

				var result = new QueryResultInternal<bool>();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				queryProcessor?.DisposeHandler(_handlerFactory, handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.Method_Out)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds)
						.CommandQueryName(queryType?.FullName));
			}
		}

		public async Task<IQueryResult<bool>> CanExecuteAsync<TResult>(IQuery<TResult> query, IQueryInterceptorOptions? options = default, CancellationToken cancellationToken = default)
		{
			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;

			Type? queryType = query?.GetType();
			AsyncQueryProcessor<TResult>? queryProcessor = null;
			IQueryHandler? handler = null;
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), _applicationContext.TraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.Method_In)
						.CommandQueryName(queryType?.FullName));

			try
			{
				if (query == null || queryType == null)
					throw new ArgumentNullException(nameof(query));

				queryProcessor = (AsyncQueryProcessor<TResult>)_asyncQueryProcessors.GetOrAdd(queryType,
					t => CreateQueryProcessor(typeof(AsyncQueryProcessor<,>).MakeGenericType(queryType, typeof(TResult))));

				handler = queryProcessor.CreateHandler(_handlerFactory);
				handler.Dispatcher = this;
				var result = await queryProcessor.CanExecuteAsync(traceInfo, handler, query, options, _applicationContext, cancellationToken);

				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				return result;
			}
			catch (Exception ex)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				var errorMessage =
					_logger.LogErrorMessage(
						traceInfo,
						x => x.ExceptionInfo(ex)
							.Detail($"{nameof(CanExecuteAsync)}<{nameof(TResult)}> error - Query = {query?.GetType().FullName ?? "NULL"}")
							.LogCode(QueryLogCode.Ex_QryDisp.ToString())
							.ClientMessage(_applicationContext.ApplicationResources?.GlobalExceptionMessage)
							.CommandQueryName(queryType?.FullName));

				var result = new QueryResultInternal<bool>();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				queryProcessor?.DisposeHandler(_handlerFactory, handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.Method_Out)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds)
						.CommandQueryName(queryType?.FullName));
			}
		}

		public IQueryResult<TResult> Execute<TResult>(IQuery<TResult> query, IQueryInterceptorOptions? options = default)
		{
			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;

			Type? queryType = query?.GetType();
			QueryProcessor<TResult>? queryProcessor = null;
			IQueryHandler? handler = null;
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), _applicationContext.TraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.Method_In)
						.CommandQueryName(queryType?.FullName));

			try
			{
				if (query == null || queryType == null)
					throw new ArgumentNullException(nameof(query));

				queryProcessor = (QueryProcessor<TResult>)_queryProcessors.GetOrAdd(queryType,
					t => CreateQueryProcessor(typeof(QueryProcessor<,>).MakeGenericType(queryType, typeof(TResult))));

				handler = queryProcessor.CreateHandler(_handlerFactory);
				handler.Dispatcher = this;
				var result = queryProcessor.Execute(traceInfo, handler, query, options, _applicationContext);

				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				return result;
			}
			catch (Exception ex)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				var errorMessage =
					_logger.LogErrorMessage(
						traceInfo,
						x => x.ExceptionInfo(ex)
							.Detail($"{nameof(Execute)}<{nameof(TResult)}> error - Query = {query?.GetType().FullName ?? "NULL"}")
							.LogCode(QueryLogCode.Ex_QryDisp.ToString())
							.ClientMessage(_applicationContext.ApplicationResources?.GlobalExceptionMessage)
							.CommandQueryName(queryType?.FullName));

				var result = new QueryResultInternal<TResult>();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				queryProcessor?.DisposeHandler(_handlerFactory, handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.Method_Out)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds)
						.CommandQueryName(queryType?.FullName));
			}
		}

		public async Task<IQueryResult<TResult>> ExecuteAsync<TResult>(IQuery<TResult> query, IQueryInterceptorOptions? options = default, CancellationToken cancellationToken = default)
		{
			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;

			Type? queryType = query?.GetType();
			AsyncQueryProcessor<TResult>? queryProcessor = null;
			IQueryHandler? handler = null;
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), _applicationContext.TraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.Method_In)
						.CommandQueryName(queryType?.FullName));

			try
			{
				if (query == null || queryType == null)
					throw new ArgumentNullException(nameof(query));

				queryProcessor = (AsyncQueryProcessor<TResult>)_asyncQueryProcessors.GetOrAdd(queryType,
					t => CreateQueryProcessor(typeof(AsyncQueryProcessor<,>).MakeGenericType(queryType, typeof(TResult))));

				handler = queryProcessor.CreateHandler(_handlerFactory);
				handler.Dispatcher = this;
				var result = await queryProcessor.ExecuteAsync(traceInfo, handler, query, options, _applicationContext, cancellationToken);

				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				return result;
			}
			catch (Exception ex)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				var errorMessage =
					_logger.LogErrorMessage(
						traceInfo,
						x => x.ExceptionInfo(ex)
							.Detail($"{nameof(ExecuteAsync)}<{nameof(TResult)}> error - Query = {query?.GetType().FullName ?? "NULL"}")
							.LogCode(QueryLogCode.Ex_QryDisp.ToString())
							.ClientMessage(_applicationContext.ApplicationResources?.GlobalExceptionMessage)
							.CommandQueryName(queryType?.FullName));

				var result = new QueryResultInternal<TResult>();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				queryProcessor?.DisposeHandler(_handlerFactory, handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.Method_Out)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds)
						.CommandQueryName(queryType?.FullName));
			}
		}

#pragma warning disable CS8603 // Possible null reference return.
		private QueryProcessorBase CreateQueryProcessor(Type type)
			=> Activator.CreateInstance(type, _handlerRegistry) as QueryProcessorBase;
#pragma warning restore CS8603 // Possible null reference return.

	}
}
