using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using Raider.Infrastructure;
using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Events;
using Raider.ServiceBus.Events.Internal;
using Raider.ServiceBus.Events.Storage.Model;
using Raider.ServiceBus.Messages;
using Raider.ServiceBus.PostgreSql.Events.Providers;
using Raider.ServiceBus.PostgreSql.Messages.Storage.Model;
using Raider.ServiceBus.PostgreSql.Storage;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.PostgreSql.Events.Storage
{
	internal partial class PostgreSqlEventBusStorage : PostgreSqlHostStorage
	{
		private readonly IPostgreSqlEventBusOptions _options;
		private readonly IEventTypeRegistry _eventTypeRegistry;
		private readonly ISerializer _serialzier;
		private readonly IHandlerMessageLogger _baseHandlerEventLogger;

		public PostgreSqlEventBusStorage(IPostgreSqlEventBusOptions options, IServiceProvider serviceProvider)
			: base(options, serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			_options = options ?? throw new ArgumentNullException(nameof(options));

			var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

			var msHandlerEventLogger = loggerFactory.CreateLogger<BaseHandlerMessageLogger>();
			_baseHandlerEventLogger = new BaseHandlerMessageLogger(msHandlerEventLogger);

			_eventTypeRegistry = serviceProvider.GetRequiredService<IEventTypeRegistry>();
			_serialzier = _options.MessageSerializer(serviceProvider);

			if (_serialzier == null)
				throw new InvalidOperationException($"{nameof(_serialzier)} == NULL");
		}

		public async Task<IHost> InitializeHostAsync(CancellationToken cancellationToken = default)
		{
			var eventTypeRegistry = ServiceProvider.GetRequiredService<IEventTypeRegistry>();
			var eventTypes = eventTypeRegistry.GetAllEventTypes()?.Select(x => x.ToDictionary()).ToList();

			var host = await base.InitializeHostInternalAsync(HostType.EventBus, eventTypes, cancellationToken);
			return host;
		}

		public SavedEvent<TEvent> CreateEvent<TEvent>(TEvent @event, ServiceBus.Events.EventOptions? options = null)
			where TEvent : ServiceBus.Events.IEvent
		{
			if (@event == null)
				throw new ArgumentNullException(nameof(@event));

			var transactionContext = CreateTransactionContext();
			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var eventType = @event.GetType();
			var idEvent = Guid.NewGuid();
			var messageType = _eventTypeRegistry.GetEventType(eventType);
			if (messageType == null)
				throw new InvalidOperationException($"Event type {eventType} was not registered");

			var messageBody = new DbMessageBody
			{
				IdMessageBody = idEvent,
				IdMessageType = messageType.IdMessageType,
				Data = _serialzier.SerializeAsString(@event)
			};

			var sql = DbMessageBody.GetInsertSql(_options);

			using var messageBodyCmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				messageBodyCmd.Transaction = transaction;

			var messageBodytable = DbMessageBody.GetDictionaryTable(_options);
			messageBodytable.SetParameters(messageBodyCmd, messageBody.ToDictionary());

			var result = messageBodyCmd.ExecuteNonQuery();

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateEvent)}: {nameof(DbMessageBody)}.{nameof(messageBodyCmd.ExecuteNonQuery)} returns {result}");



			var eventMessage = new DbHandlerMessage
			{
				IdHandlerMessage = idEvent,
				IdHost = PostgreSqlEventBusInitializer.Instance.Host.IdHost,
				IdMessageType = messageType.IdMessageType,
				IdCorrespondingMessage = null,
				IdSession = options?.IdSession,
				TimeCreatedUtc = DateTime.UtcNow,
				IdMessageStatus = (int)MessageStatus.Created,
				RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
				IdMessageBody = messageBody.IdMessageBody,
				SyncToken = Guid.NewGuid()
			};

			sql = DbHandlerMessage.GetInsertSql(_options);

			using var messageCmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				messageCmd.Transaction = transaction;

			var messageTable = DbHandlerMessage.GetDictionaryTable(_options);
			messageTable.SetParameters(messageCmd, eventMessage.ToDictionary());

			result = messageCmd.ExecuteNonQuery();

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateEvent)}: {nameof(DbHandlerMessage)}.{nameof(messageCmd.ExecuteNonQuery)} returns {result}");

			LogInformation(TraceInfo.Create(), eventMessage.IdHandlerMessage, MessageStatus.InProcess, x => x.InternalMessage("InProcess"), "InProcess", transactionContext);

			transactionContext.Commit();

			return new SavedEvent<TEvent>
			{
				IdSavedEvent = idEvent,
				Event = _options.EnableMessageSerialization ? (TEvent)_serialzier.Deserialize(eventType, messageBody.Data)! : @event
			};
		}

		public async Task<SavedEvent<TEvent>> CreateEventAsync<TEvent>(TEvent @event, ServiceBus.Events.EventOptions? options = null, CancellationToken cancellationToken = default)
			where TEvent : ServiceBus.Events.IEvent
		{
			if (@event == null)
				throw new ArgumentNullException(nameof(@event));

			var transactionContext = await CreateTransactionContextAsync(cancellationToken);
			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var eventType = @event.GetType();
			var idEvent = Guid.NewGuid();
			var messageType = _eventTypeRegistry.GetEventType(eventType);
			if (messageType == null)
				throw new InvalidOperationException($"Event type {eventType} was not registered");

			var messageBody = new DbMessageBody
			{
				IdMessageBody = idEvent,
				IdMessageType = messageType.IdMessageType,
				Data = _serialzier.SerializeAsString(@event)
			};

			var sql = DbMessageBody.GetInsertSql(_options);

			using var messageBodyCmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				messageBodyCmd.Transaction = transaction;

			var messageBodytable = DbMessageBody.GetDictionaryTable(_options);
			messageBodytable.SetParameters(messageBodyCmd, messageBody.ToDictionary());

			var result = await messageBodyCmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateEventAsync)}: {nameof(DbMessageBody)}.{nameof(messageBodyCmd.ExecuteNonQueryAsync)} returns {result}");



			var eventMessage = new DbHandlerMessage
			{
				IdHandlerMessage = idEvent,
				IdHost = PostgreSqlEventBusInitializer.Instance.Host.IdHost,
				IdMessageType = messageType.IdMessageType,
				IdCorrespondingMessage = null,
				IdSession = options?.IdSession,
				TimeCreatedUtc = DateTime.UtcNow,
				IdMessageStatus = (int)MessageStatus.Created,
				RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
				IdMessageBody = messageBody.IdMessageBody,
				SyncToken = Guid.NewGuid()
			};

			sql = DbHandlerMessage.GetInsertSql(_options);

			using var messageCmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				messageCmd.Transaction = transaction;

			var messageTable = DbHandlerMessage.GetDictionaryTable(_options);
			messageTable.SetParameters(messageCmd, eventMessage.ToDictionary());

			result = await messageCmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateEventAsync)}: {nameof(DbHandlerMessage)}.{nameof(messageCmd.ExecuteNonQueryAsync)} returns {result}");

			await LogInformationAsync(TraceInfo.Create(), eventMessage.IdHandlerMessage, MessageStatus.InProcess, x => x.InternalMessage("InProcess"), "InProcess", transactionContext, cancellationToken);

			await transactionContext.CommitAsync(cancellationToken);

			return new SavedEvent<TEvent>
			{
				IdSavedEvent = idEvent,
				Event = _options.EnableMessageSerialization ? (TEvent)_serialzier.Deserialize(eventType, messageBody.Data)! : @event
			};
		}

		private void UpdateEventStatus(Guid idEvent,  MessageStatus messageStatus, ITransactionContext transactionContext)
		{
			if (messageStatus == MessageStatus.Unchanged)
				return;

			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var sql = $@"
UPDATE {_options.HandlerMessageDbSchemaName}.""{_options.HandlerMessageDbTableName}""
SET ""{nameof(DbHandlerMessage.IdMessageStatus)}"" = @idMessageStatus, ""{nameof(DbHandlerMessage.SyncToken)}"" = @syncToken
WHERE ""{nameof(DbHandlerMessage.IdHandlerMessage)}"" = @idHandlerMessage;";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			cmd.Parameters.AddWithValue("@idMessageStatus", NpgsqlDbType.Integer, (int)messageStatus);
			cmd.Parameters.AddWithValue("@syncToken", NpgsqlDbType.Uuid, Guid.NewGuid());
			cmd.Parameters.AddWithValue("@idHandlerMessage", NpgsqlDbType.Uuid, idEvent);

			var result = cmd.ExecuteNonQuery();

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateEventStatus)}: {nameof(cmd.ExecuteNonQuery)} returns {result}");
		}

		private async Task UpdateEventStatusAsync(Guid idEvent, MessageStatus messageStatus, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		{
			if (messageStatus == MessageStatus.Unchanged)
				return;

			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var sql = $@"
UPDATE {_options.HandlerMessageDbSchemaName}.""{_options.HandlerMessageDbTableName}""
SET ""{nameof(DbHandlerMessage.IdMessageStatus)}"" = @idMessageStatus, ""{nameof(DbHandlerMessage.SyncToken)}"" = @syncToken
WHERE ""{nameof(DbHandlerMessage.IdHandlerMessage)}"" = @idHandlerMessage;";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			cmd.Parameters.AddWithValue("@idMessageStatus", NpgsqlDbType.Integer, (int)messageStatus);
			cmd.Parameters.AddWithValue("@syncToken", NpgsqlDbType.Uuid, Guid.NewGuid());
			cmd.Parameters.AddWithValue("@idHandlerMessage", NpgsqlDbType.Uuid, idEvent);

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateEventStatusAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
