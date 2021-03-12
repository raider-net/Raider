using Raider.Commands;
using Raider.DependencyInjection;
using Raider.Services.Aspects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Services.Commands
{
	public abstract class AsyncCommandHandler<TCommand, TContext, TBuilder> : IAsyncCommandHandler<TCommand>, IDisposable
			where TCommand : ICommand
			where TContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<TContext>
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public ICommandDispatcher Dispatcher { get; set; }
		public ServiceFactory ServiceFactory { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public Type? InterceptorType { get; } = typeof(AsyncCommandInterceptor<TCommand, TContext, TBuilder>);
		
		public abstract Task<ICommandResult<bool>> CanExecuteAsync(TCommand command, TContext context, CancellationToken cancellationToken);
		public abstract Task<ICommandResult> ExecuteAsync(TCommand command, TContext context, CancellationToken cancellationToken);

		protected virtual void Dispose(bool disposing)
		{
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

#pragma warning disable CS8604 // Possible null reference argument.
		Task<ICommandResult<bool>> IAsyncCommandHandler<TCommand>.CanExecuteAsync(TCommand command, ICommandHandlerContext? context, CancellationToken cancellationToken)
			=> CanExecuteAsync(command, context as TContext, cancellationToken);

		Task<ICommandResult> IAsyncCommandHandler<TCommand>.ExecuteAsync(TCommand command, ICommandHandlerContext? context, CancellationToken cancellationToken)
			=> ExecuteAsync(command, context as TContext, cancellationToken);
#pragma warning restore CS8604 // Possible null reference argument.

		ICommandHandlerOptions? ICommandHandler.GetOptions()
			=> GetOptions();

		public virtual CommandHandlerOptions? GetOptions() => null;
	}
}
