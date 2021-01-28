namespace Raider.Queries
{
	public interface IQueryHandlerFactory
	{
		IQueryHandler<TQuery, TResult>? CreateQueryHandler<TQuery, TResult>()
			where TQuery : IQuery<TResult>;

		IAsyncQueryHandler<TQuery, TResult>? CreateAsyncQueryHandler<TQuery, TResult>()
			where TQuery : IQuery<TResult>;

		void Release(IQueryHandler? handler);
	}
}
