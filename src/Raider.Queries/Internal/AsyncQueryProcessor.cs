using Raider.Queries.Aspects;
using Raider.DependencyInjection;
using Raider.Exceptions;
using Raider.Trace;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Queries.Internal
{
	internal abstract class AsyncQueryProcessor<TResult> : QueryProcessorBase
	{
		public AsyncQueryProcessor(ServiceFactory serviceFactory)
			: base(serviceFactory)
		{
		}

		public abstract Task<IQueryResult<bool>> CanExecuteAsync(
			ITraceInfo traceInfo,
			IQueryHandler handler,
			IQuery<TResult> query,
			IQueryInterceptorOptions? options,
			CancellationToken cancellationToken);

		public abstract Task<IQueryResult<TResult>> ExecuteAsync(
			ITraceInfo traceInfo,
			IQueryHandler handler,
			IQuery<TResult> query,
			IQueryInterceptorOptions? options,
			CancellationToken cancellationToken);
	}

	internal class AsyncQueryProcessor<TQuery, TResult> : AsyncQueryProcessor<TResult>
		where TQuery : IQuery<TResult>
	{
		private readonly IQueryHandlerRegistry _handlerRegistry;
		private readonly IQueryHandlerFactory _handlerFactory;

		public AsyncQueryProcessor(
			IQueryHandlerRegistry handlerRegistry,
			IQueryHandlerFactory handlerFactory,
			ServiceFactory serviceFactory)
			: base(serviceFactory)
		{
			_handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
			_handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));

			var _handlerType = _handlerRegistry.GetAsyncQueryHandler<TQuery, TResult>();
			if (_handlerType == null)
				throw new ConfigurationException($"No asynchronous handler registered for query: {typeof(TQuery).FullName}");
		}

		public override IQueryHandler CreateHandler()
		{
			var handler = _handlerFactory.CreateAsyncQueryHandler<TQuery, TResult>();
			if (handler == null)
				throw new InvalidOperationException($"Handler could not be created for type: {typeof(IAsyncQueryHandler<TQuery, TResult>).FullName}");

			return handler;
		}

		public override Task<IQueryResult<bool>> CanExecuteAsync(
			ITraceInfo traceInfo,
			IQueryHandler handler,
			IQuery<TResult> query,
			IQueryInterceptorOptions? options,
			CancellationToken cancellationToken)
		{
			var hnd = (IAsyncQueryHandler<TQuery, TResult>)handler;

			IAsyncQueryInterceptor<TQuery, TResult>? interceptor = null;
			if (hnd.InterceptorType != null)
			{
				if (!typeof(IAsyncQueryInterceptor<TQuery, TResult>).IsAssignableFrom(hnd.InterceptorType))
					throw new InvalidOperationException($"Handler {hnd.GetType().FullName} has invalid {nameof(hnd.InterceptorType)}. {hnd.InterceptorType.FullName} must implement {typeof(IAsyncQueryInterceptor<TQuery, TResult>).FullName}");

				interceptor = (IAsyncQueryInterceptor<TQuery, TResult>?)hnd.ServiceFactory.GetRequiredInstance(hnd.InterceptorType);
			}

			return interceptor == null
				? hnd.CanExecuteAsync((TQuery)query, CreateQueryHandlerContext(traceInfo), cancellationToken)
				: interceptor.InterceptCanExecuteAsync(traceInfo, hnd, (TQuery)query, options, cancellationToken);
		}

		public override Task<IQueryResult<TResult>> ExecuteAsync(
			ITraceInfo traceInfo,
			IQueryHandler handler,
			IQuery<TResult> query,
			IQueryInterceptorOptions? options,
			CancellationToken cancellationToken)
		{
			var hnd = (IAsyncQueryHandler<TQuery, TResult>)handler;

			IAsyncQueryInterceptor<TQuery, TResult>? interceptor = null;
			if (hnd.InterceptorType != null)
			{
				if (!typeof(IAsyncQueryInterceptor<TQuery, TResult>).IsAssignableFrom(hnd.InterceptorType))
					throw new InvalidOperationException($"Handler {hnd.GetType().FullName} has invalid {nameof(hnd.InterceptorType)}. {hnd.InterceptorType.FullName} must implement {typeof(IAsyncQueryInterceptor<TQuery, TResult>).FullName}");

				interceptor = (IAsyncQueryInterceptor<TQuery, TResult>?)hnd.ServiceFactory.GetRequiredInstance(hnd.InterceptorType);
			}

			return interceptor == null
				? hnd.ExecuteAsync((TQuery)query, CreateQueryHandlerContext(traceInfo), cancellationToken)
				: interceptor.InterceptExecuteAsync(traceInfo, hnd, (TQuery)query, options, cancellationToken);
		}

		public override void DisposeHandler(IQueryHandler? handler)
		{
			if (handler != null)
				_handlerFactory.Release(handler);
		}
	}
}
