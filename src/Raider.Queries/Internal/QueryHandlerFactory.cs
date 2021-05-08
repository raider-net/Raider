using Microsoft.Extensions.DependencyInjection;
using System;

namespace Raider.Queries.Internal
{
	public class QueryHandlerFactory : IQueryHandlerFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public QueryHandlerFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public IQueryHandler<TQuery, TResult>? CreateQueryHandler<TQuery, TResult>()
			where TQuery : IQuery<TResult>
		{
			var handler = _serviceProvider.GetService<IQueryHandler<TQuery, TResult>>();
			if (handler != null)
				handler.ServiceProvider = _serviceProvider;

			return handler;
		}

		public IAsyncQueryHandler<TQuery, TResult>? CreateAsyncQueryHandler<TQuery, TResult>()
			where TQuery : IQuery<TResult>
		{
			var handler = _serviceProvider.GetService<IAsyncQueryHandler<TQuery, TResult>>();
			if (handler != null)
				handler.ServiceProvider = _serviceProvider;

			return handler;
		}

		public void Release(IQueryHandler? handler)
		{
			var disposal = handler as IDisposable;
			disposal?.Dispose();
		}
	}
}
