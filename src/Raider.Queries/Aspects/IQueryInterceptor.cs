using Raider.Trace;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Queries.Aspects
{
	public interface IQueryInterceptor { }

	public interface IQueryInterceptor<TQuery, TResult> : IQueryInterceptor
		where TQuery : IQuery<TResult>
	{
		IQueryResult<bool> InterceptCanExecute(ITraceInfo previousTraceInfo, IQueryHandler<TQuery, TResult> handler, TQuery query, IQueryInterceptorOptions? options);
		IQueryResult<TResult> InterceptExecute(ITraceInfo previousTraceInfo, IQueryHandler<TQuery, TResult> handler, TQuery query, IQueryInterceptorOptions? options);
	}

	public interface IAsyncQueryInterceptor<TQuery, TResult> : IQueryInterceptor
		where TQuery : IQuery<TResult>
	{
		Task<IQueryResult<bool>> InterceptCanExecuteAsync(ITraceInfo previousTraceInfo, IAsyncQueryHandler<TQuery, TResult> handler, TQuery query, IQueryInterceptorOptions? options, CancellationToken cancellationToken);
		Task<IQueryResult<TResult>> InterceptExecuteAsync(ITraceInfo previousTraceInfo, IAsyncQueryHandler<TQuery, TResult> handler, TQuery query, IQueryInterceptorOptions? options, CancellationToken cancellationToken);
	}
}
