using Raider.Policy.Internal;
using System;

namespace Raider.Policy
{
	public class Policy
	{
		internal ExceptionPredicates ExceptionPredicates { get; }

		internal Policy(ExceptionPredicates? exceptionPredicates)
		{
			ExceptionPredicates = exceptionPredicates ?? ExceptionPredicates.None;
		}

		internal Policy(PolicyBuilder? policyBuilder)
			: this(policyBuilder?.ExceptionPredicates)
		{
		}

		/// <summary>
		/// Handle all exception types.
		/// </summary>
		/// <returns>The PolicyBuilder instance.</returns>
		public static PolicyBuilder HandleAllExceptions()
			=> new(exception => exception);

		/// <summary>
		/// Specifies the type of exception that this policy can handle.
		/// </summary>
		/// <typeparam name="TException">The type of the exception to handle.</typeparam>
		/// <returns>The PolicyBuilder instance.</returns>
		public static PolicyBuilder HandleException<TException>() where TException : Exception
			=> new(exception => exception is TException ? exception : null);

		/// <summary>
		/// Specifies the type of exception that this policy can handle with additional filters on this exception type.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
		/// <returns>The PolicyBuilder instance.</returns>
		public static PolicyBuilder HandleException<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception
			=> new(exception => (exception is TException texception && (exceptionPredicate == null || exceptionPredicate(texception))) ? exception : null);

		/// <summary>
		/// Specifies the type of exception that this policy can handle if found as an InnerException of a regular <see cref="Exception"/>, or at any level of nesting within an <see cref="AggregateException"/>.
		/// </summary>
		/// <typeparam name="TException">The type of the exception to handle.</typeparam>
		/// <returns>The PolicyBuilder instance.</returns>
		public static PolicyBuilder HandleInnerException<TException>() where TException : Exception
			=> new(PolicyBuilder.HandleInner(ex => ex is TException));

		/// <summary>
		/// Specifies the type of exception that this policy can handle, with additional filters on this exception type, if found as an InnerException of a regular <see cref="Exception"/>, or at any level of nesting within an <see cref="AggregateException"/>.
		/// </summary>
		/// <typeparam name="TException">The type of the exception to handle.</typeparam>
		/// <param name="exceptionPredicate">The inner exception predicate to filter the type of exception this policy can handle.</param>
		/// <returns>The PolicyBuilder instance.</returns>
		public static PolicyBuilder HandleInnerException<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception
			=> new(PolicyBuilder.HandleInner(ex => ex is TException texception && (exceptionPredicate == null || exceptionPredicate(texception))));

		/// <summary>
		/// Specifies the type of return result that this policy can handle with additional filters on the result.
		/// </summary>
		/// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
		/// <param name="resultPredicate">The predicate to filter results this policy will handle.</param>
		/// <returns>The PolicyBuilder instance.</returns>
		public static PolicyBuilder<TResult> HandleResult<TResult>(Func<TResult, bool> resultPredicate)
			=> new(resultPredicate);

		/// <summary>
		/// Specifies the type of return result that this policy can handle, and a result value which the policy will handle.
		/// </summary>
		/// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
		/// <param name="result">The TResult value this policy will handle.</param>
		/// <remarks>This policy filter matches the <paramref name="result"/> value returned using .Equals(), ideally suited for value types such as int and enum.  To match characteristics of class return types, consider the overload taking a result predicate.</remarks>
		/// <returns>The PolicyBuilder instance.</returns>
		public static PolicyBuilder<TResult> HandleResult<TResult>(TResult result)
			=> HandleResult(new Func<TResult, bool>(r => (r != null && r.Equals(result)) || (r == null && result == null)));
	}

	public class Policy<TResult> : Policy
	{
		internal ResultPredicates<TResult> ResultPredicates { get; }

		internal Policy(ExceptionPredicates? exceptionPredicates, ResultPredicates<TResult>? resultPredicates)
			: base(exceptionPredicates)
		{
			ResultPredicates = resultPredicates ?? ResultPredicates<TResult>.None;
		}

		internal Policy(PolicyBuilder<TResult>? policyBuilder)
			: this(policyBuilder?.ExceptionPredicates, policyBuilder?.ResultPredicates)
		{
		}

		/// <summary>
		/// Handle all exception types.
		/// </summary>
		/// <returns>The PolicyBuilder instance.</returns>
		public static new PolicyBuilder<TResult> HandleAllExceptions()
			=> new(exception => exception);

		/// <summary>
		/// Specifies the type of exception that this policy can handle.
		/// </summary>
		/// <typeparam name="TException">The type of the exception to handle.</typeparam>
		/// <returns>The PolicyBuilder instance.</returns>
		public static new PolicyBuilder<TResult> HandleException<TException>() where TException : Exception
			=> new(exception => exception is TException ? exception : null);

		/// <summary>
		/// Specifies the type of exception that this policy can handle with additional filters on this exception type.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
		/// <returns>The PolicyBuilder instance.</returns>
		public static new PolicyBuilder<TResult> HandleException<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception
			=> new(exception => (exception is TException texception && (exceptionPredicate == null || exceptionPredicate(texception))) ? exception : null);

		/// <summary>
		/// Specifies the type of exception that this policy can handle if found as an InnerException of a regular <see cref="Exception"/>, or at any level of nesting within an <see cref="AggregateException"/>.
		/// </summary>
		/// <typeparam name="TException">The type of the exception to handle.</typeparam>
		/// <returns>The PolicyBuilder instance.</returns>
		public static new PolicyBuilder<TResult> HandleInnerException<TException>() where TException : Exception
			=> new(PolicyBuilder.HandleInner(ex => ex is TException));

		/// <summary>
		/// Specifies the type of exception that this policy can handle, with additional filters on this exception type, if found as an InnerException of a regular <see cref="Exception"/>, or at any level of nesting within an <see cref="AggregateException"/>.
		/// </summary>
		/// <typeparam name="TException">The type of the exception to handle.</typeparam>
		/// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
		/// <returns>The PolicyBuilder instance.</returns>
		public static new PolicyBuilder<TResult> HandleInnerException<TException>(Func<TException, bool> exceptionPredicate) where TException : Exception
			=> new(PolicyBuilder.HandleInner(ex => ex is TException texception && (exceptionPredicate == null || exceptionPredicate(texception))));

		/// <summary>
		/// Specifies a filter on the return result values that this strongly-typed generic policy will handle.
		/// </summary>
		/// <param name="resultPredicate">The predicate to filter the results this policy will handle.</param>
		/// <returns>The PolicyBuilder instance.</returns>
		public static PolicyBuilder<TResult> HandleResult(Func<TResult, bool> resultPredicate)
			=> new(resultPredicate);

		/// <summary>
		/// Specifies a return result value which the strongly-typed generic policy will handle.
		/// </summary>
		/// <param name="result">The TResult value this policy will handle.</param>
		/// <remarks>This policy filter matches the <paramref name="result"/> value returned using .Equals(), ideally suited for value types such as int and enum.  To match characteristics of class return types, consider the overload taking a result predicate.</remarks>
		/// <returns>The PolicyBuilder instance.</returns>
		public static PolicyBuilder<TResult> HandleResult(TResult result)
			=> HandleResult(r => (r != null && r.Equals(result)) || (r == null && result == null));
	}
}
