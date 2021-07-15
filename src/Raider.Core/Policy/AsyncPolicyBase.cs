using Raider.Reflection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Policy
{
	public abstract class AsyncPolicyBase : IAsyncPolicy
	{
		/// <summary>
		/// Defines the implementation of a policy for async executions with no return value.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <returns>A <see cref="Task"/> representing the result of the execution.</returns>
		public virtual Task ExecuteAsync(Func<CancellationToken, Task> action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			return ExecuteAsync<EmptyStruct>(
				async (token) =>
				{
					await action(token).ConfigureAwait(PolicyBase.DefaultContinueOnCapturedContext);
					return EmptyStruct.Instance;
				},
				PolicyBase.DefaultContinueOnCapturedContext,
				PolicyBase.DefaultCancellationToken);
		}

		/// <summary>
		/// Defines the implementation of a policy for async executions with no return value.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <see cref="Task"/> representing the result of the execution.</returns>
		public virtual Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			return ExecuteAsync<EmptyStruct>(
				async (token) =>
				{
					await action(token).ConfigureAwait(PolicyBase.DefaultContinueOnCapturedContext);
					return EmptyStruct.Instance;
				},
				PolicyBase.DefaultContinueOnCapturedContext,
				cancellationToken);
		}

		/// <summary>
		/// Defines the implementation of a policy for async executions with no return value.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="continueOnCapturedContext">Whether async continuations should continue on a captured context.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <see cref="Task"/> representing the result of the execution.</returns>
		public virtual Task ExecuteAsync(Func<CancellationToken, Task> action, bool continueOnCapturedContext, CancellationToken cancellationToken)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			return ExecuteAsync<EmptyStruct>(
				async (token) =>
				{
					await action(token).ConfigureAwait(continueOnCapturedContext);
					return EmptyStruct.Instance;
				},
				continueOnCapturedContext,
				cancellationToken);
		}

		/// <summary>
		/// Defines the implementation of a policy for async executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <typeparam name="TResult">The type returned by asynchronous executions through the implementation.</typeparam>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> representing the result of the execution.</returns>
		public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
			=> ExecuteAsync(action, PolicyBase.DefaultContinueOnCapturedContext, cancellationToken);

		/// <summary>
		/// Defines the implementation of a policy for async executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <typeparam name="TResult">The type returned by asynchronous executions through the implementation.</typeparam>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="continueOnCapturedContext">Whether async continuations should continue on a captured context.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> representing the result of the execution.</returns>
		public abstract Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, bool continueOnCapturedContext, CancellationToken cancellationToken);
	}

	public abstract class AsyncPolicyBase<TResult> : IAsyncPolicy<TResult>
	{
		/// <summary>
		/// Defines the implementation of a policy for async executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <returns>A <see cref="Task{TResult}"/> representing the result of the execution.</returns>
		public Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action)
			=> ExecuteAsync(action, PolicyBase.DefaultContinueOnCapturedContext, PolicyBase.DefaultCancellationToken);

		/// <summary>
		/// Defines the implementation of a policy for async executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> representing the result of the execution.</returns>
		public Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
			=> ExecuteAsync(action, PolicyBase.DefaultContinueOnCapturedContext, cancellationToken);

		/// <summary>
		/// Defines the implementation of a policy for async executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="continueOnCapturedContext">Whether async continuations should continue on a captured context.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> representing the result of the execution.</returns>
		public abstract Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action, bool continueOnCapturedContext, CancellationToken cancellationToken);
	}
}
