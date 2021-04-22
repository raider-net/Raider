using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.Extensions;
using Raider.Localization;
using Raider.Messaging.Messages;
using Raider.Threading;
using Raider.Trace;
using System;
using System.Collections.Generic;
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
		private ILogger? _fallbackLogger;

		/// <summary>
		/// IdSubscriberInstance
		/// </summary>
		public int IdComponent { get; }

		/// <summary>
		/// IdSubscriberInstance
		/// </summary>
		public Guid IdInstance { get; } = Guid.NewGuid();
		public bool Initialized { get; private set; }
		public bool Started { get; private set; }
		public string Name { get; }
		public string? Description { get; set; }
		public int IdScenario { get; }
		public DateTime LastActivityUtc { get; private set; }
		public ComponentState State { get; private set; }
		public Type SubscribingMessageDataType { get; } = typeof(TData);

		public abstract bool ReadMessagesFromSequentialFIFO { get; }
		public abstract int MaxMessageProcessingRetryCount { get; }
		public abstract TimeSpan TimeoutForMessageProcessing { get; set; }
		protected abstract Dictionary<int, TimeSpan>? DelayTable { get; set; } //Dictionary<retryCount, Delay>
		protected abstract TimeSpan DefaultDelaTimeStamp { get; set; }
		protected abstract TimeSpan DelayedStart { get; }
		protected abstract TimeSpan ExecuteInterval { get; set; }
		protected virtual bool AllowMessageReadAcceleration { get; set; } = true;
		protected abstract bool DropExpiredMessages { get; set; }

		private IServiceProvider? _serviceProvider;
		internal IServiceBusStorage? Storage { get; set; }
		internal IMessageBox? MessageBox { get; private set; }
		public IServiceBusRegister? Register { get; set; }

		public Subscriber(int idSubscriber, string name)
			: this(idSubscriber, name, 0)
		{
		}

		public Subscriber(int idSubscriber, string name, int idScenario)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				var traceInfo = TraceInfo.Create();
				Serilog.Log.Logger.Error($"{traceInfo}: {nameof(name)} == null");
				throw new ArgumentNullException(nameof(name));
			}
			Name = name;

			IdComponent = idSubscriber;
			IdScenario = idScenario;
			LastActivityUtc = DateTime.UtcNow;
			State = ComponentState.Offline;
		}

		public abstract Task<MessageResult> ProcessMessageAsync(SubscriberContext context, ISubscriberMessage<TData> message, CancellationToken token = default);

		private readonly AsyncLock _initLock = new AsyncLock();
		async Task ISubscriber.InitializeAsync(IServiceProvider serviceProvider, IServiceBusStorage storage, IMessageBox messageBox, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
		{
			var appCtxTraceInfo = storage?.ServiceBusHost?.ApplicationContext.TraceInfo;
			var traceInfo = TraceInfo.Create(appCtxTraceInfo?.Principal, appCtxTraceInfo?.RuntimeUniqueKey);

			if (Initialized)
			{
				var error = "Already initialized.";

				await LogErrorAsync(
					traceInfo,
					nameof(ServiceBusDefaults.LogMessageType.Init),
					error, null, true, cancellationToken);

				throw new InvalidOperationException(error);
			}

			using (await _initLock.LockAsync())
			{
				if (Initialized)
				{
					var error = "Already initialized.";

					await LogErrorAsync(
						traceInfo,
						nameof(ServiceBusDefaults.LogMessageType.Init),
						error, null, true, cancellationToken);

					throw new InvalidOperationException(error);
				}

				if (loggerFactory == null)
				{
					Serilog.Log.Logger.Error($"{traceInfo}: {nameof(loggerFactory)} == null");
					throw new ArgumentNullException(nameof(loggerFactory));
				}

				try
				{
					_fallbackLogger = loggerFactory.CreateLogger<ServiceBusHostService>();
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, $"{traceInfo}: {nameof(loggerFactory.CreateLogger)}");
					throw;
				}

				if (serviceProvider == null)
				{
					ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(serviceProvider)} == null", null);
					throw new ArgumentNullException(nameof(serviceProvider));
				}
				_serviceProvider = serviceProvider;

				if (storage == null)
				{
					ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(storage)} == null", null);
					throw new ArgumentNullException(nameof(storage));
				}
				Storage = storage;

				if (messageBox == null)
				{
					ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(messageBox)} == null", null);
					throw new ArgumentNullException(nameof(messageBox));
				}
				MessageBox = messageBox;

				Initialized = true;
			}

		}

		async Task IComponent.StartAsync(IServiceBusStorageContext context, CancellationToken cancellationToken)
		{
			var appCtxTraceInfo = Storage?.ServiceBusHost?.ApplicationContext.TraceInfo;
			var traceInfo = TraceInfo.Create(appCtxTraceInfo?.Principal, appCtxTraceInfo?.RuntimeUniqueKey);
			LastActivityUtc = DateTime.UtcNow;

			if (Storage == null || !Initialized)
			{
				var error = "Not initialized.";

				await LogErrorAsync(
					traceInfo,
					nameof(ServiceBusDefaults.LogMessageType.Start),
					error, null, true, cancellationToken);

				throw new InvalidOperationException(error);
			}

			try
			{
				await Storage.WriteSubscriberStartAsync(context, this, cancellationToken);

				_stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
				_timer = new Timer(TimerCallback, null, DelayedStart, ExecuteInterval);
			}
			catch (Exception ex)
			{
				ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(Storage)}.{nameof(Storage.WriteSubscriberStartAsync)}", ex);
				throw;
			}

			Started = true;
		}

		private async void TimerCallback(object? state)
		{
			if (_stopTimerOnExecute || State == ComponentState.Suspended)
				StopTimer();

			if (State == ComponentState.Suspended)
				return;

			var appCtxTraceInfo = Storage?.ServiceBusHost?.ApplicationContext.TraceInfo;
			var traceInfo = TraceInfo.Create(appCtxTraceInfo?.Principal, appCtxTraceInfo?.RuntimeUniqueKey);
			var nowUtc = DateTime.UtcNow;
			LastActivityUtc = nowUtc;

			if (_serviceProvider == null)
			{
				var error = $"{nameof(_serviceProvider)} == null";

				await LogErrorAsync(
					traceInfo,
					nameof(ServiceBusDefaults.LogMessageType.TimerTics),
					error, null, true);

				throw new InvalidOperationException(error);
			}

			if (MessageBox == null)
			{
				var error = $"{nameof(MessageBox)} == null";

				await LogErrorAsync(
					traceInfo,
					nameof(ServiceBusDefaults.LogMessageType.TimerTics),
					error, null, true);

				throw new InvalidOperationException(error);
			}

			var timerNextInterval = ExecuteInterval;

			try
			{
				var message = ReadMessagesFromSequentialFIFO
					? await MessageBox.GetSubscriberMessageFromFIFOAsync(
						this,
						new List<int>
						{
							(int)MessageState.Pending,
							//(int)MessageState.InProcess, - automatically picked up if last access timedout
							(int)MessageState.Error,
							(int)MessageState.Expired
						},
						nowUtc,
						_stoppingCts?.Token ?? default)
					: await MessageBox.GetSubscriberMessageFromNonFIFOAsync(
						this,
						new List<int>
						{
							(int)MessageState.Pending,
							//(int)MessageState.InProcess, - automatically picked up if last access timedout
							(int)MessageState.Error,
							(int)MessageState.Suspended,
							(int)MessageState.Corrupted,
							(int)MessageState.Expired
						},
						nowUtc,
						_stoppingCts?.Token ?? default);

				if (message == null)
					return;

				try
				{
					if (DropExpiredMessages && message.State == MessageState.Expired)
					{
						await LogMessageStateAsync(traceInfo, MessageResult.Consume(message), _stoppingCts?.Token ?? default);

						//skratime interval nacitania novej spravy na 1s
						if (AllowMessageReadAcceleration && timerNextInterval == ExecuteInterval && 1 < ExecuteInterval.TotalSeconds)
							timerNextInterval = TimeSpan.FromSeconds(1);

						return;
					}

					if (ReadMessagesFromSequentialFIFO)
					{
						if (message.State == MessageState.InProcess && message.LastAccessUtc.HasValue)
						{
							//ak este neuplynul timeout od posledneho spracovania spravy, tak sa uspi
							if (nowUtc < message.LastAccessUtc.Value.Add(TimeoutForMessageProcessing))
							{
								timerNextInterval = message.LastAccessUtc.Value.Subtract(nowUtc);
								return;
							}
						}

						if (message.DelayedToUtc.HasValue && nowUtc < message.DelayedToUtc.Value)
						{
							//ak este neuplynul DelayedToUtc, tak sa uspi
							timerNextInterval = message.DelayedToUtc.Value.Subtract(nowUtc);
							return;
						}

						if (message.State == MessageState.Suspended || message.State == MessageState.Corrupted)
						{
							await LogActivityAsync(traceInfo, ComponentState.Suspended, null, null, _stoppingCts?.Token ?? default);
							return;
						}
					}

					await LogActivityAsync(traceInfo, ComponentState.InProcess, null, null, _stoppingCts?.Token ?? default);

					MessageResult messageResult;
					using (var scope = _serviceProvider.CreateScope())
					{
						var appCtx = scope.ServiceProvider.GetRequiredService<IApplicationContext>();

						var subscriberContext = scope.ServiceProvider.GetRequiredService<SubscriberContext>();
						subscriberContext.TraceInfo = new TraceInfoBuilder(TraceFrame.Create(), appCtx.Next()).Build();
						subscriberContext.Logger = _fallbackLogger ?? scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

						messageResult = await ProcessMessageAsync(subscriberContext, message, _stoppingCts?.Token ?? default);
					}

					if (ReadMessagesFromSequentialFIFO && messageResult.State == MessageState.Suspended)
					{
						await LogActivityAsync(traceInfo, ComponentState.Suspended, messageResult, null, _stoppingCts?.Token ?? default);
					}
					else
					{
						await LogMessageStateAsync(traceInfo, messageResult, _stoppingCts?.Token ?? default);

						//ak nejaka sprava bola uspesne spracovana, skratime interval nacitania novej spravy na 1s
						if (AllowMessageReadAcceleration && timerNextInterval == ExecuteInterval && 1 < ExecuteInterval.TotalSeconds)
							timerNextInterval = TimeSpan.FromSeconds(1);
					}
				}
				catch (Exception ex)
				{
					await LogActivityAsync(
						traceInfo,
						ComponentState.Error,
						MessageResult.Error(message, DelayTable, DefaultDelaTimeStamp == TimeSpan.Zero ? TimeSpan.FromMinutes(5) : DefaultDelaTimeStamp),
						new LogError(traceInfo, nameof(TimerCallback), $"{nameof(ProcessMessageAsync)}", ex.ToStringTrace())
						{
							IdSubscriberMessage = message.IdSubscriberMessage
						},
						_stoppingCts?.Token ?? default);
				}
			}
			catch (Exception ex)
			{
				await LogActivityAsync(
					traceInfo,
					ComponentState.Error,
					null,
					new LogError(traceInfo, nameof(TimerCallback), "Global Error", ex.ToStringTrace()),
					_stoppingCts?.Token ?? default);
			}
			finally
			{
				try
				{
					if (State == ComponentState.InProcess)
					{
						await LogActivityAsync(traceInfo, ComponentState.Idle, null, null, _stoppingCts?.Token ?? default);
					}
				}
				catch { }

				if (_stopTimerOnExecute && State != ComponentState.Suspended)
					StartTimer(timerNextInterval);
			}
		}

		private async Task LogErrorAsync(ITraceInfo? traceInfo, string logMessageType, string message, Exception? ex, bool writeErrorActivity, CancellationToken cancellationToken = default)
		{
			if (traceInfo == null)
				traceInfo = TraceInfo.Create();

			if (!Started || Storage == null)
			{
				ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, message, ex);
			}
			else
			{
				try
				{
					if (writeErrorActivity && (State != ComponentState.Error || State != ComponentState.Suspended))
					{
						await LogActivityAsync(
							traceInfo,
							ComponentState.Error,
							null,
							new LogError(traceInfo, logMessageType, message, ex?.ToStringTrace()),
							cancellationToken);
					}
					else
					{
						await Storage.WriteSubscriberLogAsync(
							this,
							new LogError(traceInfo, logMessageType, message, ex?.ToStringTrace()),
							cancellationToken);
					}
				}
				catch (Exception storageEx)
				{
					ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, message, ex);
					ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(IServiceBusStorage)}.{nameof(Storage.WriteSubscriberLogAsync)}", storageEx);
					throw;
				}
			}
		}

		private async Task LogActivityAsync(ITraceInfo? traceInfo, ComponentState state, MessageResult? messageResult, LogBase? log, CancellationToken cancellationToken = default)
		{
			var originalState = State;
			State = state;
			LastActivityUtc = DateTime.UtcNow;

			try
			{
				if (Storage == null)
					throw new InvalidOperationException($"{nameof(Storage)} == null");

				await Storage.WriteSubscriberActivityAsync(
					this,
					messageResult,
					log,
					cancellationToken);
			}
			catch (Exception ex)
			{
				await LogErrorAsync(traceInfo, $"{nameof(LogActivityAsync)}", $"{nameof(IServiceBusStorage)}.{nameof(Storage.WriteSubscriberActivityAsync)}", ex, false, cancellationToken);

				if (state != ComponentState.Error || state != ComponentState.Suspended)
				{
					await LogActivityAsync(
						traceInfo,
						ComponentState.Error,
						messageResult,
						new LogError(traceInfo, $"{nameof(LogActivityAsync)}_Reset", $"Original state = {originalState} | Failed state = {State} | Final state = {ComponentState.Error}"),
						cancellationToken);
				}

				throw;
			}
		}

		private async Task LogMessageStateAsync(ITraceInfo? traceInfo, MessageResult messageResult, CancellationToken cancellationToken = default)
		{
			try
			{
				if (Storage == null)
					throw new InvalidOperationException($"{nameof(Storage)} == null");

				await Storage.WriteMessageStateAsync(
					this,
					messageResult,
					cancellationToken);
			}
			catch (Exception ex)
			{
				await LogErrorAsync(traceInfo, $"{nameof(LogMessageStateAsync)}", $"{nameof(IServiceBusStorage)}.{nameof(Storage.WriteMessageStateAsync)}", ex, false, cancellationToken);
				throw;
			}
		}

		private bool StartTimer(TimeSpan interval)
			=> _timer?.Change(interval, interval) ?? false;

		private bool StopTimer()
			=> _timer?.Change(Timeout.Infinite, Timeout.Infinite) ?? false;

		public async Task<bool> Resume(CancellationToken cancellationToken = default)
		{
			var appCtxTraceInfo = Storage?.ServiceBusHost?.ApplicationContext.TraceInfo;
			var traceInfo = TraceInfo.Create(appCtxTraceInfo?.Principal, appCtxTraceInfo?.RuntimeUniqueKey);
			LastActivityUtc = DateTime.UtcNow;

			await LogActivityAsync(traceInfo, ComponentState.Idle, null, null, _stoppingCts?.Token ?? default);
			return _timer?.Change(TimeSpan.Zero, ExecuteInterval) ?? false;
		}

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
