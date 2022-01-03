using Npgsql;
using NpgsqlTypes;
using Raider.ServiceBus.Config.Components;
using Raider.ServiceBus.PostgreSql.Messages.Storage.Model;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.PostgreSql.Storage
{
	internal partial class PostgreSqlServiceBusStorage : PostgreSqlHostStorage
	{
		private async Task SaveComponentQueue(IComponentQueue componentQueue, Guid idComponent, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		{
			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var dbComponentQueue = new DbComponentQueue
			{
				IdComponentQueue = componentQueue.IdComponentQueue,
				IdComponent = idComponent,
				IdMessageType = componentQueue.MessageTypeModel.IdMessageType,
				Name = componentQueue.Name,
				Description = componentQueue.Description,
				LastMessageDeliveryUtc = null,
				IsFIFO = componentQueue.IsFIFO,
				ProcessingTimeoutInSeconds = componentQueue.ProcessingTimeoutInSeconds,
				MaxRetryCount = componentQueue.MaxRetryCount,
				SyncToken = Guid.NewGuid()
			};

			var sql = $@"{DbComponentQueue.GetInsertSql(_options)} 
ON CONFLICT (""{nameof(DbComponentQueue.IdComponentQueue)}"")
DO 
   UPDATE SET ""{nameof(DbComponentQueue.IdComponent)}"" = excluded.""{nameof(DbComponentQueue.IdComponent)}"",
			""{nameof(DbComponentQueue.IdMessageType)}"" = excluded.""{nameof(DbComponentQueue.IdMessageType)}"",
			""{nameof(DbComponentQueue.Name)}"" = excluded.""{nameof(DbComponentQueue.Name)}"",
			""{nameof(DbComponentQueue.Description)}"" = excluded.""{nameof(DbComponentQueue.Description)}"",
			""{nameof(DbComponentQueue.LastMessageDeliveryUtc)}"" = excluded.""{nameof(DbComponentQueue.LastMessageDeliveryUtc)}"",
			""{nameof(DbComponentQueue.IsFIFO)}"" = excluded.""{nameof(DbComponentQueue.IsFIFO)}"",
			""{nameof(DbComponentQueue.ProcessingTimeoutInSeconds)}"" = excluded.""{nameof(DbComponentQueue.ProcessingTimeoutInSeconds)}"",
			""{nameof(DbComponentQueue.MaxRetryCount)}"" = excluded.""{nameof(DbComponentQueue.MaxRetryCount)}"",
			""{nameof(DbComponentQueue.SyncToken)}"" = excluded.""{nameof(DbComponentQueue.SyncToken)}""";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			var table = DbComponentQueue.GetDictionaryTable(_options);
			table.SetParameters(cmd, dbComponentQueue.ToDictionary());

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(SaveComponentQueue)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}

		public async Task UpdateComponentQueueAsync(
			Guid idComponentQueue,
			string? description,
			int processingTimeoutInSeconds,
			int maxRetryCount,
			ITransactionContext transactionContext,
			CancellationToken cancellationToken = default)
		{
			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var sql = $@"
UPDATE {_options.ComponentDbSchemaName}.""{_options.ComponentDbTableName}""
	{(!string.IsNullOrEmpty(description) ? $@" ""{nameof(DbScenario.Description)}"" = @description, " : "")}
	""{nameof(DbComponentQueue.ProcessingTimeoutInSeconds)}"" = @processingTimeoutInSeconds,
	""{nameof(DbComponentQueue.MaxRetryCount)}"" = @maxRetryCount,
	""{nameof(DbComponentQueue.SyncToken)}"" = @syncToken
WHERE ""{nameof(DbComponentQueue.IdComponentQueue)}"" = @idComponentQueue;";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			if (!string.IsNullOrEmpty(description))
				cmd.Parameters.AddWithValue("@description", NpgsqlDbType.Varchar, description);

			cmd.Parameters.AddWithValue("@processingTimeoutInSeconds", NpgsqlDbType.Integer, processingTimeoutInSeconds);
			cmd.Parameters.AddWithValue("@maxRetryCount", NpgsqlDbType.Integer, maxRetryCount);
			cmd.Parameters.AddWithValue("@syncToken", NpgsqlDbType.Uuid, Guid.NewGuid());
			cmd.Parameters.AddWithValue("@idComponentQueue", NpgsqlDbType.Uuid, idComponentQueue);

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateComponentStatusAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
