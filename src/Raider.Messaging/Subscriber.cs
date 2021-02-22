﻿using Microsoft.Extensions.Logging;
using Raider.Messaging.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public abstract class Subscriber<TData> : ISubscriber<TData>, IDisposable
			where TData : IMessageData
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
		public Type SubscribeMessageDataType { get; } = typeof(TData);

		public abstract bool ReadMessagesFromSequentialIFIFO { get; }
		public abstract int MessageProcessRetryCount { get; }
		public abstract TimeSpan MessageInProcessTimeout { get; set; }
		protected abstract TimeSpan DelayedStart { get; set; }
		protected abstract TimeSpan ExecuteInterval { get; set; }

		public IServiceBusRegister? Register { get; set; }
		internal IMessageBox? MessageBox { get; private set; }

		public Subscriber(int idSubscriber, string name)
			: this(idSubscriber, name, 0)
		{
		}

		public Subscriber(int idSubscriber, string name, int idScenario)
		{
			IdComponent = idSubscriber;
			Name = string.IsNullOrWhiteSpace(name)
				? throw new ArgumentNullException(nameof(name))
				: name;
			IdScenario = idScenario;
		}

		public abstract Task<SubscribedMessageResult> ProcessMessageAsync(ISubscriberMessage<TData> message, CancellationToken token = default);

		Task ISubscriber.InitializeAsync(IMessageBox messageBox, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
			=> InitializeAsync(messageBox, loggerFactory, cancellationToken);

		internal async Task InitializeAsync(IMessageBox messageBox, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
		{
			MessageBox = messageBox ?? throw new ArgumentNullException(nameof(messageBox));

			if (loggerFactory == null)
				throw new ArgumentNullException(nameof(loggerFactory));

			_logger = loggerFactory.CreateLogger(GetType());

			Initialized = true;

			State = ComponentState.Idle;
			_stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			await MessageBox.SetSubscriberStateAsync(this, State, _stoppingCts.Token);
			_timer = new Timer(TimerCallback, null, DelayedStart, ExecuteInterval);
			_logger?.LogInformation($"{GetType().FullName}: Started timer");
		}

		private async void TimerCallback(object? state)
		{
			if (_stopTimerOnExecute)
				StopTimer();

			if (MessageBox == null)
				throw new InvalidOperationException($"{nameof(MessageBox)} == null");

			try
			{
				_logger?.LogInformation($"{GetType().FullName}: Timer ticks");

				var message = ReadMessagesFromSequentialIFIFO
					? await MessageBox.GetFirstSubscriberMessageAsync(this, _stoppingCts?.Token ?? default)
					: await MessageBox.GetNextSubscriberMessageAsync(this, _stoppingCts?.Token ?? default);
				if (message == null)
				{
					State = ComponentState.Idle;
					await MessageBox.SetSubscriberStateAsync(this, State, _stoppingCts?.Token ?? default);
					return;
				}

				try
				{
					State = ComponentState.InProcess;
					await MessageBox.SetSubscriberStateAsync(this, State, _stoppingCts?.Token ?? default);
					var messageResult = await ProcessMessageAsync(message, _stoppingCts?.Token ?? default);
					if (messageResult.IsValidFor(message))
					{
						await MessageBox.SetMessageStateAsync(message, messageResult);

						if (ReadMessagesFromSequentialIFIFO && messageResult.State == SubscriberMessageState.Suspended)
						{
							State = ComponentState.Suspended;
							await MessageBox.SetSubscriberStateAsync(this, State, _stoppingCts?.Token ?? default);
							await MessageBox.SetSubscriberStateAsync(this, State, _stoppingCts?.Token ?? default);
						}
					}
				}
				catch (Exception ex)
				{
					State = ComponentState.Error;
					await MessageBox.SetSubscriberStateAsync(this, State, _stoppingCts?.Token ?? default);
					_logger?.LogError(ex, $"{GetType().FullName}: ProcessMessage error {nameof(message.IdSubscriberMessage)}: {message.IdSubscriberMessage}");
					await MessageBox.SetMessageStateAsync(message, SubscriberMessageState.InProcess, message.RetryCount + 1, null); //TODO vypocitaj RetryCount a delayedToUtc
				}
			}
			catch (Exception ex)
			{
				State = ComponentState.Error;
				await MessageBox.SetSubscriberStateAsync(this, State, _stoppingCts?.Token ?? default);

				_logger?.LogError(ex, $"{GetType().FullName}: TimerCallback error");
			}
			finally
			{
				if (_stopTimerOnExecute)
					StartTimer();
			}
		}

		private bool StartTimer()
			=> _timer?.Change(TimeSpan.Zero, ExecuteInterval) ?? false;

		private bool StopTimer()
			=> _timer?.Change(Timeout.Infinite, 0) ?? false;

		public virtual void Dispose()
		{
			_timer?.Dispose();
		}
	}
}
