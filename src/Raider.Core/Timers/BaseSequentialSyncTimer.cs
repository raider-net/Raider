using System;
using System.Threading;

namespace Raider.Timers
{
	public abstract class BaseSequentialSyncTimer
	{
		protected TimeSpan StartDelay { get; set; }
		protected TimeSpan TimerInterval { get; set; }
		protected readonly Timer _timer;

		public BaseSequentialSyncTimer(object? state, TimeSpan timerInterval)
			: this(state, TimeSpan.Zero, timerInterval)
		{
		}

		public BaseSequentialSyncTimer(object? state, TimeSpan startDelay, TimeSpan timerInterval)
		{
			StartDelay = startDelay;
			TimerInterval = timerInterval;
			_timer = new Timer(TimerCallback, state, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
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
			=> _started
				? _timer.Change(TimerInterval, Timeout.InfiniteTimeSpan)
				: Start();

		protected virtual bool StopTimer()
			=> _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
	}
}
