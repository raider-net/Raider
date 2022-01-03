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
		private async Task SaveScenario(IScenario scenario, Guid idHost, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		{
			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var utcNow = DateTime.UtcNow;
			var dbScenario = new DbScenario
			{
				IdScenario = scenario.IdScenario,
				IdHost = idHost,
				Name = scenario.Name,
				Description = scenario.Description,
				Disabled = scenario.Disabled,
				LastStartTimeUtc = scenario.Disabled ? null : utcNow,
				LastHeartbeatUtc = scenario.Disabled ? null : utcNow,
				SyncToken = Guid.NewGuid()
			};

			var sql = $@"{DbScenario.GetInsertSql(_options)} 
ON CONFLICT (""{nameof(DbScenario.IdScenario)}"")
DO 
   UPDATE SET ""{nameof(DbScenario.IdHost)}"" = excluded.""{nameof(DbScenario.IdHost)}"",
			""{nameof(DbScenario.Name)}"" = excluded.""{nameof(DbScenario.Name)}"",
			""{nameof(DbScenario.Description)}"" = excluded.""{nameof(DbScenario.Description)}"",
			""{nameof(DbScenario.Disabled)}"" = excluded.""{nameof(DbScenario.Disabled)}"",
			{(scenario.Disabled ? "" : $@"""{nameof(DbScenario.LastStartTimeUtc)}"" = excluded.""{nameof(DbScenario.LastStartTimeUtc)}"",")}
			{(scenario.Disabled ? "" : $@"""{nameof(DbScenario.LastHeartbeatUtc)}"" = excluded.""{nameof(DbScenario.LastHeartbeatUtc)}"",")}
			""{nameof(DbScenario.SyncToken)}"" = excluded.""{nameof(DbScenario.SyncToken)}""";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			var table = DbScenario.GetDictionaryTable(_options);
			table.SetParameters(cmd, dbScenario.ToDictionary());

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(SaveScenario)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}

		public async Task UpdateScenarioAsync(Guid idScenario, bool? disabled, string? description, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		{
			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var sql = $@"
UPDATE {_options.HostDbSchemaName}.""{_options.HostDbTableName}""
SET {(disabled.HasValue ? $@" ""{nameof(DbScenario.Disabled)}"" = @disabled, " : "")}{(!string.IsNullOrEmpty(description) ? $@" ""{nameof(DbScenario.Description)}"" = @description, " : "")}""{nameof(DbScenario.LastHeartbeatUtc)}"" = @lastHeartbeatUtc, ""{nameof(DbScenario.SyncToken)}"" = @syncToken
WHERE ""{nameof(DbScenario.IdScenario)}"" = @idScenario;";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			if (disabled.HasValue)
				cmd.Parameters.AddWithValue("@disabled", NpgsqlDbType.Boolean, disabled.Value);

			if (!string.IsNullOrEmpty(description))
				cmd.Parameters.AddWithValue("@description", NpgsqlDbType.Varchar, description);

			cmd.Parameters.AddWithValue("@lastHeartbeatUtc", NpgsqlDbType.TimestampTz, DateTime.UtcNow);
			cmd.Parameters.AddWithValue("@syncToken", NpgsqlDbType.Uuid, Guid.NewGuid());
			cmd.Parameters.AddWithValue("@idScenario", NpgsqlDbType.Uuid, idScenario);

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateScenarioAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
