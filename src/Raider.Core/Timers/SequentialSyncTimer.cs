using System;

namespace Raider.Timers
{
	public class SequentialSyncTimer : BaseSequentialSyncTimer
	{
		private readonly Action<object?> _timerCallback;
		private readonly Action<object?, Exception>? _exceptionCallback;

		public SequentialSyncTimer(
			object? state,
			TimeSpan timerInterval,
			Action<object?> timerCallback,
			Action<object?, Exception>? exceptionCallback = null)
			: base(state, timerInterval)
		{
			_timerCallback = timerCallback ?? throw new ArgumentNullException(nameof(timerCallback));
			_exceptionCallback = exceptionCallback;
		}

		public SequentialSyncTimer(
			object? state,
			TimeSpan startDelay,
			TimeSpan timerInterval,
			Action<object?> timerCallback,
			Action<object?, Exception>? exceptionCallback = null)
			: base(state, startDelay, timerInterval)
		{
			_timerCallback = timerCallback ?? throw new ArgumentNullException(nameof(timerCallback));
			_exceptionCallback = exceptionCallback;
		}

		protected override void OnTimer(object? state)
			=> _timerCallback(state);

		protected override void OnError(object? state, Exception ex)
			=> _exceptionCallback?.Invoke(state, ex);

		public static SequentialSyncTimer Start(
			object? state,
			TimeSpan timerInterval,
			Action<object?> timerCallback,
			Action<object?, Exception>? exceptionCallback = null)
		{
			var timer = new SequentialSyncTimer(state, timerInterval, timerCallback, exceptionCallback);
			timer.Start();
			return timer;
		}

		public static SequentialSyncTimer Start(
			object? state,
			TimeSpan startDelay,
			TimeSpan timerInterval,
			Action<object?> timerCallback,
			Action<object?, Exception>? exceptionCallback = null)
		{
			var timer = new SequentialSyncTimer(state, startDelay, timerInterval, timerCallback, exceptionCallback);
			timer.Start();
			return timer;
		}
	}
}
