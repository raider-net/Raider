using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Policy.Internal
{
	internal static class FallbackPolicyProvider
	{
		internal static TResult Execute<TResult>(
			Func<CancellationToken, TResult> action,
			ExceptionPredicates shouldHandleExceptionPredicates,
			ResultPredicates<TResult> shouldHandleResultPredicates,
			Action<DelegateResult<TResult>> onFallback,
			Func<DelegateResult<TResult>, CancellationToken, TResult> fallbackAction,
			CancellationToken cancellationToken)
		{
			DelegateResult<TResult> delegateResult;

			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				TResult result = action(cancellationToken);

				if (!shouldHandleResultPredicates.AnyMatch(result))
				{
					return result;
				}

				delegateResult = new DelegateResult<TResult>(result);
			}
			catch (Exception ex)
			{
				var handledException = shouldHandleExceptionPredicates.FirstMatchOrDefault(ex);
				if (handledException == null)
				{
					throw;
				}

				delegateResult = new DelegateResult<TResult>(handledException);
			}

			onFallback(delegateResult);

			return fallbackAction(delegateResult, cancellationToken);
		}

		internal static async Task<TResult> ExecuteAsync<TResult>(
			Func<CancellationToken, Task<TResult>> action,
			ExceptionPredicates shouldHandleExceptionPredicates,
			ResultPredicates<TResult> shouldHandleResultPredicates,
			Func<DelegateResult<TResult>, Task> onFallbackAsync,
			Func<DelegateResult<TResult>, CancellationToken, Task<TResult>> fallbackAction,
			bool continueOnCapturedContext,
			CancellationToken cancellationToken)
		{
			DelegateResult<TResult> delegateResult;

			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				TResult result = await action(cancellationToken).ConfigureAwait(continueOnCapturedContext);

				if (!shouldHandleResultPredicates.AnyMatch(result))
				{
					return result;
				}

				delegateResult = new DelegateResult<TResult>(result);
			}
			catch (Exception ex)
			{
				var handledException = shouldHandleExceptionPredicates.FirstMatchOrDefault(ex);
				if (handledException == null)
				{
					throw;
				}

				delegateResult = new DelegateResult<TResult>(handledException);
			}

			await onFallbackAsync(delegateResult).ConfigureAwait(continueOnCapturedContext);

			return await fallbackAction(delegateResult, cancellationToken).ConfigureAwait(continueOnCapturedContext);
		}
	}
}
