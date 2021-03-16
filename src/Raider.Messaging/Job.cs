﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.Localization;
using Raider.Messaging.Messages;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public abstract class Job : IJob, IDisposable
	{
		private Timer? _timer;
		private CancellationTokenSource? _stoppingCts;
		private readonly bool _stopTimerOnExecute = true;
		private ILogger? _logger;

		public int IdComponent { get; }
		public bool Initialized { get; private set; }
		public string Name { get; }
		public int IdScenario { get; }
		public ComponentState State { get; private set; }

		public abstract Dictionary<int, TimeSpan>? DelayTable { get; set; } //Dictionary<retryCount, Delay>
		protected abstract TimeSpan DelayedStart { get; }
		protected abstract TimeSpan ExecuteInterval { get; set; }

		private IServiceProvider? _serviceProvider;
		internal IMessageBox? MessageBox { get; private set; }
		public IServiceBusRegister? Register { get; set; }

		public Job(int idSubscriber, string name)
			: this(idSubscriber, name, 0)
		{
		}

		public Job(int idSubscriber, string name, int idScenario)
		{
			IdComponent = idSubscriber;
			Name = string.IsNullOrWhiteSpace(name)
				? throw new ArgumentNullException(nameof(name))
				: name;
			IdScenario = idScenario;
		}

		public abstract Task<MessageResult> ExecuteAsync(SubscriberContext context, CancellationToken token = default);

		Task IJob.InitializeAsync(IServiceProvider serviceProvider, IMessageBox messageBox, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
			=> InitializeAsync(serviceProvider, messageBox, loggerFactory, cancellationToken);

		internal async Task InitializeAsync(IServiceProvider serviceProvider, IMessageBox messageBox, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			MessageBox = messageBox ?? throw new ArgumentNullException(nameof(messageBox));

			if (loggerFactory == null)
				throw new ArgumentNullException(nameof(loggerFactory));

			_logger = loggerFactory.CreateLogger(GetType());

			Initialized = true;

			State = ComponentState.Idle;
			_stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			await MessageBox.SetComponentStateAsync(this, State, null, _stoppingCts.Token);
			_timer = new Timer(TimerCallback, null, DelayedStart, ExecuteInterval);
			_logger?.LogTrace($"{GetType().FullName}: Started timer");
		}

		private async void TimerCallback(object? state)
		{
			if (_stopTimerOnExecute)
				StopTimer();

			if (_serviceProvider == null)
				throw new InvalidOperationException($"{nameof(_serviceProvider)} == null");

			if (MessageBox == null)
				throw new InvalidOperationException($"{nameof(MessageBox)} == null");

			try
			{
				_logger?.LogTrace($"{GetType().FullName}: Timer ticks");

				try
				{
					State = ComponentState.InProcess;
					await MessageBox.SetComponentStateAsync(this, State, null, _stoppingCts?.Token ?? default);

					MessageResult messageResult;
					using (var scope = _serviceProvider.CreateScope())
					{
						var tc = scope.ServiceProvider.GetService<TraceContext>();

						var subscriberContext = scope.ServiceProvider.GetRequiredService<SubscriberContext>();
						subscriberContext.TraceInfo = new TraceInfoBuilder(TraceFrame.Create(), tc?.Next()).Build();
						subscriberContext.Logger = _logger ?? scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
						subscriberContext.ApplicationResources = scope.ServiceProvider.GetRequiredService<IApplicationResources>();

						messageResult = await ExecuteAsync(subscriberContext, _stoppingCts?.Token ?? default);
					}

					if (messageResult.State == MessageState.Suspended)
					{
						State = ComponentState.Suspended;
						await MessageBox.SetComponentStateAsync(this, State, null, _stoppingCts?.Token ?? default);
					}
				}
				catch (Exception ex)
				{
					State = ComponentState.Error;
					await MessageBox.SetComponentStateAsync(this, State, null, _stoppingCts?.Token ?? default);
					_logger?.LogError(ex, $"{GetType().FullName}: Execute error {nameof(IdComponent)}: {IdComponent}");
				}
			}
			catch (Exception ex)
			{
				State = ComponentState.Error;
				await MessageBox.SetComponentStateAsync(this, State, null, _stoppingCts?.Token ?? default);

				_logger?.LogError(ex, $"{GetType().FullName}: TimerCallback error");
			}
			finally
			{
				if (_stopTimerOnExecute)
					StartTimer();
			}
		}

		private bool StartTimer()
			=> _timer?.Change(ExecuteInterval, ExecuteInterval) ?? false;

		private bool StopTimer()
			=> _timer?.Change(Timeout.Infinite, Timeout.Infinite) ?? false;

		private bool disposed;
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					_timer?.Dispose();
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