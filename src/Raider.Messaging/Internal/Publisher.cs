using Microsoft.Extensions.Logging;
using Raider.Extensions;
using Raider.Messaging.Messages;
using Raider.Threading;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	internal class Publisher<TData> : ComponentBase, IPublisher<TData>
			where TData : IMessageData
	{
		private ILogger? _fallbackLogger;

		public IReadOnlyList<ISubscriber> Subscribers { get; private set; }

		/// <summary>
		/// IdPublisher
		/// </summary>
		public override sealed int IdComponent { get; }

		/// <summary>
		/// IdPublisherInstance
		/// </summary>
		public override sealed Guid IdInstance { get; } = Guid.NewGuid();
		public override sealed bool Initialized { get; protected set; }
		public override sealed bool Started { get; protected set; }
		public override sealed string Name { get; }
		public override sealed string? Description { get; set; }
		public override sealed int IdScenario { get; }
		public override sealed DateTime LastActivityUtc { get; protected set; }
		public override sealed ComponentState State { get; protected set; }
		public IServiceBusRegister? Register { get; set; }
		public IServiceBusStorage? Storage { get; set; }
		public IMessageBox? MessageBox { get; private set; }
		public override sealed IReadOnlyDictionary<object, object> ServiceBusHostProperties => Storage?.ServiceBusHost?.Properties ?? new ReadOnlyDictionary<object, object>(new Dictionary<object, object>());

		public Type PublishingMessageDataType { get; } = typeof(TData);
		public bool WriteToSubscribers { get; }

		public Publisher(int idPublisher, string name, int idScenario, bool writeToSubscribers)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				var traceInfo = TraceInfo.Create();
				Serilog.Log.Logger.Error($"{traceInfo}: {nameof(name)} == null");
				throw new ArgumentNullException(nameof(name));
			}
			Name = name;

			IdComponent = idPublisher;
			IdScenario = idScenario;
			LastActivityUtc = DateTime.UtcNow;
			Subscribers = new List<ISubscriber>();
			State = ComponentState.Offline;
			WriteToSubscribers = writeToSubscribers;
		}

		private readonly AsyncLock _initLock = new();
		async Task IPublisher.InitializeAsync(IServiceBusStorage storage, IMessageBox messageBox, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
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

				if (Register == null)
				{
					var error = $"{nameof(Register)} == null";
					ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, error, null);
					throw new InvalidOperationException(error);
				}

				Subscribers = Register.GetMessageSubscribers<TData>();

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
				await Storage.WritePublisherStartAsync(context, this, cancellationToken);
			}
			catch (Exception ex)
			{
				ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(Storage)}.{nameof(Storage.WritePublisherStartAsync)}", ex);
				throw;
			}

			Started = true;
		}

		public async Task<IMessage<TData>> PublishMessageAsync(TData data, IMessage? previousMessage = null, DateTimeOffset? validToUtc = null, bool isRecovery = false, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
		{
			var appCtxTraceInfo = Storage?.ServiceBusHost?.ApplicationContext.TraceInfo;
			var traceInfo = TraceInfo.Create(appCtxTraceInfo?.Principal, appCtxTraceInfo?.RuntimeUniqueKey);
			LastActivityUtc = DateTime.UtcNow;

			if (!Started)
			{
				var error = $"!{nameof(Started)}";
				ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, error, null);
				throw new InvalidOperationException(error);
			}

			if (MessageBox == null)
			{
				var error = $"{nameof(MessageBox)} == null";

				await LogErrorAsync(
					traceInfo,
					nameof(ServiceBusDefaults.LogMessageType.PublishMessage),
					error, null, true, cancellationToken);

				throw new InvalidOperationException(error);
			}

			//if (Subscribers.Count == 0)
			//{
			//	var error = $"Not subscriber registered for message type {typeof(TData).FullName}";

			//	await LogErrorAsync(
			//		traceInfo,
			//		nameof(ServiceBusDefaults.LogMessageType.PublishMessage),
			//		error, null, true, cancellationToken);

			//	throw new InvalidOperationException(error);
			//}

			var message = new Message<TData>
			{
				IdMessage = Guid.NewGuid(),
				IdPreviousMessage = previousMessage?.IdMessage,
				IdPublisherInstance = IdInstance,
				CreatedUtc = DateTimeOffset.UtcNow,
				ValidToUtc = validToUtc,
				IsRecovery = isRecovery,
				Data = data
			};

			try
			{
				await MessageBox.WriteMessageAsync(message, WriteToSubscribers ? Subscribers : new List<ISubscriber>(), dbTransaction, cancellationToken);

				await LogActivityAsync(traceInfo, ComponentState.Idle, null, cancellationToken);
			}
			catch (Exception ex)
			{
				await LogErrorAsync(
					traceInfo,
					nameof(ServiceBusDefaults.LogMessageType.PublishMessage),
					$"{nameof(MessageBox)}.{nameof(MessageBox.WriteMessageAsync)}", ex, true, cancellationToken);

				throw;
			}

			return message;
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
							new LogError(traceInfo, logMessageType, message, ex?.ToStringTrace()),
							cancellationToken);
					}
					else
					{
						await Storage.WritePublisherLogAsync(
							this,
							new LogError(traceInfo, logMessageType, message, ex?.ToStringTrace()),
							cancellationToken);
					}
				}
				catch (Exception storageEx)
				{
					ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, message, ex);
					ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(IServiceBusStorage)}.{nameof(Storage.WritePublisherLogAsync)}", storageEx);
					throw;
				}
			}
		}

		private async Task LogActivityAsync(ITraceInfo? traceInfo, ComponentState state, LogBase? log, CancellationToken cancellationToken = default)
		{
			var originalState = State;
			State = state;
			LastActivityUtc = DateTime.UtcNow;

			try
			{
				if (Storage == null)
					throw new InvalidOperationException($"{nameof(Storage)} == null");

				await Storage.WritePublisherActivityAsync(
					this,
					log,
					cancellationToken);
			}
			catch (Exception ex)
			{
				await LogErrorAsync(traceInfo, $"{nameof(LogActivityAsync)}", $"{nameof(IServiceBusStorage)}.{nameof(Storage.WritePublisherActivityAsync)}", ex, false, cancellationToken);

				if (state != ComponentState.Error || state != ComponentState.Suspended)
				{
					await LogActivityAsync(
						traceInfo,
						ComponentState.Error,
						new LogError(traceInfo, $"{nameof(LogActivityAsync)}_Reset", $"Original state = {originalState} | Failed state = {State} | Final state = {ComponentState.Error}"),
						cancellationToken);
				}

				throw;
			}
		}
	}
}
