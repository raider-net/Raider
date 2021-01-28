using System;

namespace Raider.Queries
{
	public interface IQueryHandlerRegistry
	{
		bool TryRegisterHandler(Type handlerType);

		Type? GetQueryHandler<TQuery, TResult>()
			where TQuery : IQuery<TResult>;

		Type? GetQueryHandler(Type queryType);

		void RegisterQueryHandler<THandler>();

		void RegisterQueryHandler(Type handlerType);

		void RegisterQueryHandler<TQuery, TResult, THandler>()
			where TQuery : IQuery<TResult>
			where THandler : IQueryHandler<TQuery, TResult>;

		void RegisterQueryHandler(Type queryType, Type resultType, Type handlerType);

		Type? GetAsyncQueryHandler<TQuery, TResult>()
			where TQuery : IQuery<TResult>;

		Type? GetAsyncQueryHandler(Type queryType);

		void RegisterAsyncQueryHandler<THandler>();

		void RegisterAsyncQueryHandler(Type handlerType);

		void RegisterAsyncQueryHandler<TQuery, TResult, THandler>()
			where TQuery : IQuery<TResult>
			where THandler : IAsyncQueryHandler<TQuery, TResult>;

		void RegisterAsyncQueryHandler(Type queryType, Type resultType, Type handlerType);
	}
}
