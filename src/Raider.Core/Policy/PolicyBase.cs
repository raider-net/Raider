using Raider.Reflection;
using System;
using System.Threading;

namespace Raider.Policy
{
	public abstract class PolicyBase : IPolicy
	{
		internal static readonly CancellationToken DefaultCancellationToken = CancellationToken.None;

		internal const bool DefaultContinueOnCapturedContext = false;

		/// <summary>
		/// Defines the implementation of a policy for sync executions with no return value.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		public virtual void Execute(Action action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			Execute(ct => action(), DefaultCancellationToken);
		}

		/// <summary>
		/// Defines the implementation of a policy for sync executions with no return value.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		public virtual void Execute(Action<CancellationToken> action, CancellationToken cancellationToken)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			Execute<EmptyStruct>(
				ct =>
				{
					action(ct);
					return EmptyStruct.Instance;
				},
				cancellationToken);
		}

		/// <summary>
		/// Defines the implementation of a policy for synchronous executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <typeparam name="TResult">The type returned by synchronous executions through the implementation.</typeparam>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <returns>A <typeparamref name="TResult"/> result of the execution.</returns>
		public TResult Execute<TResult>(Func<CancellationToken, TResult> action)
			=> Execute(action, DefaultCancellationToken);

		/// <summary>
		/// Defines the implementation of a policy for synchronous executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <typeparam name="TResult">The type returned by synchronous executions through the implementation.</typeparam>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <typeparamref name="TResult"/> result of the execution.</returns>
		public abstract TResult Execute<TResult>(Func<CancellationToken, TResult> action, CancellationToken cancellationToken);
	}

	public abstract class PolicyBase<TResult> : IPolicy<TResult>
	{
		/// <summary>
		/// Defines the implementation of a policy for synchronous executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <returns>A <typeparamref name="TResult"/> result of the execution.</returns>
		public TResult Execute(Func<CancellationToken, TResult> action)
			=> Execute(action, PolicyBase.DefaultCancellationToken);

		/// <summary>
		/// Defines the implementation of a policy for synchronous executions returning <typeparamref name="TResult"/>.
		/// </summary>
		/// <param name="action">The action passed by calling code to execute through the policy.</param>
		/// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
		/// <returns>A <typeparamref name="TResult"/> result of the execution.</returns>
		public abstract TResult Execute(Func<CancellationToken, TResult> action, CancellationToken cancellationToken);
	}
}
