using Raider.Queries;
using Raider.DependencyInjection;
using Raider.QueryServices.Aspects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.QueryServices.Queries
{
	public abstract class AsyncQueryHandler<TQuery, TResult, TContext, TBuilder> : IAsyncQueryHandler<TQuery, TResult>, IDisposable
			where TQuery : IQuery<TResult>
			where TContext : QueryHandlerContext
			where TBuilder : QueryHandlerContext.Builder<TContext>
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public IQueryDispatcher Dispatcher { get; set; }
		public ServiceFactory ServiceFactory { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public Type? InterceptorType { get; } = typeof(AsyncQueryInterceptor<TQuery, TResult, TContext, TBuilder>);
		
		public abstract Task<IQueryResult<bool>> CanExecuteAsync(TQuery query, TContext context, CancellationToken cancellationToken);
		public abstract Task<IQueryResult<TResult>> ExecuteAsync(TQuery query, TContext context, CancellationToken cancellationToken);

		protected virtual void Dispose(bool disposing)
		{
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

#pragma warning disable CS8604 // Possible null reference argument.
		Task<IQueryResult<bool>> IAsyncQueryHandler<TQuery, TResult>.CanExecuteAsync(TQuery query, IQueryHandlerContext? context, CancellationToken cancellationToken)
			=> CanExecuteAsync(query, context as TContext, cancellationToken);

		Task<IQueryResult<TResult>> IAsyncQueryHandler<TQuery, TResult>.ExecuteAsync(TQuery query, IQueryHandlerContext? context, CancellationToken cancellationToken)
			=> ExecuteAsync(query, context as TContext, cancellationToken);
#pragma warning restore CS8604 // Possible null reference argument.

		IQueryHandlerOptions? IQueryHandler.GetOptions()
			=> GetOptions();

		public virtual QueryHandlerOptions? GetOptions() => null;
	}
}
