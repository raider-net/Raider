using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Policy
{
	public interface IAsyncPolicy
	{
		/// <summary>
		/// Defines the implementation of a policy for async executions with no return value.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <returns>A <see cref="Task"/> representing the result of the execution.</returns>
		Task ExecuteAsync(Func<CancellationToken, Task> action);

		/// <summary>
		/// Defines the implementation of a policy for async executions with no return value.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <see cref="Task"/> representing the result of the execution.</returns>
		Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken);

		/// <summary>
		/// Defines the implementation of a policy for async executions with no return value.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="continueOnCapturedContext">Whether async continuations should continue on a captured context.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <see cref="Task"/> representing the result of the execution.</returns>
		Task ExecuteAsync(Func<CancellationToken, Task> action, bool continueOnCapturedContext, CancellationToken cancellationToken);

		/// <summary>
		/// Defines the implementation of a policy for async executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <typeparam name="TResult">The type returned by asynchronous executions through the implementation.</typeparam>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> representing the result of the execution.</returns>
		Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken);

		/// <summary>
		/// Defines the implementation of a policy for async executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <typeparam name="TResult">The type returned by asynchronous executions through the implementation.</typeparam>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="continueOnCapturedContext">Whether async continuations should continue on a captured context.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> representing the result of the execution.</returns>
		Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, bool continueOnCapturedContext, CancellationToken cancellationToken);
	}

	public interface IAsyncPolicy<TResult>
	{
		/// <summary>
		/// Defines the implementation of a policy for async executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <returns>A <see cref="Task{TResult}"/> representing the result of the execution.</returns>
		Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action);

		/// <summary>
		/// Defines the implementation of a policy for async executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> representing the result of the execution.</returns>
		Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken);

		/// <summary>
		/// Defines the implementation of a policy for async executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="continueOnCapturedContext">Whether async continuations should continue on a captured context.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> representing the result of the execution.</returns>
		Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action, bool continueOnCapturedContext, CancellationToken cancellationToken);
	}
}
