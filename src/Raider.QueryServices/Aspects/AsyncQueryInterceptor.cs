using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Raider.Queries;
using Raider.Queries.Aspects;
using Raider.DependencyInjection;
using Raider.Diagnostics;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.QueryServices.Queries;
using Raider.Trace;
using System;
using System.Threading;
using System.Threading.Tasks;
using Raider.Extensions;

namespace Raider.QueryServices.Aspects
{
	public class AsyncQueryInterceptor<TQuery, TResult, TContext, TBuilder> : InterceptorBase<TContext, TBuilder>, IAsyncQueryInterceptor<TQuery, TResult>
		where TQuery : IQuery<TResult>
		where TContext : QueryHandlerContext
		where TBuilder : QueryHandlerContext.Builder<TContext>
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _logger;

		public AsyncQueryInterceptor(ServiceFactory serviceFactory, ILoggerFactory loggerFactory, ILogger<AsyncQueryInterceptor<TQuery, TResult, TContext, TBuilder>> logger)
			: base(serviceFactory)
		{
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<IQueryResult<bool>> InterceptCanExecuteAsync(ITraceInfo previousTraceInfo, IAsyncQueryHandler<TQuery, TResult> handler, TQuery query, IQueryInterceptorOptions? options, CancellationToken cancellationToken)
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
					var handlerResult = await handler.CanExecuteAsync(query, queryHandlerContextBuilder.Context, cancellationToken);
					if (handlerResult == null)
						throw new InvalidOperationException($"Handler {handler.GetType().FullName}.{nameof(handler.CanExecuteAsync)} returned null. Expected {typeof(IQueryResult<TResult>).FullName}");

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

		public async Task<IQueryResult<TResult>> InterceptExecuteAsync(ITraceInfo previousTraceInfo, IAsyncQueryHandler<TQuery, TResult> handler, TQuery query, IQueryInterceptorOptions? options, CancellationToken cancellationToken)
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
			IDbContextTransaction? tran = null;

			try
			{
				var queryHandlerContextBuilder =
					CreateQueryHandlerContext(traceInfo, _loggerFactory.CreateLogger(handler.GetType()))
						.QueryName(queryType?.FullName);

				if (query is not QueryBase<TResult> queryBase)
					throw new InvalidOperationException($"Query {queryType?.FullName} must implement {typeof(QueryBase<TResult>).FullName}");

				IQueryLogger? queryEntryLogger = null;
				QueryEntry? queryEntry = null;
				long? startTicks = null;

				if (handlerOptions.LogQueryEntry)
				{
					startTicks = StaticWatch.CurrentTicks;
					queryEntryLogger = ServiceFactory.GetInstance<IQueryLogger>();
					if (queryEntryLogger == null)
						throw new InvalidOperationException($"{nameof(IQueryLogger)} is not configured");

					queryEntry = new QueryEntry(typeof(TQuery).ToFriendlyFullName(), traceInfo, handlerOptions.SerializeQuery ? queryBase.Serialize() : null);
					queryEntryLogger.WriteQueryEntry(queryEntry);
					idQuery = queryEntry?.IdCommandQueryEntry;
				}

				queryHandlerContextBuilder.IdQueryEntry(idQuery);

				//TODO ako nastavit transaction id, ked transakcia ani dbcontext este nevznikli
				//traceInfoBuilder
				//	.TransactionId(tran?.TransactionId.ToString());

				try
				{
					var canExecuteContextBuilder =
						CreateQueryHandlerContext(traceInfo, _loggerFactory.CreateLogger(handler.GetType()))
							.QueryName(queryType?.FullName);

					var canExecuteResult = await handler.CanExecuteAsync(query, canExecuteContextBuilder.Context, cancellationToken);
					if (canExecuteResult == null)
						throw new InvalidOperationException($"Handler {handler.GetType().FullName}.{nameof(handler.CanExecuteAsync)} returned null. Expected {typeof(IQueryResult<bool>).FullName}");

					if (!resultBuilder.MergeHasError(canExecuteResult))
					{
						var executeResult = await handler.ExecuteAsync(query, queryHandlerContextBuilder.Context, cancellationToken);
						if (executeResult == null)
							throw new InvalidOperationException($"Handler {handler.GetType().FullName}.{nameof(handler.ExecuteAsync)} returned null. Expected {typeof(IQueryResult<TResult>).FullName}");

						//TODO kto zaloguje reuslt message a kto tam prida ComamndName a IdSavedCommandu ???
						resultBuilder.CopyAllHasError(executeResult);
					}

					tran = queryHandlerContextBuilder.Context.DbContextTransaction;

					if (result.HasError)
					{
						if (tran != null)
						{
							tran.Rollback();
						}
					}
					else
					{
						if (tran != null)
						{
							tran.Commit();
						}
					}

					callEndTicks = StaticWatch.CurrentTicks;
					methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);
				}
				catch (Exception executeEx)
				{
					callEndTicks = StaticWatch.CurrentTicks;
					methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

					if (tran != null)
					{
						tran.Rollback();
					}

					result = new QueryResultBuilder<TResult>()
						.WithError(traceInfo,
							x => x.ExceptionInfo(executeEx)
									.Detail(tran == null ? $"Unhandled handler ({handler.GetType().FullName}) exception." : $"Unhandled handler ({handler.GetType().FullName}) exception. DbTransaction.Rollback() succeeded.")
									.IdCommandQuery(idQuery))
						.Build();
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
			}
			finally
			{
				try
				{
					if (tran != null)
					{
						await tran.DisposeAsync();
					}
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
