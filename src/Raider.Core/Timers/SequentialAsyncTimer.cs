using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Timers
{
	public abstract class SequentialAsyncTimer
	{
		protected TimeSpan TimerInterval { get; set; }
		protected readonly Timer _timer;

		public SequentialAsyncTimer(object? state)
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

		private async void TimerCallback(object? state)
		{
			StopTimer();

			try
			{
				await OnTimer(state);
			}
			catch (Exception ex)
			{
				await OnError(state, ex);
			}
			finally
			{
				StartTimer();
			}
		}

		protected abstract Task OnTimer(object? state);
		protected abstract Task OnError(object? state, Exception ex);

		protected virtual bool StartTimer()
			=> _timer.Change(TimerInterval, Timeout.InfiniteTimeSpan);

		protected virtual bool StartTimerImmediately()
			=> _timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);

		protected virtual bool StopTimer()
			=> _timer.Change(Timeout.Infinite, Timeout.Infinite);
	}
}
