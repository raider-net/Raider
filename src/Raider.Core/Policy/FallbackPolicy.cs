using Raider.Policy.Internal;
using Raider.Reflection;
using System;
using System.Threading;

namespace Raider.Policy
{
	/// <summary>
	/// A fallback policy that can be applied to delegates.
	/// </summary>
	public class FallbackPolicy : PolicyBase
	{
		private readonly Action<Exception?> _onFallback;
		private readonly Action<Exception?, CancellationToken> _fallbackAction;
		private readonly Policy _policy;

		internal FallbackPolicy(
			PolicyBuilder policyBuilder,
			Action<Exception?> onFallback,
			Action<Exception?, CancellationToken> fallbackAction)
		{
			_onFallback = onFallback ?? throw new ArgumentNullException(nameof(onFallback));
			_fallbackAction = fallbackAction ?? throw new ArgumentNullException(nameof(fallbackAction));
			_policy = new Policy(policyBuilder);
		}

		/// <inheritdoc/>
		public override void Execute(Action<CancellationToken> action, CancellationToken cancellationToken)
			=> FallbackPolicyProvider.Execute(
					(token) =>
					{
						action(token);
						return EmptyStruct.Instance;
					},
					_policy.ExceptionPredicates,
					ResultPredicates<EmptyStruct>.None,
					(result) => _onFallback(result.Exception),
					(result, ct) =>
					{
						_fallbackAction(result.Exception, ct);
						return EmptyStruct.Instance;
					},
					cancellationToken);

		/// <inheritdoc/>
		public override TResult Execute<TResult>(Func<CancellationToken, TResult> action, CancellationToken cancellationToken)
			=> throw new InvalidOperationException($"You have executed the generic .Execute<{nameof(TResult)}> method on a non-generic {nameof(FallbackPolicy)}.  A non-generic {nameof(FallbackPolicy)} only defines a fallback action which returns void; it can never return a substitute {nameof(TResult)} value.  To use {nameof(FallbackPolicy)} to provide fallback {nameof(TResult)} values you must define a generic fallback policy {nameof(FallbackPolicy)}<{nameof(TResult)}>.  For example, define the policy as Policy<{nameof(TResult)}>.Handle<Whatever>.Fallback<{nameof(TResult)}>(/* some {nameof(TResult)} value or Func<..., {nameof(TResult)}> */);");
	}

	/// <summary>
	/// A fallback policy that can be applied to delegates returning a value of type <typeparamref name="TResult"/>.
	/// </summary>
	public class FallbackPolicy<TResult> : PolicyBase<TResult>
	{
		private readonly Action<DelegateResult<TResult>> _onFallback;
		private readonly Func<DelegateResult<TResult>, CancellationToken, TResult> _fallbackAction;
		private readonly Policy<TResult> _policy;

		internal FallbackPolicy(
			PolicyBuilder<TResult> policyBuilder,
			Action<DelegateResult<TResult>> onFallback,
			Func<DelegateResult<TResult>, CancellationToken, TResult> fallbackAction)
		{
			_onFallback = onFallback ?? throw new ArgumentNullException(nameof(onFallback));
			_fallbackAction = fallbackAction ?? throw new ArgumentNullException(nameof(fallbackAction));
			_policy = new Policy<TResult>(policyBuilder);
		}

		/// <inheritdoc/>
		public override TResult Execute(Func<CancellationToken, TResult> action, CancellationToken cancellationToken)
			=> FallbackPolicyProvider.Execute(
				action,
				_policy.ExceptionPredicates,
				_policy.ResultPredicates,
				_onFallback,
				_fallbackAction,
				cancellationToken);
	}
}
