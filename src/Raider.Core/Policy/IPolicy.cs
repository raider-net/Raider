using System;
using System.Threading;

namespace Raider.Policy
{
	public interface IPolicy
	{
		/// <summary>
		/// Defines the implementation of a policy for sync executions with no return value.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		void Execute(Action action);

		/// <summary>
		/// Defines the implementation of a policy for sync executions with no return value.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		void Execute(Action<CancellationToken> action, CancellationToken cancellationToken);

		/// <summary>
		/// Defines the implementation of a policy for synchronous executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <typeparam name="TResult">The type returned by synchronous executions through the implementation.</typeparam>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <returns>A <typeparamref name="TResult"/> result of the execution.</returns>
		TResult Execute<TResult>(Func<CancellationToken, TResult> action);

		/// <summary>
		/// Defines the implementation of a policy for synchronous executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <typeparam name="TResult">The type returned by synchronous executions through the implementation.</typeparam>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <typeparamref name="TResult"/> result of the execution.</returns>
		TResult Execute<TResult>(Func<CancellationToken, TResult> action, CancellationToken cancellationToken);
	}

	public interface IPolicy<TResult>
	{
		/// <summary>
		/// Defines the implementation of a policy for synchronous executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <returns>A <typeparamref name="TResult"/> result of the execution.</returns>
		TResult Execute(Func<CancellationToken, TResult> action);

		/// <summary>
		/// Defines the implementation of a policy for synchronous executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <typeparamref name="TResult"/> result of the execution.</returns>
		TResult Execute(Func<CancellationToken, TResult> action, CancellationToken cancellationToken);
	}
}
