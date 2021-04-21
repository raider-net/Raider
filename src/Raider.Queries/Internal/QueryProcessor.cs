using Raider.DependencyInjection;
using Raider.Exceptions;
using Raider.Localization;
using Raider.Queries.Aspects;
using Raider.Trace;
using System;

namespace Raider.Queries.Internal
{
	internal abstract class QueryProcessor<TResult> : QueryProcessorBase
	{
		public QueryProcessor()
			: base()
		{
		}

		public abstract IQueryResult<bool> CanExecute(
			ITraceInfo traceInfo,
			IQueryHandler handler,
			IQuery<TResult> query,
			IQueryInterceptorOptions? options,
			IApplicationContext applicationContext);

		public abstract IQueryResult<TResult> Execute(
			ITraceInfo traceInfo,
			IQueryHandler handler,
			IQuery<TResult> query,
			IQueryInterceptorOptions? options,
			IApplicationContext applicationContext);
	}

	internal class QueryProcessor<TQuery, TResult> : QueryProcessor<TResult>
		where TQuery : IQuery<TResult>
	{
		private readonly IQueryHandlerRegistry _handlerRegistry;

		public QueryProcessor(
			IQueryHandlerRegistry handlerRegistry)
			: base()
		{
			_handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));

			var _handlerType = _handlerRegistry.GetQueryHandler<TQuery, TResult>();
			if (_handlerType == null)
				throw new ConfigurationException($"No synchronous handler registered for query: {typeof(TQuery).FullName}");
		}

		public override IQueryHandler CreateHandler(IQueryHandlerFactory handlerFactory)
		{
			if (handlerFactory == null)
				throw new ArgumentNullException(nameof(handlerFactory));

			var handler = handlerFactory.CreateQueryHandler<TQuery, TResult>();
			if (handler == null)
				throw new InvalidOperationException($"Handler could not be created for type: {typeof(IQueryHandler<TQuery, TResult>).FullName}");

			return handler;
		}

		public override IQueryResult<bool> CanExecute(
			ITraceInfo traceInfo,
			IQueryHandler handler,
			IQuery<TResult> query,
			IQueryInterceptorOptions? options,
			IApplicationContext applicationContext)
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
				? hnd.CanExecute((TQuery)query, CreateQueryHandlerContext(traceInfo, applicationContext))
				: interceptor.InterceptCanExecute(traceInfo, hnd, (TQuery)query, options);
		}

		public override IQueryResult<TResult> Execute(
			ITraceInfo traceInfo,
			IQueryHandler handler,
			IQuery<TResult> query,
			IQueryInterceptorOptions? options,
			IApplicationContext applicationContext)
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
				? hnd.Execute((TQuery)query, CreateQueryHandlerContext(traceInfo, applicationContext))
				: interceptor.InterceptExecute(traceInfo, hnd, (TQuery)query, options);
		}

		public override void DisposeHandler(IQueryHandlerFactory handlerFactory, IQueryHandler? handler)
		{
			if (handler != null)
			{
				if (handlerFactory == null)
					throw new ArgumentNullException(nameof(handlerFactory));

				handlerFactory.Release(handler);
			}
		}
	}
}
