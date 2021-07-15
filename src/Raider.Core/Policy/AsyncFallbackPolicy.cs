using Raider.Policy.Internal;
using Raider.Reflection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Policy
{
	/// <summary>
	/// A fallback policy that can be applied to asynchronous delegates.
	/// </summary>
	public class AsyncFallbackPolicy : AsyncPolicyBase
	{
		private readonly Func<Exception?, Task> _onFallbackAsync;
		private readonly Func<Exception?, CancellationToken, Task> _fallbackAction;
		private readonly Policy _policy;

		internal AsyncFallbackPolicy(
			PolicyBuilder policyBuilder,
			Func<Exception?, Task> onFallbackAsync,
			Func<Exception?, CancellationToken, Task> fallbackAction)
		{
			_onFallbackAsync = onFallbackAsync ?? throw new ArgumentNullException(nameof(onFallbackAsync));
			_fallbackAction = fallbackAction ?? throw new ArgumentNullException(nameof(fallbackAction));
			_policy = new Policy(policyBuilder);
		}

		/// <inheritdoc/>
		public override Task ExecuteAsync(
			Func<CancellationToken, Task> action,
			bool continueOnCapturedContext,
			CancellationToken cancellationToken)
			=> AsyncFallbackPolicyProvider.ExecuteAsync<EmptyStruct>(
				async (ct) =>
				{
					await action(ct).ConfigureAwait(continueOnCapturedContext);
					return EmptyStruct.Instance;
				},
				_policy.ExceptionPredicates,
				ResultPredicates<EmptyStruct>.None,
				(result) => _onFallbackAsync(result.Exception),
				async (result, ct) =>
				{
					await _fallbackAction(result.Exception, ct).ConfigureAwait(continueOnCapturedContext);
					return EmptyStruct.Instance;
				},
				continueOnCapturedContext,
				cancellationToken);

		/// <inheritdoc/>
		public override Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, bool continueOnCapturedContext, CancellationToken cancellationToken)
			=>
			throw new InvalidOperationException($"You have executed the generic .Execute<{nameof(TResult)}> method on a non-generic {nameof(FallbackPolicy)}.  A non-generic {nameof(FallbackPolicy)} only defines a fallback action which returns void; it can never return a substitute {nameof(TResult)} value.  To use {nameof(FallbackPolicy)} to provide fallback {nameof(TResult)} values you must define a generic fallback policy {nameof(FallbackPolicy)}<{nameof(TResult)}>.  For example, define the policy as Policy<{nameof(TResult)}>.Handle<Whatever>.Fallback<{nameof(TResult)}>(/* some {nameof(TResult)} value or Func<..., {nameof(TResult)}> */);");
	}

	/// <summary>
	/// A fallback policy that can be applied to delegates.
	/// </summary>
	/// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
	public class AsyncFallbackPolicy<TResult> : AsyncPolicyBase<TResult>
	{
		private readonly Func<DelegateResult<TResult>, Task> _onFallbackAsync;
		private readonly Func<DelegateResult<TResult>, CancellationToken, Task<TResult>> _fallbackAction;
		private readonly Policy<TResult> _policy;

		internal AsyncFallbackPolicy(
			PolicyBuilder<TResult> policyBuilder,
			Func<DelegateResult<TResult>, Task> onFallbackAsync,
			Func<DelegateResult<TResult>, CancellationToken, Task<TResult>> fallbackAction)
		{
			_onFallbackAsync = onFallbackAsync ?? throw new ArgumentNullException(nameof(onFallbackAsync));
			_fallbackAction = fallbackAction ?? throw new ArgumentNullException(nameof(fallbackAction));
			_policy = new Policy<TResult>(policyBuilder);
		}

		/// <inheritdoc/>
		public override Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action, bool continueOnCapturedContext, CancellationToken cancellationToken)
			=> AsyncFallbackPolicyProvider.ExecuteAsync(
				action,
				_policy.ExceptionPredicates,
				_policy.ResultPredicates,
				_onFallbackAsync,
				_fallbackAction,
				continueOnCapturedContext,
				cancellationToken);
	}
}
