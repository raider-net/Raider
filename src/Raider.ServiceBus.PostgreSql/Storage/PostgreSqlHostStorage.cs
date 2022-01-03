using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using Raider.Converters;
using Raider.Database.PostgreSql;
using Raider.Enums;
using Raider.Extensions;
using Raider.Infrastructure;
using Raider.Logging.Extensions;
using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Model;
using Raider.ServiceBus.PostgreSql.Messages.Storage.Model;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.PostgreSql.Storage
{
	internal abstract partial class PostgreSqlHostStorage : IHostLogger
	{
		private readonly IPostgreSqlBusOptions _options;
		private readonly ISerializer _serialzier;
		private readonly ILogger _logger;
		private readonly IHostLogger _baseHostLogger;

		protected IServiceProvider ServiceProvider { get; }

		public PostgreSqlHostStorage(IPostgreSqlBusOptions options, IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_options = options ?? throw new ArgumentNullException(nameof(options));

			var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

			_logger = loggerFactory.CreateLogger<ILogger<PostgreSqlHostStorage>>();

			var msHostLogger = loggerFactory.CreateLogger<BaseHostLogger>();
			_baseHostLogger = new BaseHostLogger(msHostLogger);

			_serialzier = _options.MessageSerializer(serviceProvider);

			if (_serialzier == null)
				throw new InvalidOperationException($"{nameof(_serialzier)} == NULL");
		}

		public ITransactionContext CreateTransactionContext()
		{
			var connection = new NpgsqlConnection(_options.ConnectionString);
			connection.Open();
			var transactionContext = connection.BeginTransactionContext();
			return transactionContext;
		}

		public async Task<ITransactionContext> CreateTransactionContextAsync(CancellationToken cancellationToken = default)
		{
			var connection = new NpgsqlConnection(_options.ConnectionString);
			await connection.OpenAsync(cancellationToken);
			var transactionContext = await connection.BeginTransactionContextAsync(cancellationToken);
			return transactionContext;
		}

		protected virtual async Task<IHost> InitializeHostInternalAsync(HostType hostType, List<IDictionary<string, object?>>? messageTypes, CancellationToken cancellationToken = default)
		{
			var transactionContext = await CreateTransactionContextAsync(cancellationToken);
			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));

			try
			{
				await SaveHostTypes(transactionContext, cancellationToken);
				var host = await CreateHost(hostType, transactionContext, cancellationToken);
				await SaveMessageTypes(messageTypes, transactionContext, cancellationToken);

				await LogInformationAsync(
					TraceInfo.Create(),
					host.IdHost,
					HostStatus.Online,
					x => x.Detail("START"),
					"START",
					transactionContext,
					cancellationToken);

				await transactionContext.CommitAsync(cancellationToken);

				return host;
			}
			catch (Exception ex)
			{
				_logger.LogCriticalMessage(x => x.ExceptionInfo(ex).Detail($"{nameof(PostgreSqlHostStorage)}.{nameof(InitializeHostInternalAsync)} error."));

				try
				{
					await transactionContext.RollbackAsync(cancellationToken);
				}
				catch (Exception exRolback)
				{
					_logger.LogCriticalMessage(x => x.ExceptionInfo(exRolback).Detail($"{nameof(PostgreSqlHostStorage)}.{nameof(InitializeHostInternalAsync)} {nameof(transactionContext)}.{nameof(transactionContext.RollbackAsync)}"));
				}
				throw;
			}
			finally
			{
				try
				{
					await transactionContext.DisposeAsync();
				}
				catch (Exception ex)
				{
					_logger.LogErrorMessage(x => x.ExceptionInfo(ex).Detail($"{nameof(PostgreSqlHostStorage)}.{nameof(InitializeHostInternalAsync)} {nameof(transactionContext)}.{nameof(transactionContext.DisposeAsync)}"));
				}

				try
				{
					await connection.DisposeAsync();
				}
				catch (Exception ex)
				{
					_logger.LogErrorMessage(x => x.ExceptionInfo(ex).Detail($"{nameof(PostgreSqlHostStorage)}.{nameof(InitializeHostInternalAsync)} {nameof(connection)}.{nameof(transactionContext.DisposeAsync)}"));
				}
			}
		}

		private async Task SaveHostTypes(ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		{
			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var tmpTableName = await connection.CopyTableAsTempIfNotExistsAsync(transaction, _options.HostTypeDbSchemaName, _options.HostTypeDbTableName, cancellationToken: cancellationToken);

			var bulkInsert =
				new BulkInsert(
					new DictionaryTableOptions
					{
						SchemaName = null,
						IsTemporaryTable = true,
						TableName = tmpTableName,
						PropertyNames = DbHostType.PropertyNames,
						PropertyTypeMapping = DbHostType.PropertyTypeMapping
					},
				connection);

			var data = 
				EnumHelper.GetAllEnumValues<HostType>()
				.Select(x =>
					new DbHostType
					{
						IdHostType = GuidConverter.ToGuid((int)x),
						Name = x.ToString()
					}.ToDictionary())
				.ToList();
			await bulkInsert.WriteBatchAsync(data, false, false, cancellationToken);

			var upsert = @$"
INSERT INTO {_options.HostTypeDbSchemaName}.""{_options.HostTypeDbTableName}""
	(""{nameof(DbHostType.IdHostType)}"", ""{nameof(DbHostType.Name)}"", ""{nameof(DbHostType.Description)}"")
SELECT ""{nameof(DbHostType.IdHostType)}"", ""{nameof(DbHostType.Name)}"", ""{nameof(DbHostType.Description)}"" FROM {tmpTableName}
ON CONFLICT (""{nameof(DbHostType.IdHostType)}"") 
DO NOTHING;";

			using var upsertCmd = new NpgsqlCommand(upsert, connection);
			if (transaction != null)
				upsertCmd.Transaction = transaction;

			await upsertCmd.ExecuteNonQueryAsync(cancellationToken);

			await connection.DropTableAsync(transaction, tmpTableName, cancellationToken);
		}

		private async Task<IHost> CreateHost(HostType hostType, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		{
			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var utcNow = DateTime.UtcNow;
			var dbHost = new DbHost
			{
				IdHost = GuidConverter.ToGuid(_options.Name),
				Name = _options.Name,
				IdHostType = GuidConverter.ToGuid((int)hostType),
				CurrentRuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
				LastStartTimeUtc = utcNow,
				LastHeartbeatUtc = utcNow,
				IdHostStatus = (int)HostStatus.Online,
				SyncToken = Guid.NewGuid(),
			};


			var sql = $@"{DbHost.GetInsertSql(_options)} 
ON CONFLICT (""{nameof(DbHost.IdHost)}"") 
DO 
   UPDATE SET ""{nameof(DbHost.CurrentRuntimeUniqueKey)}"" = excluded.""{nameof(DbHost.CurrentRuntimeUniqueKey)}"",
			""{nameof(DbHost.LastStartTimeUtc)}"" = excluded.""{nameof(DbHost.LastStartTimeUtc)}"",
			""{nameof(DbHost.IdHostStatus)}"" = excluded.""{nameof(DbHost.IdHostStatus)}"",
			""{nameof(DbHost.SyncToken)}"" = excluded.""{nameof(DbHost.SyncToken)}""";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			var table = DbHost.GetDictionaryTable(_options);
			table.SetParameters(cmd, dbHost.ToDictionary());

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(CreateHost)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");

			return dbHost;
		}

		private async Task SaveMessageTypes(List<IDictionary<string, object?>>? messageTypes, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		{
			if (messageTypes == null || messageTypes.Count == 0)
				return;

			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var tmpTableName = await connection.CopyTableAsTempIfNotExistsAsync(transaction, _options.MessageTypeDbSchemaName, _options.MessageTypeDbTableName, cancellationToken: cancellationToken);

			var bulkInsert =
				new BulkInsert(
					new DictionaryTableOptions
					{
						SchemaName = null,
						IsTemporaryTable = true,
						TableName = tmpTableName,
						PropertyNames = DbMessageType.PropertyNames,
						PropertyTypeMapping = DbMessageType.PropertyTypeMapping
					},
				connection);

			await bulkInsert.WriteBatchAsync(messageTypes, false, false, cancellationToken);

			var upsert = @$"
INSERT INTO {_options.MessageTypeDbSchemaName}.""{_options.MessageTypeDbTableName}""
(""{nameof(IMessageType.IdMessageType)}"", ""{nameof(IMessageType.Name)}"", ""{nameof(IMessageType.Description)}"", ""{nameof(IMessageType.IdMessageMetaType)}"", ""{nameof(IMessageType.CrlType)}"")
SELECT ""{nameof(IMessageType.IdMessageType)}"", ""{nameof(IMessageType.Name)}"", ""{nameof(IMessageType.Description)}"", ""{nameof(IMessageType.IdMessageMetaType)}"", ""{nameof(IMessageType.CrlType)}"" FROM {tmpTableName}
ON CONFLICT (""{nameof(IMessageType.IdMessageType)}"") 
DO NOTHING;";

			using var upsertCmd = new NpgsqlCommand(upsert, connection);
			if (transaction != null)
				upsertCmd.Transaction = transaction;

			await upsertCmd.ExecuteNonQueryAsync(cancellationToken);

			await connection.DropTableAsync(transaction, tmpTableName, cancellationToken);
		}

		private void UpdateHost(Guid idHost, HostStatus hostStatus, bool? disabled, ITransactionContext transactionContext)
		{
			if (!disabled.HasValue && hostStatus == HostStatus.Unchanged)
				return;

			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var sql = $@"
UPDATE {_options.HostDbSchemaName}.""{_options.HostDbTableName}""
SET {(disabled.HasValue ? $@" ""{nameof(DbHost.Disabled)}"" = @disabled, " : "")}{(hostStatus != HostStatus.Unchanged ? $@"""{nameof(DbHost.IdHostStatus)}"" = @idHostStatus, " : "")}""{nameof(DbHost.SyncToken)}"" = @syncToken
WHERE ""{nameof(DbHost.IdHost)}"" = @idHost;";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			if (disabled.HasValue)
				cmd.Parameters.AddWithValue("@disabled", NpgsqlDbType.Boolean, disabled.Value);

			if (hostStatus != HostStatus.Unchanged)
				cmd.Parameters.AddWithValue("@idHostStatus", NpgsqlDbType.Integer, (int)hostStatus);

			cmd.Parameters.AddWithValue("@syncToken", NpgsqlDbType.Uuid, Guid.NewGuid());
			cmd.Parameters.AddWithValue("@idHost", NpgsqlDbType.Uuid, idHost);

			var result = cmd.ExecuteNonQuery();

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateHost)}: {nameof(cmd.ExecuteNonQuery)} returns {result}");
		}

		private async Task UpdateHostAsync(Guid idHost, HostStatus hostStatus, bool? disabled, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		{
			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
			var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

			var sql = $@"
UPDATE {_options.HostDbSchemaName}.""{_options.HostDbTableName}""
SET {(disabled.HasValue ? $@" ""{nameof(DbHost.Disabled)}"" = @disabled, " : "")}{(hostStatus != HostStatus.Unchanged ? $@"""{nameof(DbHost.IdHostStatus)}"" = @idHostStatus, " : "")}""{nameof(DbHost.LastHeartbeatUtc)}"" = @lastHeartbeatUtc, ""{nameof(DbHost.SyncToken)}"" = @syncToken
WHERE ""{nameof(DbHost.IdHost)}"" = @idHost;";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			if (disabled.HasValue)
				cmd.Parameters.AddWithValue("@disabled", NpgsqlDbType.Boolean, disabled.Value);

			if (hostStatus != HostStatus.Unchanged)
				cmd.Parameters.AddWithValue("@idHostStatus", NpgsqlDbType.Integer, (int)hostStatus);

			cmd.Parameters.AddWithValue("@lastHeartbeatUtc", NpgsqlDbType.TimestampTz, DateTime.UtcNow);
			cmd.Parameters.AddWithValue("@syncToken", NpgsqlDbType.Uuid, Guid.NewGuid());
			cmd.Parameters.AddWithValue("@idHost", NpgsqlDbType.Uuid, idHost);

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateHostAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
