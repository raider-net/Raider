namespace Raider.Commands
{
	public interface ICommandHandlerFactory
	{
		ICommandHandler<TCommand>? CreateVoidCommandHandler<TCommand>()
			where TCommand : ICommand;

		IAsyncCommandHandler<TCommand>? CreateAsyncVoidCommandHandler<TCommand>()
			where TCommand : ICommand;

		ICommandHandler<TCommand, TResult>? CreateCommandHandler<TCommand, TResult>()
			where TCommand : ICommand<TResult>;

		IAsyncCommandHandler<TCommand, TResult>? CreateAsyncCommandHandler<TCommand, TResult>()
			where TCommand : ICommand<TResult>;

		void Release(ICommandHandler? handler);
	}
}
