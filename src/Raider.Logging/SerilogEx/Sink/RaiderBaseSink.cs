using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Logging.SerilogEx.Sink
{
	public abstract class RaiderBaseSink : ILogEventSink, IDisposable
	{
		/// <summary>
		/// Constant used to indicate that the internal queue shouldn't be limited.
		/// </summary>
		public const int NoQueueLimit = BoundedConcurrentQueue<LogEvent>.Unbounded;

		private readonly int _batchSizeLimit;
		private readonly bool _eagerlyEmitFirstEvent;
		private readonly BoundedConcurrentQueue<LogEvent> _queue;
		private readonly BatchedConnectionStatus _status;
		private readonly Queue<LogEvent> _waitingBatch = new Queue<LogEvent>();

		private readonly object _stateLock = new object();

		private readonly PortableTimer _timer;

		private bool _unloading;
		private bool _started;

		protected RaiderBaseSink(RaiderBatchSinkOptions? options)
		{
			if (options == null)
				options = new RaiderBatchSinkOptions();

			if (options.BatchSizeLimit <= 0)
				throw new ArgumentOutOfRangeException(nameof(options), "The batch size limit must be greater than zero.");

			if (options.Period <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(options), "The period must be greater than zero.");

			if (options.MinimumBackoffPeriod <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(options), "The MinimumBackoffPeriod must be greater than zero.");

			if (options.MaximumBackoffInterval <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(options), "The MaximumBackoffInterval must be greater than zero.");

			_batchSizeLimit = options.BatchSizeLimit;
			_queue = new BoundedConcurrentQueue<LogEvent>(options.QueueLimit);
			_status = new BatchedConnectionStatus(options);
			_eagerlyEmitFirstEvent = options.EagerlyEmitFirstEvent;
			_timer = new PortableTimer(cancel => OnTick());
		}

		public virtual bool Include(LogEvent logEvent) => true;

		public abstract Task WriteBatch(IEnumerable<LogEvent> batch);

		private void SetTimer(TimeSpan interval)
		{
			_timer.Start(interval);
		}

		/// <summary>
		/// Emit the provided log event to the sink. If the sink is being disposed or
		/// the app domain unloaded, then the event is ignored.
		/// </summary>
		/// <param name="logEvent">Log event to emit.</param>
		/// <exception cref="ArgumentNullException">The event is null.</exception>
		/// <remarks>
		/// The sink implements the contract that any events whose Emit() method has
		/// completed at the time of sink disposal will be flushed (or attempted to,
		/// depending on app domain state).
		/// </remarks>
		public void Emit(LogEvent logEvent)
		{
			if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

			if (_unloading)
				return;

			if (!_started)
			{
				lock (_stateLock)
				{
					if (_unloading) return;
					if (!_started)
					{
						_queue.TryEnqueue(logEvent);
						_started = true;

						if (_eagerlyEmitFirstEvent)
						{
							// Special handling to try to get the first event across as quickly
							// as possible to show we're alive!
							SetTimer(TimeSpan.Zero);
						}
						else
						{
							SetTimer(_status.NextInterval);
						}

						return;
					}
				}
			}

			_queue.TryEnqueue(logEvent);
		}

		private async Task OnTick()
		{
			try
			{
				bool batchWasFull;
				do
				{
					while (_waitingBatch.Count < _batchSizeLimit && _queue.TryDequeue(out LogEvent? next))
					{
						if (next != null && Include(next))
							_waitingBatch.Enqueue(next);
					}

					if (_waitingBatch.Count == 0)
						return;

					await WriteBatch(_waitingBatch);

					batchWasFull = _waitingBatch.Count >= _batchSizeLimit;
					_waitingBatch.Clear();
					_status.MarkSuccess();
				}
				while (batchWasFull); // Otherwise, allow the period to elapse
			}
			catch (Exception ex)
			{
				SelfLog.WriteLine("Exception while emitting periodic batch from {0}: {1}", this, ex);
				_status.MarkFailure();
			}
			finally
			{
				if (_status.ShouldDropBatch)
					_waitingBatch.Clear();

				if (_status.ShouldDropQueue)
				{
					while (_queue.TryDequeue(out _)) { }
				}

				lock (_stateLock)
				{
					if (!_unloading)
						SetTimer(_status.NextInterval);
				}
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// Free resources held by the sink.
		/// </summary>
		/// <param name="disposing">If true, called because the object is being disposed; if false,
		/// the object is being disposed from the finalizer.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing) return;
			CloseAndFlush();
		}

		private void CloseAndFlush()
		{
			lock (_stateLock)
			{
				if (!_started || _unloading)
					return;

				_unloading = true;
			}

			_timer.Dispose();

			// This is the place where SynchronizationContext.Current is unknown and can be != null
			// so we prevent possible deadlocks here for sync-over-async downstream implementations 
			ResetSyncContextAndWait(OnTick);
		}

		private static void ResetSyncContextAndWait(Func<Task> taskFactory)
		{
			var prevContext = SynchronizationContext.Current;
			SynchronizationContext.SetSynchronizationContext(null);
			try
			{
				taskFactory().Wait();
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(prevContext);
			}
		}
	}
}
