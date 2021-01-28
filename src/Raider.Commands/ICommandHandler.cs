using Raider.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Commands
{
	public interface ICommandHandler
	{
		ICommandDispatcher Dispatcher { get; set; }
		ServiceFactory ServiceFactory { get; set; }
		Type? InterceptorType { get; }

		ICommandHandlerOptions? GetOptions();
	}

	public interface ICommandHandler<TCommand> : ICommandHandler
		where TCommand : ICommand
	{
		ICommandResult<bool> CanExecute(TCommand command, ICommandHandlerContext? context);

		ICommandResult Execute(TCommand command, ICommandHandlerContext? context);
	}

	public interface IAsyncCommandHandler<TCommand> : ICommandHandler
		where TCommand : ICommand
	{
		Task<ICommandResult<bool>> CanExecuteAsync(TCommand command, ICommandHandlerContext? context, CancellationToken cancellationToken);

		Task<ICommandResult> ExecuteAsync(TCommand command, ICommandHandlerContext? context, CancellationToken cancellationToken);
	}

	public interface ICommandHandler<TCommand, TResult> : ICommandHandler
		where TCommand : ICommand<TResult>
	{
		ICommandResult<bool> CanExecute(TCommand command, ICommandHandlerContext? context);

		ICommandResult<TResult> Execute(TCommand command, ICommandHandlerContext? context);
	}

	public interface IAsyncCommandHandler<TCommand, TResult> : ICommandHandler
		where TCommand : ICommand<TResult>
	{
		Task<ICommandResult<bool>> CanExecuteAsync(TCommand command, ICommandHandlerContext? context, CancellationToken cancellationToken);

		Task<ICommandResult<TResult>> ExecuteAsync(TCommand command, ICommandHandlerContext? context, CancellationToken cancellationToken);
	}
}
