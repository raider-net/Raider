using Raider.Queries.Aspects;
using Raider.DependencyInjection;
using Raider.Exceptions;
using Raider.Trace;
using System;

namespace Raider.Queries.Internal
{
	internal abstract class QueryProcessor<TResult> : QueryProcessorBase
	{
		public QueryProcessor(ServiceFactory serviceFactory)
			: base(serviceFactory)
		{
		}

		public abstract IQueryResult<bool> CanExecute(
			ITraceInfo traceInfo,
			IQueryHandler handler,
			IQuery<TResult> query,
			IQueryInterceptorOptions? options);

		public abstract IQueryResult<TResult> Execute(
			ITraceInfo traceInfo,
			IQueryHandler handler,
			IQuery<TResult> query,
			IQueryInterceptorOptions? options);
	}

	internal class QueryProcessor<TQuery, TResult> : QueryProcessor<TResult>
		where TQuery : IQuery<TResult>
	{
		private readonly IQueryHandlerRegistry _handlerRegistry;
		private readonly IQueryHandlerFactory _handlerFactory;

		public QueryProcessor(
			IQueryHandlerRegistry handlerRegistry,
			IQueryHandlerFactory handlerFactory,
			ServiceFactory serviceFactory)
			: base(serviceFactory)
		{
			_handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
			_handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));

			var _handlerType = _handlerRegistry.GetQueryHandler<TQuery, TResult>();
			if (_handlerType == null)
				throw new ConfigurationException($"No synchronous handler registered for query: {typeof(TQuery).FullName}");
		}

		public override IQueryHandler CreateHandler()
		{
			var handler = _handlerFactory.CreateQueryHandler<TQuery, TResult>();
			if (handler == null)
				throw new InvalidOperationException($"Handler could not be created for type: {typeof(IQueryHandler<TQuery, TResult>).FullName}");

			return handler;
		}

		public override IQueryResult<bool> CanExecute(
			ITraceInfo traceInfo,
			IQueryHandler handler,
			IQuery<TResult> query,
			IQueryInterceptorOptions? options)
		{
			var hnd = (IQueryHandler<TQuery, TResult>)handler;

			IQueryInterceptor<TQuery, TResult>? interceptor = null;
			if (hnd.InterceptorType != null)
			{
				if (!typeof(IQueryInterceptor<TQuery, TResult>).IsAssignableFrom(hnd.InterceptorType))
					throw new InvalidOperationException($"Handler {hnd.GetType().FullName} has invalid {nameof(hnd.InterceptorType)}. {hnd.InterceptorType.FullName} must implement {typeof(IQueryInterceptor<TQuery, TResult>).FullName}");

				interceptor = (IQueryInterceptor<TQuery, TResult>?)hnd.ServiceFactory.GetRequiredInstance(hnd.InterceptorType);
			}

			return interceptor == null
				? hnd.CanExecute((TQuery)query, CreateQueryHandlerContext(traceInfo))
				: interceptor.InterceptCanExecute(traceInfo, hnd, (TQuery)query, options);
		}

		public override IQueryResult<TResult> Execute(
			ITraceInfo traceInfo,
			IQueryHandler handler,
			IQuery<TResult> query,
			IQueryInterceptorOptions? options)
		{
			var hnd = (IQueryHandler<TQuery, TResult>)handler;

			IQueryInterceptor<TQuery, TResult>? interceptor = null;
			if (hnd.InterceptorType != null)
			{
				if (!typeof(IQueryInterceptor<TQuery, TResult>).IsAssignableFrom(hnd.InterceptorType))
					throw new InvalidOperationException($"Handler {hnd.GetType().FullName} has invalid {nameof(hnd.InterceptorType)}. {hnd.InterceptorType.FullName} must implement {typeof(IQueryInterceptor<TQuery, TResult>).FullName}");

				interceptor = (IQueryInterceptor<TQuery, TResult>?)hnd.ServiceFactory.GetRequiredInstance(hnd.InterceptorType);
			}

			return interceptor == null
				? hnd.Execute((TQuery)query, CreateQueryHandlerContext(traceInfo))
				: interceptor.InterceptExecute(traceInfo, hnd, (TQuery)query, options);
		}

		public override void DisposeHandler(IQueryHandler? handler)
		{
			if (handler != null)
				_handlerFactory.Release(handler);
		}
	}
}
