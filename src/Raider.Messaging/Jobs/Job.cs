using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.Extensions;
using Raider.Localization;
using Raider.Logging.Extensions;
using Raider.Messaging.Messages;
using Raider.Threading;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public abstract class Job : ComponentBase, IJob, IComponent, IDisposable
	{
		private Timer? _timer;
		private CancellationTokenSource? _stoppingCts;
		private readonly bool _stopTimerOnExecute = true;
		private ILogger? _fallbackLogger;

		/// <summary>
		/// IdJob
		/// </summary>
		public override sealed int IdComponent { get; }

		/// <summary>
		/// IdJobInstance
		/// </summary>
		public override sealed Guid IdInstance { get; } = Guid.NewGuid();
		public override sealed bool Initialized { get; protected set; }
		public override sealed bool Started { get; protected set; }
		public override sealed string Name { get; }
		public override sealed string? Description { get; set; }
		public override sealed int IdScenario { get; }
		public override sealed DateTime LastActivityUtc { get; protected set; }
		public override sealed ComponentState State { get; protected set; }

		protected abstract Dictionary<int, TimeSpan>? DelayTable { get; set; } //Dictionary<retryCount, Delay>
		protected abstract TimeSpan DelayedStart { get; }
		protected abstract TimeSpan ExecuteInterval { get; set; }

		private IServiceProvider? _serviceProvider;
		internal IServiceBusStorage? Storage { get; set; }
		internal IMessageBox? MessageBox { get; private set; }
		public IServiceBusRegister? Register { get; set; }
		public override sealed IReadOnlyDictionary<object, object> ServiceBusHostProperties => Storage?.ServiceBusHost?.Properties ?? new ReadOnlyDictionary<object, object>(new Dictionary<object, object>());

		public Job(int idSubscriber, string name)
			: this(idSubscriber, name, 0)
		{
		}

		public Job(int idJob, string name, int idScenario)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				var traceInfo = TraceInfo.Create();
				Serilog.Log.Logger.Error($"{traceInfo}: {nameof(name)} == null");
				throw new ArgumentNullException(nameof(name));
			}
			Name = name;

			IdComponent = idJob;
			IdScenario = idScenario;
			LastActivityUtc = DateTime.UtcNow;
			State = ComponentState.Offline;
		}

		public abstract Task<ComponentState> ExecuteAsync(JobContext context, CancellationToken cancellationToken = default);

		Task IJob.InitializeAsync(IServiceProvider serviceProvider, IServiceBusStorage storage, IMessageBox messageBox, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
			=> InitializeAsync(serviceProvider, storage, messageBox, loggerFactory, cancellationToken);

		private readonly AsyncLock _initLock = new();
		internal async Task InitializeAsync(IServiceProvider serviceProvider, IServiceBusStorage storage, IMessageBox messageBox, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
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
					_fallbackLogger = loggerFactory.CreateLogger<ServiceBus>();
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

		protected internal override sealed async Task StartAsync(IServiceBusStorageContext context, CancellationToken cancellationToken)
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
				await Storage.WriteJobStartAsync(context, this, cancellationToken);

				_stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
				_timer = new Timer(TimerCallback, null, DelayedStart, ExecuteInterval);
			}
			catch (Exception ex)
			{
				ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(Storage)}.{nameof(Storage.WriteJobStartAsync)}", ex);
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
			LastActivityUtc = DateTime.UtcNow;

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

			try
			{
				try
				{
					await LogActivityAsync(traceInfo, ComponentState.InProcess, null, null, _stoppingCts?.Token ?? default);

					ComponentState componentState;
					using (var scope = _serviceProvider.CreateScope())
					{
						var appCtx = scope.ServiceProvider.GetRequiredService<IApplicationContext>();

						var jobContext = scope.ServiceProvider.GetRequiredService<JobContext>();
						jobContext.TraceInfo = new TraceInfoBuilder(TraceFrame.Create(), appCtx.Next()).Build();
						jobContext.Logger = _fallbackLogger ?? scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

						componentState = await ExecuteAsync(jobContext, _stoppingCts?.Token ?? default);
					}

					if (componentState == ComponentState.Suspended)
					{
						await LogActivityAsync(traceInfo, ComponentState.Suspended, null, null, _stoppingCts?.Token ?? default);
					}
				}
				catch (Exception ex)
				{
					await LogActivityAsync(
						traceInfo,
						ComponentState.Error,
						null,
						new LogError(traceInfo, nameof(TimerCallback), $"{nameof(ExecuteAsync)}", ex.ToStringTrace()),
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
					StartTimer();
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
						await Storage.WriteJobLogAsync(
							this,
							new LogError(traceInfo, logMessageType, message, ex?.ToStringTrace()),
							cancellationToken);
					}
				}
				catch (Exception storageEx)
				{
					ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, message, ex);
					ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(IServiceBusStorage)}.{nameof(Storage.WriteJobLogAsync)}", storageEx);
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

				await Storage.WriteJobActivityAsync(
					this,
					messageResult,
					log,
					cancellationToken);
			}
			catch (Exception ex)
			{
				await LogErrorAsync(traceInfo, $"{nameof(LogActivityAsync)}", $"{nameof(IServiceBusStorage)}.{nameof(Storage.WriteJobActivityAsync)}", ex, false, cancellationToken);

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

		private bool StartTimer()
			=> _timer?.Change(ExecuteInterval, ExecuteInterval) ?? false;

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
