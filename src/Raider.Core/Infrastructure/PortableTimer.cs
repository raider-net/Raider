using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Infrastructure
{
	public class PortableTimer : IDisposable
	{
		private readonly object _stateLock = new object();

		private readonly Func<CancellationToken, Task> _onTick;
		private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
		private readonly Timer _timer;
		private readonly Action<string, object?, object?, object?>? _errorLogger;

		private bool _running;
		private bool _disposed;

		public PortableTimer(Func<CancellationToken, Task> onTick, Action<string, object?, object?, object?>? errorLogger = null) // errorLogger = Action<format, arg0, arg1, arg2>
		{
			_onTick = onTick ?? throw new ArgumentNullException(nameof(onTick));
			_timer = new Timer(_ => OnTick(), null, Timeout.Infinite, Timeout.Infinite);
			_errorLogger = errorLogger;
		}

		public void Start(TimeSpan interval)
		{
			if (interval < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval));

			lock (_stateLock)
			{
				if (_disposed)
					throw new ObjectDisposedException(nameof(PortableTimer));

				_timer.Change(interval, Timeout.InfiniteTimeSpan);
			}
		}

		private async void OnTick()
		{
			try
			{
				lock (_stateLock)
				{
					if (_disposed)
					{
						return;
					}

					// There's a little bit of raciness here, but it's needed to support the
					// current API, which allows the tick handler to reenter and set the next interval.

					if (_running)
					{
						Monitor.Wait(_stateLock);

						if (_disposed)
						{
							return;
						}
					}

					_running = true;
				}

				if (!_cancel.Token.IsCancellationRequested)
				{
					await _onTick(_cancel.Token);
				}
			}
			catch (OperationCanceledException tcx)
			{
				_errorLogger?.Invoke("The timer was canceled during invocation: {0}", tcx, null, null);
			}
			finally
			{
				lock (_stateLock)
				{
					_running = false;
					Monitor.PulseAll(_stateLock);
				}
			}
		}

		private bool disposed;
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_cancel.Cancel();

					lock (_stateLock)
					{
						if (_disposed)
						{
							return;
						}

						while (_running)
						{
							Monitor.Wait(_stateLock);
						}

						_timer.Dispose();
						_disposed = true;
					}
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
