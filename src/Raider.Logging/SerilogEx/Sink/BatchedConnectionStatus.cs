//using System;

//namespace Raider.Logging.SerilogEx.Sink
//{
//	internal class BatchedConnectionStatus
//	{
//		private readonly TimeSpan _minimumBackoffPeriod;
//		private readonly TimeSpan _maximumBackoffInterval;

//		private const int FailuresBeforeDroppingBatch = 8;
//		private const int FailuresBeforeDroppingQueue = 10;

//		private readonly TimeSpan _period;

//		private int _failuresSinceSuccessfulBatch;

//		public BatchedConnectionStatus(IBatchedPeriodOptions periodOptions)
//		{
//			if (periodOptions == null)
//				throw new ArgumentNullException(nameof(periodOptions));

//			if (periodOptions.Period < TimeSpan.Zero)
//				throw new ArgumentOutOfRangeException(nameof(periodOptions.Period), "The batching period must be a positive timespan");

//			_period = periodOptions.Period;

//			_minimumBackoffPeriod = periodOptions.MinimumBackoffPeriod <= TimeSpan.Zero
//				? TimeSpan.FromSeconds(5)
//				: periodOptions.MinimumBackoffPeriod;

//			_maximumBackoffInterval = periodOptions.MaximumBackoffInterval <= TimeSpan.Zero
//				? TimeSpan.FromMinutes(10)
//				: periodOptions.MaximumBackoffInterval;
//		}

//		public void MarkSuccess()
//		{
//			_failuresSinceSuccessfulBatch = 0;
//		}

//		public void MarkFailure()
//		{
//			++_failuresSinceSuccessfulBatch;
//		}

//		public TimeSpan NextInterval
//		{
//			get
//			{
//				// Available, and first failure, just try the batch interval
//				if (_failuresSinceSuccessfulBatch <= 1)
//					return _period;

//				// Second failure, start ramping up the interval - first 2x, then 4x, ...
//				var backoffFactor = Math.Pow(2, (_failuresSinceSuccessfulBatch - 1));

//				// If the period is ridiculously short, give it a boost so we get some
//				// visible backoff.
//				var backoffPeriod = Math.Max(_period.Ticks, _minimumBackoffPeriod.Ticks);

//				// The "ideal" interval
//				var backedOff = (long)(backoffPeriod * backoffFactor);

//				// Capped to the maximum interval
//				var cappedBackoff = Math.Min(_maximumBackoffInterval.Ticks, backedOff);

//				// Unless that's shorter than the period, in which case we'll just apply the period
//				var actual = Math.Max(_period.Ticks, cappedBackoff);

//				return TimeSpan.FromTicks(actual);
//			}
//		}

//		public bool ShouldDropBatch => _failuresSinceSuccessfulBatch >= FailuresBeforeDroppingBatch;

//		public bool ShouldDropQueue => _failuresSinceSuccessfulBatch >= FailuresBeforeDroppingQueue;
//	}
//}
