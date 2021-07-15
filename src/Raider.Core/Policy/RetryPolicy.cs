using Raider.Policy.Internal;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Raider.Policy
{
	/// <summary>
	/// A retry policy that can be applied to synchronous delegates.
	/// </summary>
	public class RetryPolicy : PolicyBase
	{
		private readonly Func<Exception?, TimeSpan, int, RetryResult?> _onRetry;
		private readonly int _permittedRetryCount;
		private readonly IEnumerable<TimeSpan>? _sleepDurationsEnumerable;
		private readonly Func<int, Exception?, TimeSpan>? _sleepDurationProvider;
		private readonly Policy _policy;

		internal RetryPolicy(
			PolicyBuilder policyBuilder,
			Func<Exception?, TimeSpan, int, RetryResult?> onRetry,
			int permittedRetryCount = int.MaxValue,
			IEnumerable<TimeSpan>? sleepDurationsEnumerable = null,
			Func<int, Exception?, TimeSpan>? sleepDurationProvider = null)
		{
			_permittedRetryCount = permittedRetryCount;
			_sleepDurationsEnumerable = sleepDurationsEnumerable;
			_sleepDurationProvider = sleepDurationProvider;
			_onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
			_policy = new Policy(policyBuilder);
		}

		/// <inheritdoc/>
		public override TResult Execute<TResult>(Func<CancellationToken, TResult> action, CancellationToken cancellationToken)
			=> RetryPolicyProvider.Execute(
					action,
					_policy.ExceptionPredicates,
					ResultPredicates<TResult>.None,
					(result, timespan, retryCount) => _onRetry(result.Exception, timespan, retryCount),
					_permittedRetryCount,
					_sleepDurationsEnumerable,
					_sleepDurationProvider != null
						? (retryCount, result) => _sleepDurationProvider(retryCount, result.Exception)
						: null,
					cancellationToken);
	}

	/// <summary>
	/// A retry policy that can be applied to synchronous delegates returning a value of type <typeparamref name="TResult"/>.
	/// </summary>
	public class RetryPolicy<TResult> : PolicyBase<TResult>
	{
		private readonly Func<DelegateResult<TResult>, TimeSpan, int, RetryResult?> _onRetry;
		private readonly int _permittedRetryCount;
		private readonly IEnumerable<TimeSpan>? _sleepDurationsEnumerable;
		private readonly Func<int, DelegateResult<TResult>, TimeSpan>? _sleepDurationProvider;
		private readonly Policy<TResult> _policy;

		internal RetryPolicy(
			PolicyBuilder<TResult> policyBuilder,
			Func<DelegateResult<TResult>, TimeSpan, int, RetryResult?> onRetry,
			int permittedRetryCount = int.MaxValue,
			IEnumerable<TimeSpan>? sleepDurationsEnumerable = null,
			Func<int, DelegateResult<TResult>, TimeSpan>? sleepDurationProvider = null)
		{
			_permittedRetryCount = permittedRetryCount;
			_sleepDurationsEnumerable = sleepDurationsEnumerable;
			_sleepDurationProvider = sleepDurationProvider;
			_onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));
			_policy = new Policy<TResult>(policyBuilder);
		}

		/// <inheritdoc/>
		public override TResult Execute(Func<CancellationToken, TResult> action, CancellationToken cancellationToken)
			=> RetryPolicyProvider.Execute(
				action,
				_policy.ExceptionPredicates,
				_policy.ResultPredicates,
				_onRetry,
				_permittedRetryCount,
				_sleepDurationsEnumerable,
				_sleepDurationProvider,
				cancellationToken);
	}
}
