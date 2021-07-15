using Raider.Policy.Internal;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Policy
{
	/// <summary>
	/// A retry policy that can be applied to asynchronous delegates.
	/// </summary>
	public class AsyncRetryPolicy : AsyncPolicyBase
	{
		private readonly Func<Exception?, TimeSpan, int, Task<RetryResult?>> _onRetryAsync;
		private readonly int _permittedRetryCount;
		private readonly IEnumerable<TimeSpan>? _sleepDurationsEnumerable;
		private readonly Func<int, Exception?, TimeSpan>? _sleepDurationProvider;
		private readonly Policy _policy;

		internal AsyncRetryPolicy(
			PolicyBuilder policyBuilder,
			Func<Exception?, TimeSpan, int, Task<RetryResult?>> onRetryAsync,
			int permittedRetryCount = int.MaxValue,
			IEnumerable<TimeSpan>? sleepDurationsEnumerable = null,
			Func<int, Exception?, TimeSpan>? sleepDurationProvider = null)
		{
			_permittedRetryCount = permittedRetryCount;
			_sleepDurationsEnumerable = sleepDurationsEnumerable;
			_sleepDurationProvider = sleepDurationProvider;
			_onRetryAsync = onRetryAsync ?? throw new ArgumentNullException(nameof(onRetryAsync));
			_policy = new Policy(policyBuilder);
		}

		/// <inheritdoc/>
		public override Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, bool continueOnCapturedContext, CancellationToken cancellationToken)
			=> AsyncRetryPolicyProvider.ExecuteAsync(
				action,
				_policy.ExceptionPredicates,
				ResultPredicates<TResult>.None,
				(result, timespan, retryCount) => _onRetryAsync(result.Exception, timespan, retryCount),
				_permittedRetryCount,
				_sleepDurationsEnumerable,
				_sleepDurationProvider != null
					? (retryCount, result) => _sleepDurationProvider(retryCount, result.Exception)
					: null,
				continueOnCapturedContext,
				cancellationToken);
	}

	/// <summary>
	/// A retry policy that can be applied to asynchronous delegates returning a value of type <typeparamref name="TResult"/>.
	/// </summary>
	public class AsyncRetryPolicy<TResult> : AsyncPolicyBase<TResult>
	{
		private readonly Func<DelegateResult<TResult>, TimeSpan, int, Task<RetryResult?>> _onRetryAsync;
		private readonly int _permittedRetryCount;
		private readonly IEnumerable<TimeSpan>? _sleepDurationsEnumerable;
		private readonly Func<int, DelegateResult<TResult>, TimeSpan>? _sleepDurationProvider;
		private readonly Policy<TResult> _policy;

		internal AsyncRetryPolicy(
			PolicyBuilder<TResult> policyBuilder,
			Func<DelegateResult<TResult>, TimeSpan, int, Task<RetryResult?>> onRetryAsync,
			int permittedRetryCount = int.MaxValue,
			IEnumerable<TimeSpan>? sleepDurationsEnumerable = null,
			Func<int, DelegateResult<TResult>, TimeSpan>? sleepDurationProvider = null)
		{
			_permittedRetryCount = permittedRetryCount;
			_sleepDurationsEnumerable = sleepDurationsEnumerable;
			_sleepDurationProvider = sleepDurationProvider;
			_onRetryAsync = onRetryAsync ?? throw new ArgumentNullException(nameof(onRetryAsync));
			_policy = new Policy<TResult>(policyBuilder);
		}

		/// <inheritdoc/>
		public override Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action, bool continueOnCapturedContext, CancellationToken cancellationToken)
			=> AsyncRetryPolicyProvider.ExecuteAsync(
				action,
				_policy.ExceptionPredicates,
				_policy.ResultPredicates,
				_onRetryAsync,
				_permittedRetryCount,
				_sleepDurationsEnumerable,
				_sleepDurationProvider,
				continueOnCapturedContext,
				cancellationToken);
	}
}

