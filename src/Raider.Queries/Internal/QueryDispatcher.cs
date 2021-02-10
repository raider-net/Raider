﻿using Microsoft.Extensions.Logging;
using Raider.Queries.Aspects;
using Raider.Queries.Logging;
using Raider.DependencyInjection;
using Raider.Diagnostics;
using Raider.Localization;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.Trace;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Queries.Internal
{
	internal class QueryDispatcher : IQueryDispatcher
	{
		private readonly ServiceFactory _serviceFactory;
		private readonly IApplicationContext _applicationContext;
		private readonly IApplicationResources _applicationResources;
		private readonly ILogger<QueryDispatcher> _logger;
		private readonly IQueryHandlerRegistry _handlerRegistry;
		private readonly IQueryHandlerFactory _handlerFactory;

		private static readonly ConcurrentDictionary<Type, QueryProcessorBase> _queryProcessors = new ConcurrentDictionary<Type, QueryProcessorBase>();
		private static readonly ConcurrentDictionary<Type, QueryProcessorBase> _asyncQueryProcessors = new ConcurrentDictionary<Type, QueryProcessorBase>();

		public QueryDispatcher(
			ServiceFactory serviceFactory,
			IQueryHandlerRegistry handlerRegistry,
			IQueryHandlerFactory handlerFactory,
			ILogger<QueryDispatcher> logger)
		{
			_handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
			_handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
			_logger = logger;
			_serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
			_applicationContext = _serviceFactory.GetRequiredInstance<IApplicationContext>();
			_applicationResources = _serviceFactory.GetRequiredInstance<IApplicationResources>();
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
				x => x.LogCode(LogCode.MethodEntry)
						.CommandQueryName(queryType?.FullName));

			try
			{
				if (query == null || queryType == null)
					throw new ArgumentNullException(nameof(query));

				queryProcessor = (QueryProcessor<TResult>)_queryProcessors.GetOrAdd(queryType,
					t => CreateQueryProcessor(typeof(QueryProcessor<,>).MakeGenericType(queryType, typeof(TResult))));

				handler = queryProcessor.CreateHandler();
				handler.Dispatcher = this;
				var result = queryProcessor.CanExecute(traceInfo, handler, query, options);

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
							.LogCode((long)QueryLogCode.QueryDispatcherError)
							.ClientMessage(_applicationResources.GlobalExceptionMessage)
							.CommandQueryName(queryType?.FullName));

				var result = new QueryResultInternal<bool>();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				queryProcessor?.DisposeHandler(handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.MethodExit)
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
				x => x.LogCode(LogCode.MethodEntry)
						.CommandQueryName(queryType?.FullName));

			try
			{
				if (query == null || queryType == null)
					throw new ArgumentNullException(nameof(query));

				queryProcessor = (AsyncQueryProcessor<TResult>)_asyncQueryProcessors.GetOrAdd(queryType,
					t => CreateQueryProcessor(typeof(AsyncQueryProcessor<,>).MakeGenericType(queryType, typeof(TResult))));

				handler = queryProcessor.CreateHandler();
				handler.Dispatcher = this;
				var result = await queryProcessor.CanExecuteAsync(traceInfo, handler, query, options, cancellationToken);

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
							.LogCode((long)QueryLogCode.QueryDispatcherError)
							.ClientMessage(_applicationResources.GlobalExceptionMessage)
							.CommandQueryName(queryType?.FullName));

				var result = new QueryResultInternal<bool>();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				queryProcessor?.DisposeHandler(handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.MethodExit)
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
				x => x.LogCode(LogCode.MethodEntry)
						.CommandQueryName(queryType?.FullName));

			try
			{
				if (query == null || queryType == null)
					throw new ArgumentNullException(nameof(query));

				queryProcessor = (QueryProcessor<TResult>)_queryProcessors.GetOrAdd(queryType,
					t => CreateQueryProcessor(typeof(QueryProcessor<,>).MakeGenericType(queryType, typeof(TResult))));

				handler = queryProcessor.CreateHandler();
				handler.Dispatcher = this;
				var result = queryProcessor.Execute(traceInfo, handler, query, options);

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
							.LogCode((long)QueryLogCode.QueryDispatcherError)
							.ClientMessage(_applicationResources.GlobalExceptionMessage)
							.CommandQueryName(queryType?.FullName));

				var result = new QueryResultInternal<TResult>();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				queryProcessor?.DisposeHandler(handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.MethodExit)
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
				x => x.LogCode(LogCode.MethodEntry)
						.CommandQueryName(queryType?.FullName));

			try
			{
				if (query == null || queryType == null)
					throw new ArgumentNullException(nameof(query));

				queryProcessor = (AsyncQueryProcessor<TResult>)_asyncQueryProcessors.GetOrAdd(queryType,
					t => CreateQueryProcessor(typeof(AsyncQueryProcessor<,>).MakeGenericType(queryType, typeof(TResult))));

				handler = queryProcessor.CreateHandler();
				handler.Dispatcher = this;
				var result = await queryProcessor.ExecuteAsync(traceInfo, handler, query, options, cancellationToken);

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
							.LogCode((long)QueryLogCode.QueryDispatcherError)
							.ClientMessage(_applicationResources.GlobalExceptionMessage)
							.CommandQueryName(queryType?.FullName));

				var result = new QueryResultInternal<TResult>();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				queryProcessor?.DisposeHandler(handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.MethodExit)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds)
						.CommandQueryName(queryType?.FullName));
			}
		}

#pragma warning disable CS8603 // Possible null reference return.
		private QueryProcessorBase CreateQueryProcessor(Type type)
			=> Activator.CreateInstance(type, _handlerRegistry, _handlerFactory, _serviceFactory) as QueryProcessorBase;
#pragma warning restore CS8603 // Possible null reference return.

	}
}