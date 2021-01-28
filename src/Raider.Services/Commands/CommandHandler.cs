using Raider.Commands;
using Raider.DependencyInjection;
using Raider.Services.Aspects;
using System;

namespace Raider.Services.Commands
{
	public abstract class CommandHandler<TCommand, TResult, TContext, TBuilder> : ICommandHandler<TCommand, TResult>, IDisposable
			where TCommand : ICommand<TResult>
			where TContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<TContext>
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public ICommandDispatcher Dispatcher { get; set; }
		public ServiceFactory ServiceFactory { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public Type? InterceptorType { get; } = typeof(CommandInterceptor<TCommand, TResult, TContext, TBuilder>);
		
		public abstract ICommandResult<bool> CanExecute(TCommand command, TContext context);
		public abstract ICommandResult<TResult> Execute(TCommand command, TContext context);

		public virtual void Dispose()
		{
		}

#pragma warning disable CS8604 // Possible null reference argument.
		ICommandResult<bool> ICommandHandler<TCommand, TResult>.CanExecute(TCommand command, ICommandHandlerContext? context)
			=> CanExecute(command, context as TContext);

		ICommandResult<TResult> ICommandHandler<TCommand, TResult>.Execute(TCommand command, ICommandHandlerContext? context)
			=> Execute(command, context as TContext);
#pragma warning restore CS8604 // Possible null reference argument.

		ICommandHandlerOptions? ICommandHandler.GetOptions()
			=> GetOptions();

		public virtual CommandHandlerOptions? GetOptions() => null;
	}
}
