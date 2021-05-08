using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Queries
{
	public interface IQueryHandler
	{
		IQueryDispatcher Dispatcher { get; set; }
		IServiceProvider ServiceProvider { get; set; }
		Type? InterceptorType { get; }

		IQueryHandlerOptions? GetOptions();
	}

	public interface IQueryHandler<TQuery, TResult> : IQueryHandler
		where TQuery : IQuery<TResult>
	{
		IQueryResult<bool> CanExecute(TQuery query, IQueryHandlerContext? context);

		IQueryResult<TResult> Execute(TQuery query, IQueryHandlerContext? context);
	}

	public interface IAsyncQueryHandler<TQuery, TResult> : IQueryHandler
		where TQuery : IQuery<TResult>
	{
		Task<IQueryResult<bool>> CanExecuteAsync(TQuery query, IQueryHandlerContext? context, CancellationToken cancellationToken);

		Task<IQueryResult<TResult>> ExecuteAsync(TQuery query, IQueryHandlerContext? context, CancellationToken cancellationToken);
	}
}
