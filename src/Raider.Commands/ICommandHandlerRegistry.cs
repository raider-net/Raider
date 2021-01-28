using System;

namespace Raider.Commands
{
	public interface ICommandHandlerRegistry
	{
		bool TryRegisterHandler(Type handlerType);

		Type? GetVoidCommandHandler<TCommand>()
			where TCommand : ICommand;

		Type? GetVoidCommandHandler(Type commandType);

		void RegisterVoidCommandHandler<THandler>();

		void RegisterVoidCommandHandler(Type handlerType);

		void RegisterVoidCommandHandler<TCommand, THandler>()
			where TCommand : ICommand
			where THandler : ICommandHandler<TCommand>;

		void RegisterVoidCommandHandler(Type commandType, Type handlerType);

		Type? GetAsyncVoidCommandHandler<TCommand>()
			where TCommand : ICommand;

		Type? GetAsyncVoidCommandHandler(Type commandType);

		void RegisterAsyncVoidCommandHandler<THandler>();

		void RegisterAsyncVoidCommandHandler(Type handlerType);

		void RegisterAsyncVoidCommandHandler<TCommand, THandler>()
			where TCommand : ICommand
			where THandler : IAsyncCommandHandler<TCommand>;

		void RegisterAsyncVoidCommandHandler(Type commandType, Type handlerType);

		Type? GetCommandHandler<TCommand, TResult>()
			where TCommand : ICommand<TResult>;

		Type? GetCommandHandler(Type commandType);

		void RegisterCommandHandler<THandler>();

		void RegisterCommandHandler(Type handlerType);

		void RegisterCommandHandler<TCommand, TResult, THandler>()
			where TCommand : ICommand<TResult>
			where THandler : ICommandHandler<TCommand, TResult>;

		void RegisterCommandHandler(Type commandType, Type resultType, Type handlerType);

		Type? GetAsyncCommandHandler<TCommand, TResult>()
			where TCommand : ICommand<TResult>;

		Type? GetAsyncCommandHandler(Type commandType);

		void RegisterAsyncCommandHandler<THandler>();

		void RegisterAsyncCommandHandler(Type handlerType);

		void RegisterAsyncCommandHandler<TCommand, TResult, THandler>()
			where TCommand : ICommand<TResult>
			where THandler : IAsyncCommandHandler<TCommand, TResult>;

		void RegisterAsyncCommandHandler(Type commandType, Type resultType, Type handlerType);
	}
}
