using System;
using System.Threading.Tasks;

namespace Raider.Timers
{
	public class SequentialAsyncTimer : BaseSequentialAsyncTimer
	{
		private readonly Func<object?, Task> _timerCallback;
		private readonly Func<object?, Exception, Task>? _exceptionCallback;

		public SequentialAsyncTimer(
			object? state,
			TimeSpan timerInterval,
			Func<object?, Task> timerCallback,
			Func<object?, Exception, Task>? exceptionCallback = null)
			: base(state, timerInterval)
		{
			_timerCallback = timerCallback ?? throw new ArgumentNullException(nameof(timerCallback));
			_exceptionCallback = exceptionCallback;
		}

		public SequentialAsyncTimer(
			object? state,
			TimeSpan startDelay,
			TimeSpan timerInterval,
			Func<object?, Task> timerCallback,
			Func<object?, Exception, Task>? exceptionCallback = null)
			: base(state, startDelay, timerInterval)
		{
			_timerCallback = timerCallback ?? throw new ArgumentNullException(nameof(timerCallback));
			_exceptionCallback = exceptionCallback;
		}

		protected override Task OnTimerAsync(object? state)
			=> _timerCallback(state);

		protected override Task OnErrorAsync(object? state, Exception ex)
			=> _exceptionCallback?.Invoke(state, ex) ?? Task.CompletedTask;

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
