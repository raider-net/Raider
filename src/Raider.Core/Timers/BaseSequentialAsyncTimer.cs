using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Timers
{
	public abstract class BaseSequentialAsyncTimer
	{
		protected TimeSpan StartDelay { get; set; }
		protected TimeSpan TimerInterval { get; set; }
		protected readonly Timer _timer;

		public BaseSequentialAsyncTimer(object? state, TimeSpan timerInterval)
			: this(state, TimeSpan.Zero, timerInterval)
		{
		}

		public BaseSequentialAsyncTimer(object? state, TimeSpan startDelay, TimeSpan timerInterval)
		{
			StartDelay = startDelay;
			TimerInterval = timerInterval;
			_timer = new Timer(TimerCallbackAsync, state, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
		}

		private bool _started;
		private readonly object _startLock = new();
		public virtual bool Start()
		{
			if (_started)
				throw new InvalidOperationException($"{GetType().Name} already started.");

			lock (_startLock)
			{
				if (_started)
					throw new InvalidOperationException($"{GetType().Name} already started.");

				//Start timer
				var result = _timer.Change(StartDelay, Timeout.InfiniteTimeSpan);
				_started = true;
				return result;
			}
		}

		public virtual void Stop()
		{
			if (!_started)
				return;

			lock (_startLock)
			{
				if (!_started)
					return;

				//Stop timer
				_timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
				_started = false;
			}
		}

		public virtual void Restart()
		{
			lock (_startLock)
			{
				_timer.Change(StartDelay, Timeout.InfiniteTimeSpan);
				_started = true;
			}
		}

		private async void TimerCallbackAsync(object? state)
		{
			StopTimer();

			try
			{
				await OnTimerAsync(state).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await OnErrorAsync(state, ex).ConfigureAwait(false);
			}
			finally
			{
				StartTimer();
			}
		}

		protected abstract Task OnTimerAsync(object? state);
		protected abstract Task OnErrorAsync(object? state, Exception ex);

		protected virtual bool StartTimer()
			=> _started
				? _timer.Change(TimerInterval, Timeout.InfiniteTimeSpan)
				: Start();

		protected virtual bool StopTimer()
			=> _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
	}
}
