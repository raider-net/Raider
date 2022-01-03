using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using Raider.Infrastructure;
using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Messages;
using Raider.ServiceBus.Messages.Internal;
using Raider.ServiceBus.Messages.Storage.Model;
using Raider.ServiceBus.PostgreSql.Messages.Providers;
using Raider.ServiceBus.PostgreSql.Messages.Storage.Model;
using Raider.ServiceBus.PostgreSql.Storage;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.PostgreSql.Messages.Storage
{
	internal partial class PostgreSqlMessageBusStorage : PostgreSqlHostStorage
	{
		private readonly IPostgreSqlMessageBusOptions _options;
		private readonly IMessageTypeRegistry _messageTypeRegistry;
		private readonly ISerializer _serialzier;
		private readonly IHandlerMessageLogger _baseHandlerMessageLogger;

		public PostgreSqlMessageBusStorage(IPostgreSqlMessageBusOptions options, IServiceProvider serviceProvider)
			: base(options, serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			_options = options ?? throw new ArgumentNullException(nameof(options));

			var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

			var msHandlerMessageLogger = loggerFactory.CreateLogger<BaseHandlerMessageLogger>();
			_baseHandlerMessageLogger = new BaseHandlerMessageLogger(msHandlerMessageLogger);

			_messageTypeRegistry = serviceProvider.GetRequiredService<IMessageTypeRegistry>();
			_serialzier = _options.MessageSerializer(serviceProvider);

			if (_serialzier == null)
				throw new InvalidOperationException($"{nameof(_serialzier)} == NULL");
		}

		public async Task<IHost> InitializeHostAsync(CancellationToken cancellationToken = default)
		{
			var messageTypeRegistry = ServiceProvider.GetRequiredService<IMessageTypeRegistry>();
			var messageTypes = messageTypeRegistry.GetAllMessageTypes()?.Select(x => x.ToDictionary()).ToList();

			var host = await base.InitializeHostInternalAsync(HostType.MessageBus, messageTypes, cancellationToken);
			return host;
		}

		public SavedMessage<TMessage> CreateRequestMessage<TMessage>(TMessage requestMessage, ServiceBus.Messages.MessageOptions? options = null)
			where TMessage : ServiceBus.Messages.IBaseRequestMessage
		{
			if (requestMessage == null)
				throw new ArgumentNullException(nameof(requestMessage));

			var transactionContext = CreateTransactionContext();
			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var requestMessageType = requestMessage.GetType();
			var idMessage = Guid.NewGuid();
			var messageType = _messageTypeRegistry.GetMessageType(requestMessageType);
			if (messageType == null)
				throw new InvalidOperationException($"Message type {requestMessageType} was not registered");

			var messageBody = new DbMessageBody
			{
				IdMessageBody = idMessage,
				IdMessageType = messageType.IdMessageType,
				Data = _serialzier.SerializeAsString(requestMessage)
			};

			var sql = DbMessageBody.GetInsertSql(_options);

			using var messageBodyCmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				messageBodyCmd.Transaction = transaction;

			var messageBodytable = DbMessageBody.GetDictionaryTable(_options);
			messageBodytable.SetParameters(messageBodyCmd, messageBody.ToDictionary());

			var result = messageBodyCmd.ExecuteNonQuery();

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateRequestMessage)}: {nameof(DbMessageBody)}.{nameof(messageBodyCmd.ExecuteNonQuery)} returns {result}");



			var handlerMessage = new DbHandlerMessage
			{
				IdHandlerMessage = idMessage,
				IdHost = PostgreSqlMessageBusInitializer.Instance.Host.IdHost,
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
			messageTable.SetParameters(messageCmd, handlerMessage.ToDictionary());

			result = messageCmd.ExecuteNonQuery();

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateRequestMessage)}: {nameof(DbHandlerMessage)}.{nameof(messageCmd.ExecuteNonQuery)} returns {result}");

			LogInformation(TraceInfo.Create(), handlerMessage.IdHandlerMessage, MessageStatus.InProcess, x => x.InternalMessage("InProcess"), "InProcess", transactionContext);

			transactionContext.Commit();

			return new SavedMessage<TMessage>
			{
				IdSavedMessage = idMessage,
				Message = _options.EnableMessageSerialization ? (TMessage)_serialzier.Deserialize(requestMessageType, messageBody.Data)! : requestMessage
			};
		}

		public Guid CreateResponseMessage<TResponse>(IResult<TResponse> responseMessage, IMessageHandlerContext handlerContext)
		{
			if (responseMessage == null)
				throw new ArgumentNullException(nameof(responseMessage));

			if (handlerContext == null)
				throw new ArgumentNullException(nameof(handlerContext));

			var transactionContext = CreateTransactionContext();
			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var idResponseMessage = Guid.NewGuid();
			var responseMessageType = typeof(TResponse);

			var messageType = _messageTypeRegistry.GetMessageType(responseMessageType);
			if (messageType == null)
				throw new InvalidOperationException($"Message type {responseMessageType} was not registered");

			var messageBody = new DbMessageBody
			{
				IdMessageBody = idResponseMessage,
				IdMessageType = messageType.IdMessageType,
				Data = _serialzier.SerializeAsString(responseMessage)
			};

			var sql = DbMessageBody.GetInsertSql(_options);

			using var messageBodyCmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				messageBodyCmd.Transaction = transaction;

			var messageBodytable = DbMessageBody.GetDictionaryTable(_options);
			messageBodytable.SetParameters(messageBodyCmd, messageBody.ToDictionary());

			var result = messageBodyCmd.ExecuteNonQuery();

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateResponseMessage)}: {nameof(DbMessageBody)}.{nameof(messageBodyCmd.ExecuteNonQuery)} returns {result}");



			var handlerMessage = new DbHandlerMessage
			{
				IdHandlerMessage = idResponseMessage,
				IdHost = PostgreSqlMessageBusInitializer.Instance.Host.IdHost,
				IdMessageType = messageType.IdMessageType,
				TimeCreatedUtc = DateTime.UtcNow,
				IdMessageStatus = (int)MessageStatus.Created,
				RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
				IdCorrespondingMessage = handlerContext.IdMessage,
				IdSession = handlerContext.IdSession,
				IdMessageBody = messageBody.IdMessageBody,
				SyncToken = Guid.NewGuid()
			};

			sql = DbHandlerMessage.GetInsertSql(_options);

			using var messageCmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				messageCmd.Transaction = transaction;

			var messageTable = DbHandlerMessage.GetDictionaryTable(_options);
			messageTable.SetParameters(messageCmd, handlerMessage.ToDictionary());

			result = messageCmd.ExecuteNonQuery();

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateResponseMessage)}: {nameof(DbHandlerMessage)}.{nameof(messageCmd.ExecuteNonQuery)} returns {result}");

			SetCorrespondingResponseMessage(handlerContext.IdMessage, idResponseMessage, transactionContext);
			LogInformation(TraceInfo.Create(), handlerContext.IdMessage, MessageStatus.Unchanged, x => x.InternalMessage("CREATED RESPONSE"), "CREATED RESPONSE", transactionContext);
			LogInformation(TraceInfo.Create(), idResponseMessage, MessageStatus.Completed, x => x.InternalMessage("Completed"), "Completed", transactionContext);

			transactionContext.Commit();

			return idResponseMessage;
		}

		public async Task<SavedMessage<TMessage>> CreateRequestMessageAsync<TMessage>(TMessage requestMessage, ServiceBus.Messages.MessageOptions? options = null, CancellationToken cancellationToken = default)
			where TMessage : ServiceBus.Messages.IBaseRequestMessage
		{
			if (requestMessage == null)
				throw new ArgumentNullException(nameof(requestMessage));

			var transactionContext = await CreateTransactionContextAsync(cancellationToken);
			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var requestMessageType = requestMessage.GetType();
			var idMessage = Guid.NewGuid();
			var messageType = _messageTypeRegistry.GetMessageType(requestMessageType);
			if (messageType == null)
				throw new InvalidOperationException($"Message type {requestMessageType} was not registered");

			var messageBody = new DbMessageBody
			{
				IdMessageBody = idMessage,
				IdMessageType = messageType.IdMessageType,
				Data = _serialzier.SerializeAsString(requestMessage)
			};

			var sql = DbMessageBody.GetInsertSql(_options);

			using var messageBodyCmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				messageBodyCmd.Transaction = transaction;

			var messageBodytable = DbMessageBody.GetDictionaryTable(_options);
			messageBodytable.SetParameters(messageBodyCmd, messageBody.ToDictionary());

			var result = await messageBodyCmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateRequestMessageAsync)}: {nameof(DbMessageBody)}.{nameof(messageBodyCmd.ExecuteNonQueryAsync)} returns {result}");



			var handlerMessage = new DbHandlerMessage
			{
				IdHandlerMessage = idMessage,
				IdHost = PostgreSqlMessageBusInitializer.Instance.Host.IdHost,
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
			messageTable.SetParameters(messageCmd, handlerMessage.ToDictionary());

			result = await messageCmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateRequestMessageAsync)}: {nameof(DbHandlerMessage)}.{nameof(messageCmd.ExecuteNonQueryAsync)} returns {result}");

			await LogInformationAsync(TraceInfo.Create(), handlerMessage.IdHandlerMessage, MessageStatus.InProcess, x => x.InternalMessage("InProcess"), "InProcess", transactionContext, cancellationToken);

			await transactionContext.CommitAsync(cancellationToken);

			return new SavedMessage<TMessage>
			{
				IdSavedMessage = idMessage,
				Message = _options.EnableMessageSerialization ? (TMessage)_serialzier.Deserialize(requestMessageType, messageBody.Data)! : requestMessage
			};
		}

		public async Task<Guid> CreateResponseMessageAsync<TResponse>(IResult<TResponse> responseMessage, IMessageHandlerContext handlerContext, CancellationToken cancellationToken = default)
		{
			if (responseMessage == null)
				throw new ArgumentNullException(nameof(responseMessage));

			if (handlerContext == null)
				throw new ArgumentNullException(nameof(handlerContext));

			var transactionContext = await CreateTransactionContextAsync(cancellationToken);
			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var idResponseMessage = Guid.NewGuid();
			var responseMessageType = typeof(TResponse);

			var messageType = _messageTypeRegistry.GetMessageType(responseMessageType);
			if (messageType == null)
				throw new InvalidOperationException($"Message type {responseMessageType} was not registered");

			var messageBody = new DbMessageBody
			{
				IdMessageBody = idResponseMessage,
				IdMessageType = messageType.IdMessageType,
				Data = _serialzier.SerializeAsString(responseMessage)
			};

			var sql = DbMessageBody.GetInsertSql(_options);

			using var messageBodyCmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				messageBodyCmd.Transaction = transaction;

			var messageBodytable = DbMessageBody.GetDictionaryTable(_options);
			messageBodytable.SetParameters(messageBodyCmd, messageBody.ToDictionary());

			var result = await messageBodyCmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateResponseMessageAsync)}: {nameof(DbMessageBody)}.{nameof(messageBodyCmd.ExecuteNonQueryAsync)} returns {result}");



			var handlerMessage = new DbHandlerMessage
			{
				IdHandlerMessage = idResponseMessage,
				IdHost = PostgreSqlMessageBusInitializer.Instance.Host.IdHost,
				IdMessageType = messageType.IdMessageType,
				TimeCreatedUtc = DateTime.UtcNow,
				IdMessageStatus = (int)MessageStatus.Created,
				RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
				IdCorrespondingMessage = handlerContext.IdMessage,
				IdSession = handlerContext.IdSession,
				IdMessageBody = messageBody.IdMessageBody,
				SyncToken = Guid.NewGuid()
			};

			sql = DbHandlerMessage.GetInsertSql(_options);

			using var messageCmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				messageCmd.Transaction = transaction;

			var messageTable = DbHandlerMessage.GetDictionaryTable(_options);
			messageTable.SetParameters(messageCmd, handlerMessage.ToDictionary());

			result = await messageCmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateResponseMessageAsync)}: {nameof(DbHandlerMessage)}.{nameof(messageCmd.ExecuteNonQueryAsync)} returns {result}");

			await SetCorrespondingResponseMessageAsync(handlerContext.IdMessage, idResponseMessage, transactionContext, cancellationToken);
			await LogInformationAsync(TraceInfo.Create(), handlerContext.IdMessage, MessageStatus.Unchanged, x => x.InternalMessage("CREATED  RESPONSE"), "CREATED  RESPONSE", transactionContext, cancellationToken);
			await LogInformationAsync(TraceInfo.Create(), idResponseMessage, MessageStatus.Completed, x => x.InternalMessage("Completed"), "Completed", transactionContext, cancellationToken);

			await transactionContext.CommitAsync(cancellationToken);

			return idResponseMessage;
		}

		private void UpdateMessageStatus(Guid idMessage, MessageStatus messageStatus, ITransactionContext transactionContext)
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
			cmd.Parameters.AddWithValue("@idHandlerMessage", NpgsqlDbType.Uuid, idMessage);

			var result = cmd.ExecuteNonQuery();

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateMessageStatus)}: {nameof(cmd.ExecuteNonQuery)} returns {result}");
		}

		private void SetCorrespondingResponseMessage(Guid idMessage, Guid idResponseMessage, ITransactionContext transactionContext)
		{
			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var sql = $@"
UPDATE {_options.HandlerMessageDbSchemaName}.""{_options.HandlerMessageDbTableName}""
SET ""{nameof(DbHandlerMessage.IdCorrespondingMessage)}"" = @idCorrespondingMessage, ""{nameof(DbHandlerMessage.SyncToken)}"" = @syncToken
WHERE ""{nameof(DbHandlerMessage.IdHandlerMessage)}"" = @idHandlerMessage;";

			using var firstCmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				firstCmd.Transaction = transaction;

			firstCmd.Parameters.AddWithValue("@idCorrespondingMessage", NpgsqlDbType.Uuid, idResponseMessage);
			firstCmd.Parameters.AddWithValue("@syncToken", NpgsqlDbType.Uuid, Guid.NewGuid());
			firstCmd.Parameters.AddWithValue("@idHandlerMessage", NpgsqlDbType.Uuid, idMessage);

			var result = firstCmd.ExecuteNonQuery();

			if (result != 1)
				throw new InvalidOperationException($"{nameof(SetCorrespondingResponseMessage)}: {nameof(firstCmd.ExecuteNonQuery)} returns {result}");
		}

		private async Task UpdateMessageStatusAsync(Guid idMessage, MessageStatus messageStatus, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
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
			cmd.Parameters.AddWithValue("@idHandlerMessage", NpgsqlDbType.Uuid, idMessage);

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateMessageStatusAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}

		private async Task SetCorrespondingResponseMessageAsync(Guid idMessage, Guid idResponseMessage, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		{
			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var sql = $@"
UPDATE {_options.HandlerMessageDbSchemaName}.""{_options.HandlerMessageDbTableName}""
SET ""{nameof(DbHandlerMessage.IdCorrespondingMessage)}"" = @idCorrespondingMessage, ""{nameof(DbHandlerMessage.SyncToken)}"" = @syncToken
WHERE ""{nameof(DbHandlerMessage.IdHandlerMessage)}"" = @idHandlerMessage;";

			using var firstCmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				firstCmd.Transaction = transaction;

			firstCmd.Parameters.AddWithValue("@idCorrespondingMessage", NpgsqlDbType.Uuid, idResponseMessage);
			firstCmd.Parameters.AddWithValue("@syncToken", NpgsqlDbType.Uuid, Guid.NewGuid());
			firstCmd.Parameters.AddWithValue("@idHandlerMessage", NpgsqlDbType.Uuid, idMessage);

			var result = await firstCmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(SetCorrespondingResponseMessageAsync)}: {nameof(firstCmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
