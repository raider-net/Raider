using Raider.Trace;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Commands.Aspects
{
	public interface ICommandInterceptor { }

	public interface ICommandInterceptor<TCommand> : ICommandInterceptor
		where TCommand : ICommand
	{
		ICommandResult<bool> InterceptCanExecute(ITraceInfo previousTraceInfo, ICommandHandler<TCommand> handler, TCommand command, ICommandInterceptorOptions? options);
		ICommandResult InterceptExecute(ITraceInfo previousTraceInfo, ICommandHandler<TCommand> handler, TCommand command, ICommandInterceptorOptions? options);
	}

	public interface IAsyncCommandInterceptor<TCommand> : ICommandInterceptor
		where TCommand : ICommand
	{
		Task<ICommandResult<bool>> InterceptCanExecuteAsync(ITraceInfo previousTraceInfo, IAsyncCommandHandler<TCommand> handler, TCommand command, ICommandInterceptorOptions? options, CancellationToken cancellationToken);
		Task<ICommandResult> InterceptExecuteAsync(ITraceInfo previousTraceInfo, IAsyncCommandHandler<TCommand> handler, TCommand command, ICommandInterceptorOptions? options, CancellationToken cancellationToken);
	}

	public interface ICommandInterceptor<TCommand, TResult> : ICommandInterceptor
		where TCommand : ICommand<TResult>
	{
		ICommandResult<bool> InterceptCanExecute(ITraceInfo previousTraceInfo, ICommandHandler<TCommand, TResult> handler, TCommand command, ICommandInterceptorOptions? options);
		ICommandResult<TResult> InterceptExecute(ITraceInfo previousTraceInfo, ICommandHandler<TCommand, TResult> handler, TCommand command, ICommandInterceptorOptions? options);
	}

	public interface IAsyncCommandInterceptor<TCommand, TResult> : ICommandInterceptor
		where TCommand : ICommand<TResult>
	{
		Task<ICommandResult<bool>> InterceptCanExecuteAsync(ITraceInfo previousTraceInfo, IAsyncCommandHandler<TCommand, TResult> handler, TCommand command, ICommandInterceptorOptions? options, CancellationToken cancellationToken);
		Task<ICommandResult<TResult>> InterceptExecuteAsync(ITraceInfo previousTraceInfo, IAsyncCommandHandler<TCommand, TResult> handler, TCommand command, ICommandInterceptorOptions? options, CancellationToken cancellationToken);
	}
}
