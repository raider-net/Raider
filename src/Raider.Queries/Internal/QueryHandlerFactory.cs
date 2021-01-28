using Raider.DependencyInjection;
using System;

namespace Raider.Queries.Internal
{
	public class QueryHandlerFactory : IQueryHandlerFactory
	{
		private readonly ServiceFactory _serviceFactory;

		public QueryHandlerFactory(ServiceFactory serviceFactory)
		{
			_serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
		}

		public IQueryHandler<TQuery, TResult>? CreateQueryHandler<TQuery, TResult>()
			where TQuery : IQuery<TResult>
		{
			var handler = _serviceFactory.GetInstance<IQueryHandler<TQuery, TResult>>();
			if (handler != null)
				handler.ServiceFactory = _serviceFactory.GetRequiredInstance<ServiceFactory>();

			return handler;
		}

		public IAsyncQueryHandler<TQuery, TResult>? CreateAsyncQueryHandler<TQuery, TResult>()
			where TQuery : IQuery<TResult>
		{
			var handler = _serviceFactory.GetInstance<IAsyncQueryHandler<TQuery, TResult>>();
			if (handler != null)
				handler.ServiceFactory = _serviceFactory.GetRequiredInstance<ServiceFactory>();

			return handler;
		}

		public void Release(IQueryHandler? handler)
		{
			var disposal = handler as IDisposable;
			disposal?.Dispose();
		}
	}
}
