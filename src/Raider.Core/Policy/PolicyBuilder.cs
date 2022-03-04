using Raider.Policy.Internal;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Policy
{
	/// <summary>
	/// Builder class that holds the list of current exception predicates.
	/// </summary>
	public sealed class PolicyBuilder
	{
		internal ExceptionPredicates ExceptionPredicates { get; }

		internal PolicyBuilder(ExceptionPredicate exceptionPredicate)
		{
			if (exceptionPredicate == null)
				throw new ArgumentNullException(nameof(exceptionPredicate));

			ExceptionPredicates = new ExceptionPredicates();
			ExceptionPredicates.Add(exceptionPredicate);
		}

		/// <summary>
		/// Specifies the type of exception that this policy can handle.
		/// </summary>
		/// <typeparam name="TException">The type of the exception to handle.</typeparam>
		/// <returns>The PolicyBuilder instance.</returns>
		public PolicyBuilder HandleException<TException>() where TException : Exception
		{
			ExceptionPredicates.Add(exception => exception is TException ? exception : null);
			return this;
		}

		/// <summary>
		/// Specifies the type of exception that this policy can handle with additional filters on this exception type.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
		/// <returns>The PolicyBuilder instance.</returns>
		public PolicyBuilder HandleException<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception
		{
			ExceptionPredicates.Add(exception => exception is TException texception && exceptionPredicate(texception) ? exception : null);
			return this;
		}

		/// <summary>
		/// Specifies the type of exception that this policy can handle if found as an InnerException of a regular <see cref="Exception"/>, or at any level of nesting within an <see cref="AggregateException"/>.
		/// </summary>
		/// <typeparam name="TException">The type of the exception to handle.</typeparam>
		/// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
		public PolicyBuilder HandleInnerException<TException>() where TException : Exception
		{
			ExceptionPredicates.Add((HandleInner(ex => ex is TException)));
			return this;
		}

		/// <summary>
		/// Specifies the type of exception that this policy can handle, with additional filters on this exception type, if found as an InnerException of a regular <see cref="Exception"/>, or at any level of nesting within an <see cref="AggregateException"/>.
		/// </summary>
		/// <typeparam name="TException">The type of the exception to handle.</typeparam>
		/// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
		/// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
		public PolicyBuilder HandleInnerException<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception
		{
			ExceptionPredicates.Add(HandleInner(exception => exception is TException texception && exceptionPredicate(texception)));
			return this;
		}

		internal static ExceptionPredicate HandleInner(Func<Exception?, bool> predicate)
		{
			return exception =>
			{
				if (exception is AggregateException aggregateException)
				{
					//search all inner exceptions wrapped inside the AggregateException recursively
					foreach (var innerException in aggregateException.Flatten().InnerExceptions)
					{
						var matchedInAggregate = HandleInnerNested(predicate, innerException);
						if (matchedInAggregate != null)
							return matchedInAggregate;
					}
				}

				return HandleInnerNested(predicate, exception);
			};
		}

		private static Exception? HandleInnerNested(Func<Exception?, bool> predicate, Exception? current)
		{
			if (current == null) return null;
			if (predicate(current)) return current;
			return HandleInnerNested(predicate, current.InnerException);
		}

		/// <summary>
		/// Specifies the type of result that this policy can handle with additional filters on the result.
		/// </summary>
		/// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
		/// <param name="resultPredicate">The predicate to filter the results this policy will handle.</param>
		/// <returns>The PolicyBuilder instance.</returns>
		public PolicyBuilder<TResult> HandleResult<TResult>(Func<TResult, bool> resultPredicate)
			=> new PolicyBuilder<TResult>(ExceptionPredicates).HandleResult(resultPredicate);

		/// <summary>
		/// Specifies a result value which the policy will handle.
		/// </summary>
		/// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
		/// <param name="result">The TResult value this policy will handle.</param>
		/// <remarks>This policy filter matches the <paramref name="result"/> value returned using .Equals(), ideally suited for value types such as int and enum.  To match characteristics of class return types, consider the overload taking a result predicate.</remarks>
		/// <returns>The PolicyBuilder instance.</returns>
		public PolicyBuilder<TResult> HandleResult<TResult>(TResult result)
			=> HandleResult<TResult>(r => (r != null && r.Equals(result)) || (r == null && result == null));

		/// <summary>
		/// Builds a <see cref="Policy"/> that will retry <paramref name="retryCount"/> times.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <returns>The policy instance.</returns>
		public RetryPolicy Retry(int retryCount)
		{
			return new RetryPolicy(
				this,
				(ex, timespan, i) => null,
				retryCount);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will retry <paramref name="retryCount"/> times
		/// calling <paramref name="onRetry"/> on each retry with the raised exception and retry count.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">onRetry</exception>
		public RetryPolicy Retry(int retryCount, Action<Exception?, int> onRetry)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy(
				this,
				(ex, timespan, i) =>
				{
					onRetry(ex, i);
					return null;
				},
				retryCount);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will retry <paramref name="retryCount"/> times
		/// calling <paramref name="onRetry"/> on each retry with the raised exception and retry count.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">onRetry</exception>
		public RetryPolicy Retry(int retryCount, Func<Exception?, int, RetryResult?> onRetry)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy(
				this,
				(ex, timespan, i) => onRetry(ex, i),
				retryCount);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will retry indefinitely until the action succeeds.
		/// </summary>
		/// <returns>The policy instance.</returns>
		public RetryPolicy RetryUntilSucceeds()
		{
			return new RetryPolicy(
				this,
				(ex, timespan, i) => null);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will retry indefinitely
		/// calling <paramref name="onRetry"/> on each retry with the raised exception and retry count.
		/// </summary>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">onRetry</exception>
		public RetryPolicy RetryUntilSucceeds(Action<Exception?, int> onRetry)
		{
			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy(
				this,
				(ex, timespan, i) =>
				{
					onRetry(ex, i);
					return null;
				});
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will retry indefinitely
		/// calling <paramref name="onRetry"/> on each retry with the raised exception and retry count.
		/// </summary>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">onRetry</exception>
		public RetryPolicy RetryUntilSucceeds(Func<Exception?, int, RetryResult?> onRetry)
		{
			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy(
				this,
				(ex, timespan, i) => onRetry(ex, i));
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry <paramref name="retryCount"/> times.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		public RetryPolicy WaitAndRetry(int retryCount, Func<int, Exception?, TimeSpan> sleepDurationProvider)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			return new RetryPolicy(
				this,
				(ex, timespan, i) => null,
				retryCount,
				sleepDurationProvider: sleepDurationProvider);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry <paramref name="retryCount"/> times
		/// calling <paramref name="onRetry"/> on each retry with the raised exception, current sleep duration, retry count, and context data.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">
		/// timeSpanProvider
		/// or
		/// onRetry
		/// </exception>
		public RetryPolicy WaitAndRetry(int retryCount, Func<int, Exception?, TimeSpan> sleepDurationProvider, Action<Exception?, TimeSpan, int> onRetry)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy(
				this,
				(ex, timespan, i) =>
				{
					onRetry(ex, timespan, i);
					return null;
				},
				retryCount,
				sleepDurationProvider: sleepDurationProvider);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry <paramref name="retryCount"/> times
		/// calling <paramref name="onRetry"/> on each retry with the raised exception, current sleep duration, retry count, and context data.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">
		/// timeSpanProvider
		/// or
		/// onRetry
		/// </exception>
		public RetryPolicy WaitAndRetry(int retryCount, Func<int, Exception?, TimeSpan> sleepDurationProvider, Func<Exception?, TimeSpan, int, RetryResult?> onRetry)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy(
				this,
				onRetry,
				retryCount,
				sleepDurationProvider: sleepDurationProvider);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry as many times as there are provided <paramref name="sleepDurations"/>
		/// On each retry, the duration to wait is the current <paramref name="sleepDurations"/> item.
		/// </summary>
		/// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
		/// <returns>The policy instance.</returns>
		public RetryPolicy WaitAndRetry(IEnumerable<TimeSpan> sleepDurations)
		{
			if (sleepDurations == null) throw new ArgumentNullException(nameof(sleepDurations));

			return new RetryPolicy(
				this,
				(ex, timespan, i) => null,
				sleepDurationsEnumerable: sleepDurations);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry as many times as there are provided <paramref name="sleepDurations"/>
		/// calling <paramref name="onRetry"/> on each retry with the raised exception, current sleep duration, retry count and context data.
		/// On each retry, the duration to wait is the current <paramref name="sleepDurations"/> item.
		/// </summary>
		/// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">
		/// sleepDurations
		/// or
		/// onRetry
		/// </exception>
		public RetryPolicy WaitAndRetry(IEnumerable<TimeSpan> sleepDurations, Action<Exception?, TimeSpan, int> onRetry)
		{
			if (sleepDurations == null)
				throw new ArgumentNullException(nameof(sleepDurations));

			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy(
				this,
				(ex, timespan, i) =>
				{
					onRetry(ex, timespan, i);
					return null;
				},
				sleepDurationsEnumerable: sleepDurations);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry as many times as there are provided <paramref name="sleepDurations"/>
		/// calling <paramref name="onRetry"/> on each retry with the raised exception, current sleep duration, retry count and context data.
		/// On each retry, the duration to wait is the current <paramref name="sleepDurations"/> item.
		/// </summary>
		/// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">
		/// sleepDurations
		/// or
		/// onRetry
		/// </exception>
		public RetryPolicy WaitAndRetry(IEnumerable<TimeSpan> sleepDurations, Func<Exception?, TimeSpan, int, RetryResult?> onRetry)
		{
			if (sleepDurations == null)
				throw new ArgumentNullException(nameof(sleepDurations));

			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy(
				this,
				onRetry,
				sleepDurationsEnumerable: sleepDurations);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry indefinitely until the action succeeds.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
		/// the current retry number (1 for first retry, 2 for second etc)
		/// </summary>
		/// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		public RetryPolicy WaitAndRetryUntilSucceeds(Func<int, Exception?, TimeSpan> sleepDurationProvider)
		{
			if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

			return new RetryPolicy(
				this,
				(ex, timespan, i) => null,
				sleepDurationProvider: sleepDurationProvider);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry indefinitely until the action succeeds, 
		/// calling <paramref name="onRetry"/> on each retry with the raised exception.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
		/// the current retry number (1 for first retry, 2 for second etc)
		/// </summary>
		/// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		/// <exception cref="ArgumentNullException">onRetry</exception>
		public RetryPolicy WaitAndRetryUntilSucceeds(Func<int, Exception?, TimeSpan> sleepDurationProvider, Action<Exception?, int, TimeSpan> onRetry)
		{
			if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
			if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy(
				this,
				(ex, timespan, i) =>
				{
					onRetry(ex, i, timespan);
					return null;
				},
				sleepDurationProvider: sleepDurationProvider);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry indefinitely until the action succeeds, 
		/// calling <paramref name="onRetry"/> on each retry with the raised exception.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
		/// the current retry number (1 for first retry, 2 for second etc)
		/// </summary>
		/// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		/// <exception cref="ArgumentNullException">onRetry</exception>
		public RetryPolicy WaitAndRetryUntilSucceeds(Func<int, Exception?, TimeSpan> sleepDurationProvider, Func<Exception?, int, TimeSpan, RetryResult?> onRetry)
		{
			if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
			if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy(
				this,
				(ex, timespan, i) => onRetry(ex, i, timespan),
				sleepDurationProvider: sleepDurationProvider);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy" /> that will retry <paramref name="retryCount" /> times.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <returns>The policy instance.</returns>
		public AsyncRetryPolicy RetryAsync(int retryCount)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			return new AsyncRetryPolicy(
				this,
				(ex, timespan, i) => Task.FromResult<RetryResult?>(null),
				retryCount);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy"/> that will retry <paramref name="retryCount"/> times
		/// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and context data.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="onRetryAsync">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
		/// <exception cref="ArgumentNullException">onRetryAsync</exception>
		public AsyncRetryPolicy RetryAsync(int retryCount, Func<Exception?, int, Task> onRetryAsync)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy(
				this,
				async (ex, timespan, i) =>
				{
					await onRetryAsync(ex, i).ConfigureAwait(false);
					return null;
				},
				retryCount
			);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy"/> that will retry <paramref name="retryCount"/> times
		/// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and context data.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="onRetryAsync">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
		/// <exception cref="ArgumentNullException">onRetryAsync</exception>
		public AsyncRetryPolicy RetryAsync(int retryCount, Func<Exception?, int, Task<RetryResult?>> onRetryAsync)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy(
				this,
				(ex, timespan, i) => onRetryAsync(ex, i),
				retryCount
			);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy" /> that will retry indefinitely until the action succeeds.
		/// </summary>
		/// <returns>The policy instance.</returns>
		public AsyncRetryPolicy RetryUntilSucceedsAsync()
		{
			return new AsyncRetryPolicy(
				this,
				(ex, timespan, i) => Task.FromResult<RetryResult?>(null));
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy"/> that will retry indefinitely
		/// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and context data.
		/// </summary>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">onRetryAsync</exception>
		public AsyncRetryPolicy RetryUntilSucceedsAsync(Func<Exception?, int, Task> onRetryAsync)
		{
			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy(
				this,
				async (ex, timespan, i) =>
				{
					await onRetryAsync(ex, i).ConfigureAwait(false);
					return null;
				});
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy"/> that will retry indefinitely
		/// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and context data.
		/// </summary>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">onRetryAsync</exception>
		public AsyncRetryPolicy RetryUntilSucceedsAsync(Func<Exception?, int, Task<RetryResult?>> onRetryAsync)
		{
			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy(
				this,
				(ex, timespan, i) => onRetryAsync(ex, i));
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		public AsyncRetryPolicy WaitAndRetryAsync(int retryCount, Func<int, Exception?, TimeSpan> sleepDurationProvider)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			return new AsyncRetryPolicy(
				this,
				(ex, timespan, i) => Task.FromResult<RetryResult?>(null),
				retryCount,
				sleepDurationProvider: sleepDurationProvider
			);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
		/// calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">
		/// sleepDurationProvider
		/// or
		/// onRetryAsync
		/// </exception>
		public AsyncRetryPolicy WaitAndRetryAsync(int retryCount, Func<int, Exception?, TimeSpan> sleepDurationProvider, Func<Exception?, TimeSpan, int, Task> onRetryAsync)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy(
				this,
				async (ex, timespan, i) =>
				{
					await onRetryAsync(ex, timespan, i).ConfigureAwait(false);
					return null;
				},
				retryCount,
				sleepDurationProvider: sleepDurationProvider
			);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry <paramref name="retryCount" /> times
		/// calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">
		/// sleepDurationProvider
		/// or
		/// onRetryAsync
		/// </exception>
		public AsyncRetryPolicy WaitAndRetryAsync(int retryCount, Func<int, Exception?, TimeSpan> sleepDurationProvider, Func<Exception?, TimeSpan, int, Task<RetryResult?>> onRetryAsync)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy(
				this,
				onRetryAsync,
				retryCount,
				sleepDurationProvider: sleepDurationProvider
			);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided <paramref name="sleepDurations" />.
		/// On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
		/// </summary>
		/// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">sleepDurations</exception>
		public AsyncRetryPolicy WaitAndRetryAsync(IEnumerable<TimeSpan> sleepDurations)
		{
			if (sleepDurations == null)
				throw new ArgumentNullException(nameof(sleepDurations));

			return new AsyncRetryPolicy(
				this,
				(ex, timespan, i) => Task.FromResult<RetryResult?>(null),
				sleepDurationsEnumerable: sleepDurations);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
		/// <paramref name="sleepDurations" />
		/// calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
		/// On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
		/// </summary>
		/// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">
		/// sleepDurations
		/// or
		/// onRetryAsync
		/// </exception>
		public AsyncRetryPolicy WaitAndRetryAsync(IEnumerable<TimeSpan> sleepDurations, Func<Exception?, TimeSpan, int, Task> onRetryAsync)
		{
			if (sleepDurations == null)
				throw new ArgumentNullException(nameof(sleepDurations));
			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy(
				this,
				async (ex, timespan, i) =>
				{
					await onRetryAsync(ex, timespan, i).ConfigureAwait(false);
					return null;
				},
				sleepDurationsEnumerable: sleepDurations);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy" /> that will wait and retry as many times as there are provided
		/// <paramref name="sleepDurations" />
		/// calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
		/// On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
		/// </summary>
		/// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">
		/// sleepDurations
		/// or
		/// onRetryAsync
		/// </exception>
		public AsyncRetryPolicy WaitAndRetryAsync(IEnumerable<TimeSpan> sleepDurations, Func<Exception?, TimeSpan, int, Task<RetryResult?>> onRetryAsync)
		{
			if (sleepDurations == null)
				throw new ArgumentNullException(nameof(sleepDurations));
			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy(
				this,
				onRetryAsync,
				sleepDurationsEnumerable: sleepDurations);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		public AsyncRetryPolicy WaitAndRetryUntilSucceedsAsync(Func<int, Exception?, TimeSpan> sleepDurationProvider)
		{
			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			return new AsyncRetryPolicy(
				this,
				(ex, timespan, i) => Task.FromResult<RetryResult?>(null),
				sleepDurationProvider: sleepDurationProvider
			);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
		/// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and execution context.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		/// <exception cref="ArgumentNullException">onRetryAsync</exception>
		public AsyncRetryPolicy WaitAndRetryUntilSucceedsAsync(Func<int, Exception?, TimeSpan> sleepDurationProvider, Func<Exception?, int, TimeSpan, Task> onRetryAsync)
		{
			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy(
				this,
				async (exception, timespan, i) =>
				{
					await onRetryAsync(exception, i, timespan).ConfigureAwait(false);
					return null;
				},
				sleepDurationProvider: sleepDurationProvider
			);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy"/> that will wait and retry indefinitely
		/// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and execution context.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		/// <exception cref="ArgumentNullException">onRetryAsync</exception>
		public AsyncRetryPolicy WaitAndRetryUntilSucceedsAsync(Func<int, Exception?, TimeSpan> sleepDurationProvider, Func<Exception?, int, TimeSpan, Task<RetryResult?>> onRetryAsync)
		{
			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy(
				this,
				(exception, timespan, i) => onRetryAsync(exception, i, timespan),
				sleepDurationProvider: sleepDurationProvider
			);
		}

		/// <summary>
		/// Builds a <see cref="FallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate, but if this throws a handled exception, first calls <paramref name="onFallback"/> with details of the handled exception and the execution context; then calls <paramref name="fallbackAction"/>.  
		/// </summary>
		/// <param name="fallbackAction">The fallback action.</param>
		/// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
		/// <exception cref="ArgumentNullException">fallbackAction</exception>
		/// <exception cref="ArgumentNullException">onFallback</exception>
		/// <returns>The policy instance.</returns>
		public FallbackPolicy Fallback(Action<Exception?, CancellationToken> fallbackAction, Action<Exception?> onFallback)
		{
			if (fallbackAction == null)
				throw new ArgumentNullException(nameof(fallbackAction));

			if (onFallback == null)
				throw new ArgumentNullException(nameof(onFallback));

			return new FallbackPolicy(this, onFallback, fallbackAction);
		}

		/// <summary>
		/// Builds an <see cref="AsyncFallbackPolicy"/> which provides a fallback action if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception and execution context; then asynchronously calls <paramref name="fallbackAction"/>.  
		/// </summary>
		/// <param name="fallbackAction">The fallback delegate.</param>
		/// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
		/// <exception cref="ArgumentNullException">fallbackAction</exception>
		/// <exception cref="ArgumentNullException">onFallbackAsync</exception>
		/// <returns>The policy instance.</returns>
		public AsyncFallbackPolicy FallbackAsync(Func<Exception?, CancellationToken, Task> fallbackAction, Func<Exception?, Task> onFallbackAsync)
		{
			if (fallbackAction == null)
				throw new ArgumentNullException(nameof(fallbackAction));

			if (onFallbackAsync == null)
				throw new ArgumentNullException(nameof(onFallbackAsync));

			return new AsyncFallbackPolicy(this, onFallbackAsync, fallbackAction);
		}
	}

	/// <summary>
	/// Builder class that holds the list of current execution predicates filtering TResult result values.
	/// </summary>
	public class PolicyBuilder<TResult>
	{
		internal ExceptionPredicates ExceptionPredicates { get; }

		internal ResultPredicates<TResult> ResultPredicates { get; }

		private PolicyBuilder()
		{
			ExceptionPredicates = new ExceptionPredicates();
			ResultPredicates = new ResultPredicates<TResult>();
		}

		internal PolicyBuilder(Func<TResult, bool> resultPredicate) : this()
			=> HandleResult(resultPredicate);

		internal PolicyBuilder(ExceptionPredicate predicate) : this()
			=> ExceptionPredicates.Add(predicate);

		internal PolicyBuilder(ExceptionPredicates exceptionPredicates)
			: this()
			=> ExceptionPredicates = exceptionPredicates;

		/// <summary>
		/// Specifies the type of exception that this policy can handle.
		/// </summary>
		/// <typeparam name="TException">The type of the exception to handle.</typeparam>
		/// <returns>The PolicyBuilder instance.</returns>
		public PolicyBuilder<TResult> HandleException<TException>() where TException : Exception
		{
			ExceptionPredicates.Add(exception => exception is TException ? exception : null);
			return this;
		}

		/// <summary>
		/// Specifies the type of exception that this policy can handle with additional filters on this exception type.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
		/// <returns>The PolicyBuilder instance.</returns>
		public PolicyBuilder<TResult> HandleException<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception
		{
			ExceptionPredicates.Add(exception => exception is TException texception && exceptionPredicate(texception) ? exception : null);
			return this;
		}

		/// <summary>
		/// Specifies the type of exception that this policy can handle if found as an InnerException of a regular <see cref="Exception"/>, or at any level of nesting within an <see cref="AggregateException"/>.
		/// </summary>
		/// <typeparam name="TException">The type of the exception to handle.</typeparam>
		/// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
		public PolicyBuilder<TResult> HandleInnerException<TException>() where TException : Exception
		{
			ExceptionPredicates.Add((PolicyBuilder.HandleInner(ex => ex is TException)));
			return this;
		}

		/// <summary>
		/// Specifies the type of exception that this policy can handle, with additional filters on this exception type, if found as an InnerException of a regular <see cref="Exception"/>, or at any level of nesting within an <see cref="AggregateException"/>.
		/// </summary>
		/// <typeparam name="TException">The type of the exception to handle.</typeparam>
		/// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
		/// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
		public PolicyBuilder<TResult> HandleInnerException<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception
		{
			ExceptionPredicates.Add(PolicyBuilder.HandleInner(ex => ex is TException texception && exceptionPredicate(texception)));
			return this;
		}

		/// <summary>
		/// Specifies the type of result that this policy can handle with additional filters on the result.
		/// </summary>
		/// <param name="resultPredicate">The predicate to filter the results this policy will handle.</param>
		/// <returns>The PolicyBuilder instance.</returns>
		public PolicyBuilder<TResult> HandleResult(Func<TResult, bool> resultPredicate)
		{
			if (resultPredicate == null)
				throw new ArgumentNullException(nameof(resultPredicate));

			bool predicate(TResult result) => resultPredicate(result);
			ResultPredicates.Add(predicate);
			return this;
		}

		/// <summary>
		/// Specifies a result value which the policy will handle.
		/// </summary>
		/// <param name="result">The TResult value this policy will handle.</param>
		/// <remarks>This policy filter matches the <paramref name="result"/> value returned using .Equals(), ideally suited for value types such as int and enum.  To match characteristics of class return types, consider the overload taking a result predicate.</remarks>
		/// <returns>The PolicyBuilder instance.</returns>
		public PolicyBuilder<TResult> HandleResult(TResult result)
			=> HandleResult(r => (r != null && r.Equals(result)) || (r == null && result == null));

		/// <summary>
		/// Builds a <see cref="Policy"/> that will retry <paramref name="retryCount"/> times.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <returns>The policy instance.</returns>
		public RetryPolicy<TResult> Retry(int retryCount)
		{
			return new RetryPolicy<TResult>(
				this,
				(result, timespan, i) => null,
				retryCount);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will retry <paramref name="retryCount"/> times
		/// calling <paramref name="onRetry"/> on each retry with the raised exception and retry count.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">onRetry</exception>
		public RetryPolicy<TResult> Retry(int retryCount, Action<DelegateResult<TResult>, int> onRetry)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy<TResult>(
				this,
				(result, timespan, i) =>
				{
					onRetry(result, i);
					return null;
				},
				retryCount);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will retry <paramref name="retryCount"/> times
		/// calling <paramref name="onRetry"/> on each retry with the raised exception and retry count.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">onRetry</exception>
		public RetryPolicy<TResult> Retry(int retryCount, Func<DelegateResult<TResult>, int, RetryResult?> onRetry)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy<TResult>(
				this,
				(result, timespan, i) => onRetry(result, i),
				retryCount);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will retry indefinitely until the action succeeds.
		/// </summary>
		/// <returns>The policy instance.</returns>
		public RetryPolicy<TResult> RetryUntilSucceeds()
		{
			return new RetryPolicy<TResult>(
				this,
				(result, timespan, i) => null);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will retry indefinitely
		/// calling <paramref name="onRetry"/> on each retry with the raised exception and retry count.
		/// </summary>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">onRetry</exception>
		public RetryPolicy<TResult> RetryUntilSucceeds(Action<DelegateResult<TResult>, int> onRetry)
		{
			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy<TResult>(
				this,
				(result, timespan, i) =>
				{
					onRetry(result, i);
					return null;
				});
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will retry indefinitely
		/// calling <paramref name="onRetry"/> on each retry with the raised exception and retry count.
		/// </summary>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">onRetry</exception>
		public RetryPolicy<TResult> RetryUntilSucceeds(Func<DelegateResult<TResult>, int, RetryResult?> onRetry)
		{
			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy<TResult>(
				this,
				(result, timespan, i) => onRetry(result, i));
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry <paramref name="retryCount"/> times.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		public RetryPolicy<TResult> WaitAndRetry(int retryCount, Func<int, DelegateResult<TResult>, TimeSpan> sleepDurationProvider)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			return new RetryPolicy<TResult>(
				this,
				(result, timespan, i) => null,
				retryCount,
				sleepDurationProvider: sleepDurationProvider);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry <paramref name="retryCount"/> times
		/// calling <paramref name="onRetry"/> on each retry with the raised exception, current sleep duration, retry count, and context data.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">
		/// timeSpanProvider
		/// or
		/// onRetry
		/// </exception>
		public RetryPolicy<TResult> WaitAndRetry(int retryCount, Func<int, DelegateResult<TResult>, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, int> onRetry)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy<TResult>(
				this,
				(ex, timespan, i) =>
				{
					onRetry(ex, timespan, i);
					return null;
				},
				retryCount,
				sleepDurationProvider: sleepDurationProvider);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry <paramref name="retryCount"/> times
		/// calling <paramref name="onRetry"/> on each retry with the raised exception, current sleep duration, retry count, and context data.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider"/> with
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">
		/// timeSpanProvider
		/// or
		/// onRetry
		/// </exception>
		public RetryPolicy<TResult> WaitAndRetry(int retryCount, Func<int, DelegateResult<TResult>, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, int, RetryResult?> onRetry)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy<TResult>(
				this,
				onRetry,
				retryCount,
				sleepDurationProvider: sleepDurationProvider);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry as many times as there are provided <paramref name="sleepDurations"/>
		/// On each retry, the duration to wait is the current <paramref name="sleepDurations"/> item.
		/// </summary>
		/// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
		/// <returns>The policy instance.</returns>
		public RetryPolicy<TResult> WaitAndRetry(IEnumerable<TimeSpan> sleepDurations)
		{
			if (sleepDurations == null) throw new ArgumentNullException(nameof(sleepDurations));

			return new RetryPolicy<TResult>(
				this,
				(result, timespan, i) => null,
				sleepDurationsEnumerable: sleepDurations);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry as many times as there are provided <paramref name="sleepDurations"/>
		/// calling <paramref name="onRetry"/> on each retry with the raised exception, current sleep duration, retry count and context data.
		/// On each retry, the duration to wait is the current <paramref name="sleepDurations"/> item.
		/// </summary>
		/// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">
		/// sleepDurations
		/// or
		/// onRetry
		/// </exception>
		public RetryPolicy<TResult> WaitAndRetry(IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan, int> onRetry)
		{
			if (sleepDurations == null)
				throw new ArgumentNullException(nameof(sleepDurations));

			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy<TResult>(
				this,
				(ex, timespan, i) =>
				{
					onRetry(ex, timespan, i);
					return null;
				},
				sleepDurationsEnumerable: sleepDurations);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry as many times as there are provided <paramref name="sleepDurations"/>
		/// calling <paramref name="onRetry"/> on each retry with the raised exception, current sleep duration, retry count and context data.
		/// On each retry, the duration to wait is the current <paramref name="sleepDurations"/> item.
		/// </summary>
		/// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">
		/// sleepDurations
		/// or
		/// onRetry
		/// </exception>
		public RetryPolicy<TResult> WaitAndRetry(IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, int, RetryResult?> onRetry)
		{
			if (sleepDurations == null)
				throw new ArgumentNullException(nameof(sleepDurations));

			if (onRetry == null)
				throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy<TResult>(
				this,
				onRetry,
				sleepDurationsEnumerable: sleepDurations);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry indefinitely until the action succeeds.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
		/// the current retry number (1 for first retry, 2 for second etc)
		/// </summary>
		/// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		public RetryPolicy<TResult> WaitAndRetryUntilSucceeds(Func<int, DelegateResult<TResult>, TimeSpan> sleepDurationProvider)
		{
			if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));

			return new RetryPolicy<TResult>(
				this,
				(result, timespan, i) => null,
				sleepDurationProvider: sleepDurationProvider);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry indefinitely until the action succeeds, 
		/// calling <paramref name="onRetry"/> on each retry with the raised exception.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
		/// the current retry number (1 for first retry, 2 for second etc)
		/// </summary>
		/// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		/// <exception cref="ArgumentNullException">onRetry</exception>
		public RetryPolicy<TResult> WaitAndRetryUntilSucceeds(Func<int, DelegateResult<TResult>, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, int, TimeSpan> onRetry)
		{
			if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
			if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy<TResult>(
				this,
				(result, timespan, i) =>
				{
					onRetry(result, i, timespan);
					return null;
				},
				sleepDurationProvider: sleepDurationProvider);
		}

		/// <summary>
		/// Builds a <see cref="Policy"/> that will wait and retry indefinitely until the action succeeds, 
		/// calling <paramref name="onRetry"/> on each retry with the raised exception.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
		/// the current retry number (1 for first retry, 2 for second etc)
		/// </summary>
		/// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
		/// <param name="onRetry">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		/// <exception cref="ArgumentNullException">onRetry</exception>
		public RetryPolicy<TResult> WaitAndRetryUntilSucceeds(Func<int, DelegateResult<TResult>, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, int, TimeSpan, RetryResult?> onRetry)
		{
			if (sleepDurationProvider == null) throw new ArgumentNullException(nameof(sleepDurationProvider));
			if (onRetry == null) throw new ArgumentNullException(nameof(onRetry));

			return new RetryPolicy<TResult>(
				this,
				(result, timespan, i) => onRetry(result, i, timespan),
				sleepDurationProvider: sleepDurationProvider);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry <paramref name="retryCount" /> times.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <returns>The policy instance.</returns>
		public AsyncRetryPolicy<TResult> RetryAsync(int retryCount)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			return new AsyncRetryPolicy<TResult>(
				this,
				(result, timespan, i) => Task.FromResult<RetryResult?>(null),
				retryCount);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry <paramref name="retryCount"/> times
		/// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and context data.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="onRetryAsync">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
		/// <exception cref="ArgumentNullException">onRetryAsync</exception>
		public AsyncRetryPolicy<TResult> RetryAsync(int retryCount, Func<DelegateResult<TResult>, int, Task> onRetryAsync)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy<TResult>(
				this,
				async (result, timespan, i) =>
				{
					await onRetryAsync(result, i).ConfigureAwait(false);
					return null;
				},
				retryCount
			);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry <paramref name="retryCount"/> times
		/// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and context data.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="onRetryAsync">The action to call on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than zero.</exception>
		/// <exception cref="ArgumentNullException">onRetryAsync</exception>
		public AsyncRetryPolicy<TResult> RetryAsync(int retryCount, Func<DelegateResult<TResult>, int, Task<RetryResult?>> onRetryAsync)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy<TResult>(
				this,
				(result, timespan, i) => onRetryAsync(result, i),
				retryCount
			);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will retry indefinitely until the action succeeds.
		/// </summary>
		/// <returns>The policy instance.</returns>
		public AsyncRetryPolicy<TResult> RetryUntilSucceedsAsync()
		{
			return new AsyncRetryPolicy<TResult>(
				this,
				(result, timespan, i) => Task.FromResult<RetryResult?>(null));
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry indefinitely
		/// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and context data.
		/// </summary>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">onRetryAsync</exception>
		public AsyncRetryPolicy<TResult> RetryUntilSucceedsAsync(Func<DelegateResult<TResult>, int, Task> onRetryAsync)
		{
			if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy<TResult>(
				this,
				async (result, timespan, i) =>
				{
					await onRetryAsync(result, i).ConfigureAwait(false);
					return null;
				});
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will retry indefinitely
		/// calling <paramref name="onRetryAsync"/> on each retry with the raised exception and context data.
		/// </summary>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">onRetryAsync</exception>
		public AsyncRetryPolicy<TResult> RetryUntilSucceedsAsync(Func<DelegateResult<TResult>, int, Task<RetryResult?>> onRetryAsync)
		{
			if (onRetryAsync == null) throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy<TResult>(
				this,
				(result, timespan, i) => onRetryAsync(result, i));
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		public AsyncRetryPolicy<TResult> WaitAndRetryAsync(int retryCount, Func<int, DelegateResult<TResult>, TimeSpan> sleepDurationProvider)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			return new AsyncRetryPolicy<TResult>(
				this,
				(result, timespan, i) => Task.FromResult<RetryResult?>(null),
				retryCount,
				sleepDurationProvider: sleepDurationProvider
			);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
		/// calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">
		/// sleepDurationProvider
		/// or
		/// onRetryAsync
		/// </exception>
		public AsyncRetryPolicy<TResult> WaitAndRetryAsync(int retryCount, Func<int, DelegateResult<TResult>, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, int, Task> onRetryAsync)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy<TResult>(
				this,
				async (result, timespan, i) =>
				{
					await onRetryAsync(result, timespan, i).ConfigureAwait(false);
					return null;
				},
				retryCount,
				sleepDurationProvider: sleepDurationProvider
			);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry <paramref name="retryCount" /> times
		/// calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">retryCount;Value must be greater than or equal to zero.</exception>
		/// <exception cref="ArgumentNullException">
		/// sleepDurationProvider
		/// or
		/// onRetryAsync
		/// </exception>
		public AsyncRetryPolicy<TResult> WaitAndRetryAsync(int retryCount, Func<int, DelegateResult<TResult>, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, int, Task<RetryResult?>> onRetryAsync)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount), "Value must be greater than or equal to zero.");

			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy<TResult>(
				this,
				onRetryAsync,
				retryCount,
				sleepDurationProvider: sleepDurationProvider
			);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided <paramref name="sleepDurations" />.
		/// On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
		/// </summary>
		/// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">sleepDurations</exception>
		public AsyncRetryPolicy<TResult> WaitAndRetryAsync(IEnumerable<TimeSpan> sleepDurations)
		{
			if (sleepDurations == null)
				throw new ArgumentNullException(nameof(sleepDurations));

			return new AsyncRetryPolicy<TResult>(
				this,
				(result, timespan, i) => Task.FromResult<RetryResult?>(null),
				sleepDurationsEnumerable: sleepDurations);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
		/// <paramref name="sleepDurations" />
		/// calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
		/// On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
		/// </summary>
		/// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">
		/// sleepDurations
		/// or
		/// onRetryAsync
		/// </exception>
		public AsyncRetryPolicy<TResult> WaitAndRetryAsync(IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, int, Task> onRetryAsync)
		{
			if (sleepDurations == null)
				throw new ArgumentNullException(nameof(sleepDurations));
			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy<TResult>(
				this,
				async (result, timespan, i) =>
				{
					await onRetryAsync(result, timespan, i).ConfigureAwait(false);
					return null;
				},
				sleepDurationsEnumerable: sleepDurations);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}" /> that will wait and retry as many times as there are provided
		/// <paramref name="sleepDurations" />
		/// calling <paramref name="onRetryAsync" /> on each retry with the raised exception, the current sleep duration, retry count, and context data.
		/// On each retry, the duration to wait is the current <paramref name="sleepDurations" /> item.
		/// </summary>
		/// <param name="sleepDurations">The sleep durations to wait for on each retry.</param>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">
		/// sleepDurations
		/// or
		/// onRetryAsync
		/// </exception>
		public AsyncRetryPolicy<TResult> WaitAndRetryAsync(IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, int, Task<RetryResult?>> onRetryAsync)
		{
			if (sleepDurations == null)
				throw new ArgumentNullException(nameof(sleepDurations));
			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy<TResult>(
				this,
				onRetryAsync,
				sleepDurationsEnumerable: sleepDurations);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		public AsyncRetryPolicy<TResult> WaitAndRetryUntilSucceedsAsync(Func<int, DelegateResult<TResult>, TimeSpan> sleepDurationProvider)
		{
			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			return new AsyncRetryPolicy<TResult>(
				this,
				(result, timespan, i) => Task.FromResult<RetryResult?>(null),
				sleepDurationProvider: sleepDurationProvider
			);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely
		/// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and execution context.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		/// <exception cref="ArgumentNullException">onRetryAsync</exception>
		public AsyncRetryPolicy<TResult> WaitAndRetryUntilSucceedsAsync(Func<int, DelegateResult<TResult>, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, int, TimeSpan, Task> onRetryAsync)
		{
			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy<TResult>(
				this,
				async (exception, timespan, i) =>
				{
					await onRetryAsync(exception, i, timespan).ConfigureAwait(false);
					return null;
				},
				sleepDurationProvider: sleepDurationProvider
			);
		}

		/// <summary>
		/// Builds an <see cref="AsyncRetryPolicy{TResult}"/> that will wait and retry indefinitely
		/// calling <paramref name="onRetryAsync"/> on each retry with the raised exception, retry count and execution context.
		/// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with 
		/// the current retry number (1 for first retry, 2 for second etc), previous exception and execution context.
		/// </summary>
		/// <param name="sleepDurationProvider">A function providing the duration to wait before retrying.</param>
		/// <param name="onRetryAsync">The action to call asynchronously on each retry.</param>
		/// <returns>The policy instance.</returns>
		/// <exception cref="ArgumentNullException">sleepDurationProvider</exception>
		/// <exception cref="ArgumentNullException">onRetryAsync</exception>
		public AsyncRetryPolicy<TResult> WaitAndRetryUntilSucceedsAsync(Func<int, DelegateResult<TResult>, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, int, TimeSpan, Task<RetryResult?>> onRetryAsync)
		{
			if (sleepDurationProvider == null)
				throw new ArgumentNullException(nameof(sleepDurationProvider));

			if (onRetryAsync == null)
				throw new ArgumentNullException(nameof(onRetryAsync));

			return new AsyncRetryPolicy<TResult>(
				this,
				(exception, timespan, i) => onRetryAsync(exception, i, timespan),
				sleepDurationProvider: sleepDurationProvider
			);
		}

		/// <summary>
		/// Builds a <see cref="FallbackPolicy"/> which provides a fallback value if the main execution fails.  Executes the main delegate, but if this throws a handled exception or raises a handled result, first calls <paramref name="onFallback"/> with details of the handled exception or result and the execution context; then calls <paramref name="fallbackAction"/> and returns its result.  
		/// </summary>
		/// <param name="fallbackAction">The fallback action.</param>
		/// <param name="onFallback">The action to call before invoking the fallback delegate.</param>
		/// <exception cref="ArgumentNullException">fallbackAction</exception>
		/// <exception cref="ArgumentNullException">onFallback</exception>
		/// <returns>The policy instance.</returns>
		public FallbackPolicy<TResult> Fallback(Func<DelegateResult<TResult>, CancellationToken, TResult> fallbackAction, Action<DelegateResult<TResult>> onFallback)
		{
			if (fallbackAction == null)
				throw new ArgumentNullException(nameof(fallbackAction));

			if (onFallback == null)
				throw new ArgumentNullException(nameof(onFallback));

			return new FallbackPolicy<TResult>(this,  onFallback, fallbackAction);
		}

		/// <summary>
		/// Builds an <see cref="AsyncFallbackPolicy{TResult}"/> which provides a fallback value if the main execution fails.  Executes the main delegate asynchronously, but if this throws a handled exception or raises a handled result, first asynchronously calls <paramref name="onFallbackAsync"/> with details of the handled exception or result and the execution context; then asynchronously calls <paramref name="fallbackAction"/> and returns its result.  
		/// </summary>
		/// <param name="fallbackAction">The fallback delegate.</param>
		/// <param name="onFallbackAsync">The action to call asynchronously before invoking the fallback delegate.</param>
		/// <exception cref="ArgumentNullException">fallbackAction</exception>
		/// <exception cref="ArgumentNullException">onFallbackAsync</exception>
		/// <returns>The policy instance.</returns>
		public AsyncFallbackPolicy<TResult> FallbackAsync(Func<DelegateResult<TResult>, CancellationToken, Task<TResult>> fallbackAction, Func<DelegateResult<TResult>, Task> onFallbackAsync)
		{
			if (fallbackAction == null)
				throw new ArgumentNullException(nameof(fallbackAction));

			if (onFallbackAsync == null)
				throw new ArgumentNullException(nameof(onFallbackAsync));

			return new AsyncFallbackPolicy<TResult>(this, onFallbackAsync, fallbackAction);
		}
	}
}
