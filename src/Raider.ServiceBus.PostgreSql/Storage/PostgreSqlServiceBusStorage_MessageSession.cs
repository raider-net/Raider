using Npgsql;
using NpgsqlTypes;
using Raider.Infrastructure;
using Raider.ServiceBus.Components;
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
		public async Task CreateMessageSessionAsync(Guid idSession, Guid idComponent, string? state, string? stateCrlType, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		{
			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var messageSession = new DbMessageSession
			{
				IdMessageSession = idSession,
				IdComponent = idComponent,
				TimeCreatedUtc = DateTime.UtcNow,
				RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
				State = state,
				StateCrlType = stateCrlType
			};

			var sql = DbMessageSession.GetInsertSql(_options);

			using var messageSessionCmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				messageSessionCmd.Transaction = transaction;

			var messageSessionTable = DbMessageSession.GetDictionaryTable(_options);
			messageSessionTable.SetParameters(messageSessionCmd, messageSession.ToDictionary());

			var result = await messageSessionCmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateMessageSessionAsync)}: {nameof(DbMessageSession)}.{nameof(messageSessionCmd.ExecuteNonQueryAsync)} returns {result}");



			sql = $@"
UPDATE {_options.ComponentDbSchemaName}.""{_options.ComponentDbTableName}""
SET ""{nameof(DbComponent.IdCurrentSession)}"" = @idSession, ""{nameof(DbComponent.LastHeartbeatUtc)}"" = @lastHeartbeatUtc, ""{nameof(DbComponent.SyncToken)}"" = @syncToken
WHERE ""{nameof(DbComponent.IdComponent)}"" = @idComponent;";

			using var componentCmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				componentCmd.Transaction = transaction;

			componentCmd.Parameters.AddWithValue("@idSession", NpgsqlDbType.Uuid, idSession);
			componentCmd.Parameters.AddWithValue("@lastHeartbeatUtc", NpgsqlDbType.TimestampTz, DateTime.UtcNow);
			componentCmd.Parameters.AddWithValue("@syncToken", NpgsqlDbType.Uuid, Guid.NewGuid());
			componentCmd.Parameters.AddWithValue("@idComponent", NpgsqlDbType.Uuid, idComponent);

			result = await componentCmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"Update {nameof(DbComponent)}.{nameof(DbComponent.IdCurrentSession)}: {nameof(componentCmd.ExecuteNonQueryAsync)} returns {result}");

			await LogInformationAsync(
				TraceInfo.Create(),
				idComponent,
				ComponentStatus.Unchanged,
				x => x.Detail($"New session = {idSession}"),
				$"New session = {idSession}",
				transactionContext,
				cancellationToken);
		}

		public async Task UpdateMessageSessionStateAsync<TMessage>(Guid idSession, string? state, string? stateCrlType, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		{
			if (!string.IsNullOrWhiteSpace(state) && string.IsNullOrWhiteSpace(stateCrlType))
				throw new ArgumentNullException(nameof(stateCrlType));

			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var sql = $@"
UPDATE {_options.MessageSessionDbSchemaName}.""{_options.MessageSessionDbTableName}""
SET ""{nameof(DbMessageSession.State)}"" = @state, ""{nameof(DbMessageSession.StateCrlType)}"" = @stateCrlType
WHERE ""{nameof(DbMessageSession.IdMessageSession)}"" = @idSession;";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			if (string.IsNullOrWhiteSpace(state))
				cmd.Parameters.AddWithValue("@state", NpgsqlDbType.Jsonb, DBNull.Value);
			else
				cmd.Parameters.AddWithValue("@state", NpgsqlDbType.Jsonb, state);

			if (string.IsNullOrWhiteSpace(stateCrlType))
				cmd.Parameters.AddWithValue("@stateCrlType", NpgsqlDbType.Jsonb, DBNull.Value);
			else
				cmd.Parameters.AddWithValue("@stateCrlType", NpgsqlDbType.Jsonb, stateCrlType);

			cmd.Parameters.AddWithValue("@idSession", NpgsqlDbType.Uuid, idSession);

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateMessageSessionAsync)}: {nameof(DbMessageSession)}.{nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
