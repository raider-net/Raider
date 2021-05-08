using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.Diagnostics;
using Raider.Extensions;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.Queries;
using Raider.Queries.Aspects;
using Raider.QueryServices.Queries;
using Raider.Trace;
using System;

namespace Raider.QueryServices.Aspects
{
	public class QueryInterceptor<TQuery, TResult, TContext, TBuilder> : InterceptorBase<TContext, TBuilder>, IQueryInterceptor<TQuery, TResult>
		where TQuery : IQuery<TResult>
		where TContext : QueryHandlerContext
		where TBuilder : QueryHandlerContext.Builder<TContext>
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _logger;

		public QueryInterceptor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ILogger<QueryInterceptor<TQuery, TResult, TContext, TBuilder>> logger)
			: base(serviceProvider)
		{
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public IQueryResult<bool> InterceptCanExecute(ITraceInfo previousTraceInfo, IQueryHandler<TQuery, TResult> handler, TQuery query, IQueryInterceptorOptions? options)
		{
			throw new NotImplementedException();

			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;
			Type? queryType = query?.GetType();
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), previousTraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.Method_In)
						.CommandQueryName(queryType?.FullName));

			if (handler.GetOptions() is not QueryHandlerOptions handlerOptions)
				handlerOptions = new QueryHandlerOptions();

			var resultBuilder = new QueryResultBuilder<bool>();
			var result = resultBuilder.Build();

			try
			{
				var queryHandlerContextBuilder =
					CreateQueryHandlerContext(traceInfo, _loggerFactory.CreateLogger(handler.GetType()))
						.QueryName(queryType?.FullName);

				try
				{
					var handlerResult = handler.CanExecute(query, queryHandlerContextBuilder.Context);
					if (handlerResult == null)
						throw new InvalidOperationException($"Handler {handler.GetType().FullName}.{nameof(handler.CanExecute)} returned null. Expected {typeof(IQueryResult<TResult>).FullName}");

					callEndTicks = StaticWatch.CurrentTicks;
					methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);
				}
				catch (Exception executeEx)
				{
					callEndTicks = StaticWatch.CurrentTicks;
					methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

					result = new QueryResultBuilder<bool>()
						.WithError(traceInfo,
							x => x.ExceptionInfo(executeEx)
									.Detail($"Unhandled handler ({handler.GetType().FullName}) exception."))
						.Build();
				}
			}
			catch (Exception interEx)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				result = new QueryResultBuilder<bool>()
					.WithError(traceInfo,
						x => x.ExceptionInfo(interEx)
								.Detail($"Unhandled interceptor ({this.GetType().FullName}) exception."))
					.Build();
			}
			finally
			{
				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.Method_Out)
						.CommandQueryName(queryType?.FullName)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds));
			}

			return result;
		}

		public IQueryResult<TResult> InterceptExecute(ITraceInfo previousTraceInfo, IQueryHandler<TQuery, TResult> handler, TQuery query, IQueryInterceptorOptions? options)
		{
			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;
			Type? queryType = query?.GetType();
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), previousTraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.Method_In)
						.CommandQueryName(queryType?.FullName));

			if (handler.GetOptions() is not QueryHandlerOptions handlerOptions)
				handlerOptions = new QueryHandlerOptions();

			var resultBuilder = new QueryResultBuilder<TResult>();
			var result = resultBuilder.Build();
			Guid? idQuery = null;
			TContext? context = default;

			try
			{
				var queryHandlerContextBuilder =
					CreateQueryHandlerContext(traceInfo, _loggerFactory.CreateLogger(handler.GetType()))
						.QueryName(queryType?.FullName);

				context = queryHandlerContextBuilder.Context;

				if (query is not QueryBase<TResult> queryBase)
					throw new InvalidOperationException($"Query {queryType?.FullName} must implement {typeof(QueryBase<TResult>).FullName}");

				IQueryLogger? queryEntryLogger = null;
				QueryEntry? queryEntry = null;
				long? startTicks = null;

				if (handlerOptions.LogQueryEntry)
				{
					startTicks = StaticWatch.CurrentTicks;
					queryEntryLogger = ServiceProvider.GetService<IQueryLogger>();
					if (queryEntryLogger == null)
						throw new InvalidOperationException($"{nameof(IQueryLogger)} is not configured");

					queryEntry = new QueryEntry(typeof(TQuery).ToFriendlyFullName(), traceInfo, handlerOptions.SerializeQuery ? queryBase.Serialize() : null);
					queryEntryLogger.WriteQueryEntry(queryEntry);
					idQuery = queryEntry?.IdCommandQueryEntry;
				}

				queryHandlerContextBuilder.IdQueryEntry(idQuery);

				try
				{
					var canExecuteContextBuilder =
						CreateQueryHandlerContext(traceInfo, _loggerFactory.CreateLogger(handler.GetType()))
						.QueryName(queryType?.FullName);

					var canExecuteResult = handler.CanExecute(query, canExecuteContextBuilder.Context);
					if (canExecuteResult == null)
						throw new InvalidOperationException($"Handler {handler.GetType().FullName}.{nameof(handler.CanExecute)} returned null. Expected {typeof(IQueryResult<bool>).FullName}");

					if (!resultBuilder.MergeHasError(canExecuteResult))
					{
						var executeResult = handler.Execute(query, context);
						if (executeResult == null)
							throw new InvalidOperationException($"Handler {handler.GetType().FullName}.{nameof(handler.Execute)} returned null. Expected {typeof(IQueryResult<TResult>).FullName}");

						resultBuilder.CopyAllHasError(executeResult);
					}

					if (result.HasError)
					{
						foreach (var errMsg in result.ErrorMessages)
						{
							if (string.IsNullOrWhiteSpace(errMsg.ClientMessage))
								errMsg.ClientMessage = context.ApplicationResources?.GlobalExceptionMessage;

							if (!errMsg.IdCommandQuery.HasValue)
								errMsg.IdCommandQuery = idQuery;

							_logger.LogErrorMessage(errMsg);
						}

						context.Rollback();
					}
					else
					{
						context.Commit();
					}

					callEndTicks = StaticWatch.CurrentTicks;
					methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);
				}
				catch (Exception executeEx)
				{
					callEndTicks = StaticWatch.CurrentTicks;
					methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

					var hasTrans = context.HasTransaction();
					context.Rollback();

					result = new QueryResultBuilder<TResult>()
						.WithError(traceInfo,
							x => x.ExceptionInfo(executeEx)
									.Detail(hasTrans ? $"Unhandled handler ({handler.GetType().FullName}) exception. DbTransaction.Rollback() succeeded." : $"Unhandled handler ({handler.GetType().FullName}) exception.")
									.IdCommandQuery(idQuery))
						.Build();

					foreach (var errMsg in result.ErrorMessages)
					{
						if (string.IsNullOrWhiteSpace(errMsg.ClientMessage))
							errMsg.ClientMessage = context.ApplicationResources?.GlobalExceptionMessage;

						if (!errMsg.IdCommandQuery.HasValue)
							errMsg.IdCommandQuery = idQuery;

						_logger.LogErrorMessage(errMsg);
					}
				}
				finally
				{
					if (handlerOptions.LogQueryEntry && queryEntryLogger != null && queryEntry != null && startTicks.HasValue)
					{
						long endTicks = StaticWatch.CurrentTicks;
						var elapsedMilliseconds = StaticWatch.ElapsedMilliseconds(startTicks.Value, endTicks);
						queryEntryLogger.WriteQueryExit(queryEntry, elapsedMilliseconds);
					}
				}
			}
			catch (Exception interEx)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				result = new QueryResultBuilder<TResult>()
					.WithError(traceInfo,
						x => x.ExceptionInfo(interEx)
								.Detail($"Unhandled interceptor ({this.GetType().FullName}) exception.")
								.IdCommandQuery(idQuery))
					.Build();

				foreach (var errMsg in result.ErrorMessages)
				{
					if (string.IsNullOrWhiteSpace(errMsg.ClientMessage))
						errMsg.ClientMessage = context?.ApplicationResources?.GlobalExceptionMessage;

					if (!errMsg.IdCommandQuery.HasValue)
						errMsg.IdCommandQuery = idQuery;

					_logger.LogErrorMessage(errMsg);
				}
			}
			finally
			{
				try
				{
					if (context != null)
						context.DisposeTransaction();
				}
				catch { }

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.Method_Out)
						.CommandQueryName(queryType?.FullName)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds));
			}

			return result;
		}
	}
}
