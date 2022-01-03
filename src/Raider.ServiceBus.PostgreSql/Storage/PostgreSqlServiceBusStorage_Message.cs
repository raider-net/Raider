using Npgsql;
using NpgsqlTypes;
using Raider.Infrastructure;
using Raider.ServiceBus.Messages;
using Raider.ServiceBus.PostgreSql.Messages.Storage.Model;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.PostgreSql.Storage
{
	internal partial class PostgreSqlServiceBusStorage : PostgreSqlHostStorage
	{
		//public async Task<Model.SavedMessage<TMessage>> CreateRequestMessageAsync<TMessage>(TMessage message, Guid idSourceComponent, ServiceBusMessageOptions options, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		//	where TMessage : IMessageHeader
		//{
		//	if (message == null)
		//		throw new ArgumentNullException(nameof(message));

		//	if (options == null)
		//		throw new ArgumentNullException(nameof(options));

		//	if (transactionContext == null)
		//		throw new ArgumentNullException(nameof(transactionContext));

		//	var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
		//	var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

		//	var requestMessageType = message.GetType();
		//	var idMessage = Guid.NewGuid();

		//	if (!ServiceBusInitializer.Instance.MessageTypes.TryGetValue(requestMessageType, out var messageType))
		//		throw new InvalidOperationException($"Message type {requestMessageType} was not registered");

		//	var messageBody = new DbMessageBody
		//	{
		//		IdMessageBody = idMessage,
		//		IdMessageType = messageType.IdMessageType,
		//		Data = _serialzier.SerializeAsString(message)
		//	};

		//	var sql = DbMessageBody.GetInsertSql(_options);

		//	using var messageBodyCmd = new NpgsqlCommand(sql, connection);
		//	if (transaction != null)
		//		messageBodyCmd.Transaction = transaction;

		//	var messageBodytable = DbMessageBody.GetDictionaryTable(_options);
		//	messageBodytable.SetParameters(messageBodyCmd, messageBody.ToDictionary());

		//	var result = await messageBodyCmd.ExecuteNonQueryAsync(cancellationToken);

		//	if (result != 1)
		//		throw new InvalidOperationException($"{nameof(CreateRequestMessageAsync)}: {nameof(DbMessageBody)}.{nameof(messageBodyCmd.ExecuteNonQueryAsync)} returns {result}");



		//	var handlerMessage = new DbMessageHeader
		//	{
		//		IdMessage = idMessage,
		//		IdMessageType = messageType.IdMessageType,
		//		IdCorrespondingMessage = null,
		//		IdSession = options.IdSession,
		//		IdSourceComponent = idSourceComponent,
		//		IdSourceResponseQueue = options.IdSourceResponseQueue,
		//		IdTargetComponent = options.IdTargetComponent,
		//		IdTargetQueue = options.IdTargetQueue,
		//		IdMessageBody = messageBody.IdMessageBody,
		//		Description = options.Description,
		//		IdPriority = (int)options.Priority,
		//		TimeCreatedUtc = DateTime.UtcNow,
		//		TimeLastProcessedUtc = null,
		//		IdMessageStatus = (int)MessageStatus.Created,
		//		RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
		//		RetryCount = 0,
		//		DelayedToUtc = null,
		//		SyncToken = Guid.NewGuid()
		//	};

		//	sql = DbMessageHeader.GetInsertSql(_options);

		//	using var messageCmd = new NpgsqlCommand(sql, connection);
		//	if (transaction != null)
		//		messageCmd.Transaction = transaction;

		//	var messageTable = DbMessageHeader.GetDictionaryTable(_options);
		//	messageTable.SetParameters(messageCmd, handlerMessage.ToDictionary());

		//	result = await messageCmd.ExecuteNonQueryAsync(cancellationToken);

		//	if (result != 1)
		//		throw new InvalidOperationException($"{nameof(CreateRequestMessageAsync)}: {nameof(DbMessageHeader)}.{nameof(messageCmd.ExecuteNonQueryAsync)} returns {result}");

		//	await LogInformationAsync(TraceInfo.Create(), handlerMessage.IdMessage, null, MessageStatus.InProcess, x => x.InternalMessage("InProcess"), "InProcess", transactionContext, cancellationToken);

		//	return new Model.SavedMessage<TMessage>
		//	{
		//		IdSavedMessage = idMessage,
		//		Message = _options.EnableMessageSerialization ? (TMessage)_serialzier.Deserialize(requestMessageType, messageBody.Data)! : message
		//	};
		//}

		//public async Task<Guid> CreateResponseMessageAsync<TResponse>(IResult<TResponse> responseMessage, IMessageHandlerContext handlerContext, CancellationToken cancellationToken = default)
		//{
		//	if (responseMessage == null)
		//		throw new ArgumentNullException(nameof(responseMessage));

		//	if (handlerContext == null)
		//		throw new ArgumentNullException(nameof(handlerContext));

		//	var transactionContext = await CreateTransactionContextAsync(cancellationToken);
		//	var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
		//	var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

		//	var idResponseMessage = Guid.NewGuid();
		//	var responseMessageType = typeof(TResponse);

		//	var messageType = _messageTypeRegistry.GetMessageType(responseMessageType);
		//	if (messageType == null)
		//		throw new InvalidOperationException($"Message type {responseMessageType} was not registered");

		//	var messageBody = new DbMessageBody
		//	{
		//		IdMessageBody = idResponseMessage,
		//		IdMessageType = messageType.IdMessageType,
		//		Data = _serialzier.SerializeAsString(responseMessage)
		//	};

		//	var sql = DbMessageBody.GetInsertSql(_options);

		//	using var messageBodyCmd = new NpgsqlCommand(sql, connection);
		//	if (transaction != null)
		//		messageBodyCmd.Transaction = transaction;

		//	var messageBodytable = DbMessageBody.GetDictionaryTable(_options);
		//	messageBodytable.SetParameters(messageBodyCmd, messageBody.ToDictionary());

		//	var result = await messageBodyCmd.ExecuteNonQueryAsync(cancellationToken);

		//	if (result != 1)
		//		throw new InvalidOperationException($"{nameof(CreateResponseMessageAsync)}: {nameof(DbMessageBody)}.{nameof(messageBodyCmd.ExecuteNonQueryAsync)} returns {result}");



		//	var handlerMessage = new DbMessageHeader
		//	{
		//		IdMessage = idResponseMessage,
		//		IdHost = ServiceBusInitializer.Instance.Host.IdHost,
		//		IdMessageType = messageType.IdMessageType,
		//		TimeCreatedUtc = DateTime.UtcNow,
		//		IdMessageStatus = (int)MessageStatus.Created,
		//		RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
		//		IdCorrespondingMessage = handlerContext.IdMessage,
		//		IdSession = handlerContext.IdSession,
		//		IdMessageBody = messageBody.IdMessageBody,
		//		SyncToken = Guid.NewGuid()
		//	};

		//	sql = DbMessageHeader.GetInsertSql(_options);

		//	using var messageCmd = new NpgsqlCommand(sql, connection);
		//	if (transaction != null)
		//		messageCmd.Transaction = transaction;

		//	var messageTable = DbMessageHeader.GetDictionaryTable(_options);
		//	messageTable.SetParameters(messageCmd, handlerMessage.ToDictionary());

		//	result = await messageCmd.ExecuteNonQueryAsync(cancellationToken);

		//	if (result != 1)
		//		throw new InvalidOperationException($"{nameof(CreateResponseMessageAsync)}: {nameof(DbMessageHeader)}.{nameof(messageCmd.ExecuteNonQueryAsync)} returns {result}");

		//	await LogInformationAsync(TraceInfo.Create(), handlerContext.IdMessage, idResponseMessage, MessageStatus.Unchanged, x => x.InternalMessage("CREATED  RESPONSE"), "CREATED  RESPONSE", transactionContext, cancellationToken);
		//	await LogInformationAsync(TraceInfo.Create(), idResponseMessage, handlerContext.IdMessage, MessageStatus.Completed, x => x.InternalMessage("Completed"), "Completed", transactionContext, cancellationToken);

		//	await transactionContext.CommitAsync(cancellationToken);

		//	return idResponseMessage;
		//}

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
SET ""{nameof(DbMessageHeader.IdMessageStatus)}"" = @idMessageStatus, ""{nameof(DbMessageHeader.SyncToken)}"" = @syncToken
WHERE ""{nameof(DbMessageHeader.IdMessage)}"" = @IdMessage;";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			cmd.Parameters.AddWithValue("@idMessageStatus", NpgsqlDbType.Integer, (int)messageStatus);
			cmd.Parameters.AddWithValue("@syncToken", NpgsqlDbType.Uuid, Guid.NewGuid());
			cmd.Parameters.AddWithValue("@IdMessage", NpgsqlDbType.Uuid, idMessage);

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
SET ""{nameof(DbMessageHeader.IdCorrespondingMessage)}"" = @idCorrespondingMessage, ""{nameof(DbMessageHeader.SyncToken)}"" = @syncToken
WHERE ""{nameof(DbMessageHeader.IdMessage)}"" = @idHandlerMessage;";

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
