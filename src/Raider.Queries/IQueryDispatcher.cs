using Raider.Queries.Aspects;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Queries
{
	public interface IQueryDispatcher
	{
		IQueryResult<bool> CanExecute<TResult>(IQuery<TResult> query, IQueryInterceptorOptions? options = null);

		Task<IQueryResult<bool>> CanExecuteAsync<TResult>(IQuery<TResult> query, IQueryInterceptorOptions? options = null, CancellationToken cancellationToken = default);

		IQueryResult<TResult> Execute<TResult>(IQuery<TResult> query, IQueryInterceptorOptions? options = null);

		Task<IQueryResult<TResult>> ExecuteAsync<TResult>(IQuery<TResult> query, IQueryInterceptorOptions? options = null, CancellationToken cancellationToken = default);
	}
}
