using Npgsql;
using NpgsqlTypes;
using Raider.ServiceBus.Components;
using Raider.ServiceBus.Config.Components;
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
		private async Task SaveComponent(IComponent component, Guid idScenario, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		{
			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var utcNow = DateTime.UtcNow;
			var dbComponent = new DbComponent
			{
				IdComponent = component.IdComponent,
				IdScenario = idScenario,
				Name = component.Name,
				CrlType = component.ResolvedCrlType,
				Description = component.Description,
				ThrottleDelayInMilliseconds = component.ThrottleDelayInMilliseconds,
				InactivityTimeoutInSeconds = component.InactivityTimeoutInSeconds,
				ShutdownTimeoutInSeconds = component.ShutdownTimeoutInSeconds,
				IdCurrentSession = null,
				IdComponentStatus = (int)ComponentStatus.Idle,
				LastStartTimeUtc = utcNow,
				LastHeartbeatUtc = utcNow,
				SyncToken = Guid.NewGuid()
			};

			var sql = $@"{DbComponent.GetInsertSql(_options)} 
ON CONFLICT (""{nameof(DbComponent.IdComponent)}"")
DO 
   UPDATE SET ""{nameof(DbComponent.IdScenario)}"" = excluded.""{nameof(DbComponent.IdScenario)}"",
			""{nameof(DbComponent.Name)}"" = excluded.""{nameof(DbComponent.Name)}"",
			""{nameof(DbComponent.CrlType)}"" = excluded.""{nameof(DbComponent.CrlType)}"",
			""{nameof(DbComponent.Description)}"" = excluded.""{nameof(DbComponent.Description)}"",
			""{nameof(DbComponent.ThrottleDelayInMilliseconds)}"" = excluded.""{nameof(DbComponent.ThrottleDelayInMilliseconds)}"",
			""{nameof(DbComponent.InactivityTimeoutInSeconds)}"" = excluded.""{nameof(DbComponent.InactivityTimeoutInSeconds)}"",
			""{nameof(DbComponent.ShutdownTimeoutInSeconds)}"" = excluded.""{nameof(DbComponent.ShutdownTimeoutInSeconds)}"",
			""{nameof(DbComponent.IdCurrentSession)}"" = excluded.""{nameof(DbComponent.IdCurrentSession)}"",
			""{nameof(DbComponent.IdComponentStatus)}"" = excluded.""{nameof(DbComponent.IdComponentStatus)}"",
			""{nameof(DbComponent.LastStartTimeUtc)}"" = excluded.""{nameof(DbComponent.LastStartTimeUtc)}"",
			""{nameof(DbComponent.LastHeartbeatUtc)}"" = excluded.""{nameof(DbComponent.LastHeartbeatUtc)}"",
			""{nameof(DbComponent.SyncToken)}"" = excluded.""{nameof(DbComponent.SyncToken)}""";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			var table = DbComponent.GetDictionaryTable(_options);
			table.SetParameters(cmd, dbComponent.ToDictionary());

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(SaveComponent)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}

		private async Task UpdateComponentStatusAsync(Guid idComponent, ComponentStatus componentStatus, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		{
			if (componentStatus == ComponentStatus.Unchanged)
				return;

			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var sql = $@"
UPDATE {_options.ComponentDbSchemaName}.""{_options.ComponentDbTableName}""
SET {(componentStatus != ComponentStatus.Unchanged ? $@"""{nameof(DbComponent.IdComponentStatus)}"" = @idComponentStatus, " : "")}""{nameof(DbComponent.LastHeartbeatUtc)}"" = @lastHeartbeatUtc, ""{nameof(DbComponent.SyncToken)}"" = @syncToken
WHERE ""{nameof(DbComponent.IdComponent)}"" = @idComponent;";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			if (componentStatus != ComponentStatus.Unchanged)
				cmd.Parameters.AddWithValue("@idComponentStatus", NpgsqlDbType.Integer, (int)componentStatus);

			cmd.Parameters.AddWithValue("@lastHeartbeatUtc", NpgsqlDbType.TimestampTz, DateTime.UtcNow);
			cmd.Parameters.AddWithValue("@syncToken", NpgsqlDbType.Uuid, Guid.NewGuid());
			cmd.Parameters.AddWithValue("@idComponent", NpgsqlDbType.Uuid, idComponent);

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateComponentStatusAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}

		public async Task UpdateComponentMetadataAsync(
			Guid idComponent,
			string name,
			string? description,
			int throttleDelayInMilliseconds,
			int inactivityTimeoutInSeconds,
			int shutdownTimeoutInSeconds,
			ITransactionContext transactionContext,
			CancellationToken cancellationToken = default)
		{
			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var sql = $@"
UPDATE {_options.ComponentDbSchemaName}.""{_options.ComponentDbTableName}""
SET {(!string.IsNullOrWhiteSpace(name) ? $@"""{nameof(DbComponent.Name)}"" = @name, " : "")}
	{(!string.IsNullOrEmpty(description) ? $@" ""{nameof(DbScenario.Description)}"" = @description, " : "")}
	""{nameof(DbComponent.ThrottleDelayInMilliseconds)}"" = @throttleDelayInMilliseconds,
	""{nameof(DbComponent.InactivityTimeoutInSeconds)}"" = @inactivityTimeoutInSeconds,
	""{nameof(DbComponent.ShutdownTimeoutInSeconds)}"" = @shutdownTimeoutInSeconds,
	""{nameof(DbComponent.LastHeartbeatUtc)}"" = @lastHeartbeatUtc,
	""{nameof(DbComponent.SyncToken)}"" = @syncToken
WHERE ""{nameof(DbComponent.IdComponent)}"" = @idComponent;";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			if (!string.IsNullOrWhiteSpace(name))
				cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Varchar, name);

			if (!string.IsNullOrEmpty(description))
				cmd.Parameters.AddWithValue("@description", NpgsqlDbType.Varchar, description);

			cmd.Parameters.AddWithValue("@throttleDelayInMilliseconds", NpgsqlDbType.Integer, throttleDelayInMilliseconds);
			cmd.Parameters.AddWithValue("@inactivityTimeoutInSeconds", NpgsqlDbType.Integer, inactivityTimeoutInSeconds);
			cmd.Parameters.AddWithValue("@shutdownTimeoutInSeconds", NpgsqlDbType.Integer, shutdownTimeoutInSeconds);
			cmd.Parameters.AddWithValue("@lastHeartbeatUtc", NpgsqlDbType.TimestampTz, DateTime.UtcNow);
			cmd.Parameters.AddWithValue("@syncToken", NpgsqlDbType.Uuid, Guid.NewGuid());
			cmd.Parameters.AddWithValue("@idComponent", NpgsqlDbType.Uuid, idComponent);

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateComponentStatusAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");

			var metadata = $"Updated metadata: {nameof(name)} = {name} | {nameof(description)} = {description} | {nameof(throttleDelayInMilliseconds)} = {throttleDelayInMilliseconds} | {nameof(inactivityTimeoutInSeconds)} = {inactivityTimeoutInSeconds} | {nameof(shutdownTimeoutInSeconds)} = {shutdownTimeoutInSeconds}";
			await LogInformationAsync(
				TraceInfo.Create(),
				idComponent,
				ComponentStatus.Unchanged,
				x => x.Detail(metadata),
				metadata,
				transactionContext,
				cancellationToken);
		}
	}
}
