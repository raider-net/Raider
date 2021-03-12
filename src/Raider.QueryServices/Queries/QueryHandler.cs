using Raider.Queries;
using Raider.DependencyInjection;
using Raider.QueryServices.Aspects;
using System;

namespace Raider.QueryServices.Queries
{
	public abstract class QueryHandler<TQuery, TResult, TContext, TBuilder> : IQueryHandler<TQuery, TResult>, IDisposable
			where TQuery : IQuery<TResult>
			where TContext : QueryHandlerContext
			where TBuilder : QueryHandlerContext.Builder<TContext>
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public IQueryDispatcher Dispatcher { get; set; }
		public ServiceFactory ServiceFactory { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public Type? InterceptorType { get; } = typeof(QueryInterceptor<TQuery, TResult, TContext, TBuilder>);
		
		public abstract IQueryResult<bool> CanExecute(TQuery query, TContext context);
		public abstract IQueryResult<TResult> Execute(TQuery query, TContext context);

		protected virtual void Dispose(bool disposing)
		{
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

#pragma warning disable CS8604 // Possible null reference argument.
		IQueryResult<bool> IQueryHandler<TQuery, TResult>.CanExecute(TQuery query, IQueryHandlerContext? context)
			=> CanExecute(query, context as TContext);

		IQueryResult<TResult> IQueryHandler<TQuery, TResult>.Execute(TQuery query, IQueryHandlerContext? context)
			=> Execute(query, context as TContext);
#pragma warning restore CS8604 // Possible null reference argument.

		IQueryHandlerOptions? IQueryHandler.GetOptions()
			=> GetOptions();

		public virtual QueryHandlerOptions? GetOptions() => null;
	}
}
