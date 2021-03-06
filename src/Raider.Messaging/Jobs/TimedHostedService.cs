﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.Jobs
{
	public abstract class TimedHostedService : IHostedService, IDisposable
	{
		private readonly ILogger? _logger;

		private Timer? _timer;
		private Task? _executingTask;
		private CancellationTokenSource? _stoppingCts;

		protected Task? ExecutingTask => _executingTask;
		protected ILogger? Logger => _logger;
		protected abstract bool StopTimerOnExecute { get; set; }
		protected abstract TimeSpan DelayedExecuteStart { get; set; }
		protected abstract TimeSpan ExecuteInterval { get; set; }

		public TimedHostedService(ILogger logger)
		{
			_logger = logger;
		}

		protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

		protected bool StartTimer()
			=> _timer?.Change(ExecuteInterval, ExecuteInterval) ?? false;

		private bool StopTimer()
			=> _timer?.Change(Timeout.Infinite, Timeout.Infinite) ?? false;

		public virtual Task StartAsync(CancellationToken cancellationToken)
		{
			_stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			_timer = new Timer(TimerCallback, null, DelayedExecuteStart, ExecuteInterval);
			_logger?.LogInformation($"{GetType().FullName}: Started timer");
			return Task.CompletedTask;
		}

		private void TimerCallback(object? state)
		{
			if (StopTimerOnExecute)
				StopTimer();

			if (_executingTask == null || _executingTask.IsCompleted)
			{
				_logger?.LogInformation($"{GetType().FullName}: No task is running, check for new job");

				_executingTask = ExecuteAsync(_stoppingCts?.Token ?? default);
			}
			else
			{
				_logger?.LogInformation($"{GetType().FullName}: There is a task still running, wait for next cycle");
			}
		}

		public virtual async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger?.LogInformation($"{GetType().FullName}: Initiate graceful shutdown");
			
			StopTimer();

			if (_executingTask == null || _executingTask.IsCompleted)
			{
				return;
			}

			try
			{
				_stoppingCts?.Cancel();
			}
			finally
			{
				await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
			}
		}

		private bool disposed;
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					_timer?.Dispose();
					_stoppingCts?.Cancel();
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
