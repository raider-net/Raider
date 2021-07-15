/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raider.Policy
{
	public interface IPolicyOptionsBuilder<TBuilder, TObject>
		where TBuilder : IPolicyOptionsBuilder<TBuilder, TObject>
		where TObject : IPolicyOptions
	{
		TBuilder Object(TObject policyOptions);
		TObject Build();



		TBuilder RetryIf(Func<Exception, int, TimeSpan, bool> condition);

		TBuilder WaitAndRetry(Dictionary<int, TimeSpan> sleepDurations);

		TBuilder WaitAndRetry(Dictionary<int, TimeSpan> sleepDurations, Action<Exception, int, TimeSpan> onBeforeRetry);

		TBuilder WaitAndRetryIf(Func<Exception, int, TimeSpan, TimeSpan?> sleepDurationProvider);





		//TBuilder HandledExcpetion<TException>()
		//	where TException : Exception;

		//TBuilder HandledExcpetion<TException>(Func<TException, bool> condition)
		//	where TException : Exception;

		//TBuilder HandledResult<R>(Func<R, bool> condition);

		//TBuilder Retry(int count);

		//TBuilder Retry(int count, Action<Exception, int> onBeforeRetry);

		//TBuilder RetryUntilSucceeds();

		//TBuilder RetryUntilSucceeds(Action<Exception, int> onBeforeRetry);

		//TBuilder WaitAndRetry(IEnumerable<TimeSpan> sleepDurations);

		//TBuilder WaitAndRetry(IEnumerable<TimeSpan> sleepDurations, Action<Exception, int, TimeSpan> onBeforeRetry);

		//TBuilder WaitAndRetry(int retryCount, Func<Exception, int, TimeSpan> sleepDurationProvider);

		//TBuilder WaitAndRetry(int retryCount, Func<Exception, int, TimeSpan> sleepDurationProvider, Action<Exception, int, TimeSpan> onBeforeRetry);

		//TBuilder WaitAndRetryUntilSucceeds(Func<Exception, int, TimeSpan> sleepDurationProvider);

		//TBuilder WaitAndRetryUntilSucceeds(Func<Exception, int, TimeSpan> sleepDurationProvider, Action<Exception, int, TimeSpan> onBeforeRetry);

		//TBuilder Fallback(Action<Exception> fallbackAction);
	}

	public abstract class PolicyOptionsBuilderBase<TBuilder, TObject> : IPolicyOptionsBuilder<TBuilder, TObject>
		where TBuilder : PolicyOptionsBuilderBase<TBuilder, TObject>
		where TObject : IPolicyOptions
	{
		protected readonly TBuilder _builder;
		protected TObject _PolicyOptions;

		protected PolicyOptionsBuilderBase(TObject policyOptions)
		{
			_PolicyOptions = policyOptions;
			_builder = (TBuilder)this;
		}

		public virtual TBuilder Object(TObject policyOptions)
		{
			_PolicyOptions = policyOptions ?? throw new ArgumentNullException(nameof(policyOptions));

			return _builder;
		}

		public TObject Build()
			=> _PolicyOptions;

		public TBuilder HandledExcpetion<TException>() where TException : Exception
		{
			throw new NotImplementedException();
		}

		public TBuilder HandledExcpetion<TException>(Func<TException, bool> condition) where TException : Exception
		{
			throw new NotImplementedException();
		}

		public TBuilder HandledResult<R>(Func<R, bool> condition)
		{
			throw new NotImplementedException();
		}

		public TBuilder Retry(int count)
		{
			throw new NotImplementedException();
		}

		public TBuilder Retry(int count, Action<Exception, int> onBeforeRetry)
		{
			throw new NotImplementedException();
		}

		public TBuilder RetryIf(Func<Exception, int, TimeSpan, bool> condition)
		{
			throw new NotImplementedException();
		}

		public TBuilder RetryUntilSucceeds()
		{
			throw new NotImplementedException();
		}

		public TBuilder RetryUntilSucceeds(Action<Exception, int> onBeforeRetry)
		{
			throw new NotImplementedException();
		}

		public TBuilder WaitAndRetry(IEnumerable<TimeSpan> sleepDurations)
		{
			throw new NotImplementedException();
		}

		public TBuilder WaitAndRetry(IEnumerable<TimeSpan> sleepDurations, Action<Exception, int, TimeSpan> onBeforeRetry)
		{
			throw new NotImplementedException();
		}

		public TBuilder WaitAndRetry(Dictionary<int, TimeSpan> sleepDurations)
		{
			throw new NotImplementedException();
		}

		public TBuilder WaitAndRetry(Dictionary<int, TimeSpan> sleepDurations, Action<Exception, int, TimeSpan> onBeforeRetry)
		{
			throw new NotImplementedException();
		}

		public TBuilder WaitAndRetry(int retryCount, Func<Exception, int, TimeSpan> sleepDurationProvider)
		{
			throw new NotImplementedException();
		}

		public TBuilder WaitAndRetry(int retryCount, Func<Exception, int, TimeSpan> sleepDurationProvider, Action<Exception, int, TimeSpan> onBeforeRetry)
		{
			throw new NotImplementedException();
		}

		public TBuilder WaitAndRetryUntilSucceeds(Func<Exception, int, TimeSpan> sleepDurationProvider)
		{
			throw new NotImplementedException();
		}

		public TBuilder WaitAndRetryUntilSucceeds(Func<Exception, int, TimeSpan> sleepDurationProvider, Action<Exception, int, TimeSpan> onBeforeRetry)
		{
			throw new NotImplementedException();
		}

		public TBuilder WaitAndRetryIf(Func<Exception, int, TimeSpan, TimeSpan?> sleepDurationProvider)
		{
			throw new NotImplementedException();
		}

		public TBuilder Fallback(Action<Exception> fallbackAction)
		{
			throw new NotImplementedException();
		}
	}

	public class PolicyOptionsBuilder : PolicyOptionsBuilderBase<PolicyOptionsBuilder, PolicyOptions>
	{
		public PolicyOptionsBuilder()
			: this(new PolicyOptions())
		{
		}

		public PolicyOptionsBuilder(PolicyOptions policyOptions)
			: base(policyOptions)
		{
		}

		public static implicit operator PolicyOptions?(PolicyOptionsBuilder builder)
		{
			if (builder == null)
				return null;

			return builder._PolicyOptions;
		}

		public static implicit operator PolicyOptionsBuilder?(PolicyOptions policyOptions)
		{
			if (policyOptions == null)
				return null;

			return new PolicyOptionsBuilder(policyOptions);
		}
	}

	public interface IPolicyOptionsBuilder<TBuilder, T, TObject> : IPolicyOptionsBuilder<TBuilder, TObject>
		where TBuilder : IPolicyOptionsBuilder<TBuilder, T, TObject>
		where TObject : IPolicyOptions<T>
	{




	}

	public abstract class PolicyOptionsBuilderBase<TBuilder, T, TObject> : PolicyOptionsBuilderBase<TBuilder, TObject>, IPolicyOptionsBuilder<TBuilder, T, TObject>
		where TBuilder : PolicyOptionsBuilderBase<TBuilder, T, TObject>
		where TObject : IPolicyOptions<T>
	{
		protected PolicyOptionsBuilderBase(TObject policyOptions)
			: base(policyOptions)
		{
		}
	}

	public class PolicyOptionsBuilder<T> : PolicyOptionsBuilderBase<PolicyOptionsBuilder<T>, T, IPolicyOptions<T>>
	{
		public PolicyOptionsBuilder()
			: this(new PolicyOptions<T>())
		{
		}

		public PolicyOptionsBuilder(PolicyOptions<T> policyOptions)
			: base(policyOptions)
		{
		}

		public static implicit operator PolicyOptions<T>?(PolicyOptionsBuilder<T> builder)
		{
			if (builder == null)
				return null;

			return builder._PolicyOptions as PolicyOptions<T>;
		}

		public static implicit operator PolicyOptionsBuilder<T>?(PolicyOptions<T> policyOptions)
		{
			if (policyOptions == null)
				return null;

			return new PolicyOptionsBuilder<T>(policyOptions);
		}

		public static implicit operator PolicyOptionsBuilder?(PolicyOptionsBuilder<T> builder)
		{
			if (builder == null)
				return null;

			return new PolicyOptionsBuilder((PolicyOptions<T>)builder._PolicyOptions);
		}
	}
}
*/
