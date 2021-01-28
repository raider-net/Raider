using Raider.Commands.Aspects;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Commands
{
	public interface ICommandDispatcher
	{
		ICommandResult<bool> CanExecute(ICommand command, ICommandInterceptorOptions? options = null);

		Task<ICommandResult<bool>> CanExecuteAsync(ICommand command, ICommandInterceptorOptions? options = null, CancellationToken cancellationToken = default);

		ICommandResult<bool> CanExecute<TResult>(ICommand<TResult> command, ICommandInterceptorOptions? options = null);

		Task<ICommandResult<bool>> CanExecuteAsync<TResult>(ICommand<TResult> command, ICommandInterceptorOptions? options = null, CancellationToken cancellationToken = default);

		ICommandResult Execute(ICommand command, ICommandInterceptorOptions? options = null);

		Task<ICommandResult> ExecuteAsync(ICommand command, ICommandInterceptorOptions? options = null, CancellationToken cancellationToken = default);

		ICommandResult<TResult> Execute<TResult>(ICommand<TResult> command, ICommandInterceptorOptions? options = null);

		Task<ICommandResult<TResult>> ExecuteAsync<TResult>(ICommand<TResult> command, ICommandInterceptorOptions? options = null, CancellationToken cancellationToken = default);
	}
}
