using System;
using System.Threading;

namespace Raider.Timers
{
	public abstract class SequentialSyncTimer
	{
		protected TimeSpan TimerInterval { get; set; }
		protected readonly Timer _timer;

		public SequentialSyncTimer(object? state)
		{
			_timer = new Timer(TimerCallback, state, Timeout.Infinite, Timeout.Infinite);
		}

		private bool _started;
		private readonly object _startLock = new();
		protected virtual void StartInternal()
		{
			if (_started)
				throw new InvalidOperationException($"{GetType().Name} already started.");

			lock (_startLock)
			{
				if (_started)
					throw new InvalidOperationException($"{GetType().Name} already started.");

				StartTimerImmediately();
				_started = true;
			}
		}

		private void TimerCallback(object? state)
		{
			StopTimer();

			try
			{
				OnTimer(state);
			}
			catch (Exception ex)
			{
				OnError(state, ex);
			}
			finally
			{
				StartTimer();
			}
		}

		protected abstract void OnTimer(object? state);
		protected abstract void OnError(object? state, Exception ex);

		protected virtual bool StartTimer()
			=> _timer.Change(TimerInterval, Timeout.InfiniteTimeSpan);

		protected virtual bool StartTimerImmediately()
			=> _timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);

		protected virtual bool StopTimer()
			=> _timer.Change(Timeout.Infinite, Timeout.Infinite);
	}
}
