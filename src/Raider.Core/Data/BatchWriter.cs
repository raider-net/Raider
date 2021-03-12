using Raider.Collections;
using Raider.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Data
{
	public class BatchWriter<T> : IBatchWriter<T>, IDisposable
	{
		/// <summary>
		/// Constant used to indicate that the internal queue shouldn't be limited.
		/// </summary>
		public const int NoQueueLimit = BoundedConcurrentQueue<T>.Unbounded;

		private readonly int _batchSizeLimit;
		private readonly bool _eagerlyEmitFirstEvent;
		private readonly BoundedConcurrentQueue<T> _queue;
		private readonly BatchedConnectionStatus _status;
		private readonly Queue<T> _waitingBatch = new Queue<T>();
		private readonly Action<string, object?, object?, object?>? _errorLogger;

		private readonly Func<T, bool>? _includeCallBack;
		private readonly Func<IEnumerable<T>, CancellationToken, Task>? _writeBatchCallback;

		private readonly object _stateLock = new object();

		private readonly PortableTimer _timer;

		private bool _unloading;
		private bool _started;

		public BatchWriter(
			IBatchWriterOptions? options,
			Action<string, object?, object?, object?>? errorLogger = null) // errorLogger = Action<format, arg0, arg1, arg2>
		{
			if (options == null)
				options = new BatchWriterOptions();

			if (options.BatchSizeLimit <= 0)
				throw new ArgumentOutOfRangeException(nameof(options), "The batch size limit must be greater than zero.");

			if (options.Period <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(options), "The period must be greater than zero.");

			if (options.MinimumBackoffPeriod <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(options), "The MinimumBackoffPeriod must be greater than zero.");

			if (options.MaximumBackoffInterval <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(options), "The MaximumBackoffInterval must be greater than zero.");

			_errorLogger = errorLogger;
			_batchSizeLimit = options.BatchSizeLimit;
			_queue = new BoundedConcurrentQueue<T>(options.QueueLimit);
			_status = new BatchedConnectionStatus(options);
			_eagerlyEmitFirstEvent = options.EagerlyEmitFirstEvent;
			_timer = new PortableTimer(cancel => OnTick());
		}

		public BatchWriter(
			Func<T, bool>? includeCallBack,
			Func<IEnumerable<T>, CancellationToken, Task>? writeBatchCallback,
			BatchWriterOptions? options,
			Action<string, object?, object?, object?>? errorLogger = null) // errorLogger = Action<format, arg0, arg1, arg2>
			: this(options, errorLogger)
		{
			_includeCallBack = includeCallBack;
			_writeBatchCallback = writeBatchCallback;
		}

		public virtual bool Include(T obj)
		{
			if (_includeCallBack == null)
				return true;

			return _includeCallBack(obj);
		}

		protected virtual Task WriteBatch(IEnumerable<T> batch, CancellationToken cancellationToken = default)
		{
			if (_writeBatchCallback == null)
				throw new InvalidOperationException($"{nameof(_writeBatchCallback)} == null");

			return _writeBatchCallback(batch, cancellationToken);
		}

		private void SetTimer(TimeSpan interval)
		{
			_timer.Start(interval);
		}

		/// <summary>
		/// Emit the provided log event to the sink. If the sink is being disposed or
		/// the app domain unloaded, then the event is ignored.
		/// </summary>
		/// <param name="obj">Log event to emit.</param>
		/// <exception cref="ArgumentNullException">The event is null.</exception>
		/// <remarks>
		/// The sink implements the contract that any events whose Emit() method has
		/// completed at the time of sink disposal will be flushed (or attempted to,
		/// depending on app domain state).
		/// </remarks>
		public void Write(T obj)
		{
			if (obj == null) throw new ArgumentNullException(nameof(obj));

			if (_unloading)
				return;

			if (!_started)
			{
				lock (_stateLock)
				{
					if (_unloading) return;
					if (!_started)
					{
						_queue.TryEnqueue(obj);
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

			_queue.TryEnqueue(obj);
		}

		private async Task OnTick()
		{
			try
			{
				bool batchWasFull;
				do
				{
					while (_waitingBatch.Count < _batchSizeLimit && _queue.TryDequeue(out T? next))
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
				_errorLogger?.Invoke("Exception while emitting periodic batch from {0}: {1}", this, ex, null);
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

		private bool disposed;
		/// <summary>
		/// Free resources held by the sink.
		/// </summary>
		/// <param name="disposing">If true, called because the object is being disposed; if false,
		/// the object is being disposed from the finalizer.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					CloseAndFlush();
				}

				disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
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
