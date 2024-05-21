using Raider.Database.PostgreSql;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql
{
	public static class NpgsqlConnectionExtensions
	{
		public static Task<bool> ExistsAsync(this NpgsqlConnection connection, NpgsqlTransaction? transaction, string tableName, CancellationToken cancellationToken = default)
			=> PostgreSqlCommands.ExistsAsync(connection, transaction, tableName, cancellationToken);

		public static Task<bool> ExistsAsync(this NpgsqlConnection connection, NpgsqlTransaction? transaction, string schemaName, string tableName, CancellationToken cancellationToken = default)
			=> PostgreSqlCommands.ExistsAsync(connection, transaction, schemaName, tableName, cancellationToken);

		public static Task<string> CopyTableAsTempIfNotExistsAsync(
			this NpgsqlConnection connection,
			NpgsqlTransaction? transaction,
			string sourceSchemaName,
			string sourceTableName,
			TmpTableCommitOptions commitOptions = TmpTableCommitOptions.PreserveRows,
			bool copyWithData = false,
			bool truncateDataIfAny = true,
			CancellationToken cancellationToken = default)
			=> PostgreSqlCommands.CopyTableAsTempIfNotExistsAsync(connection, transaction, sourceSchemaName, sourceTableName, commitOptions, copyWithData, truncateDataIfAny, cancellationToken);

		public static Task<string> CopyTableAsTempIfNotExistsAsync(
			this NpgsqlConnection connection,
			NpgsqlTransaction? transaction,
			string sourceSchemaName,
			string sourceTableName,
			string tmpTableName,
			TmpTableCommitOptions commitOptions,
			bool copyWithData,
			bool truncateDataIfAny,
			CancellationToken cancellationToken = default)
			=> PostgreSqlCommands.CopyTableAsTempIfNotExistsAsync(connection, transaction, sourceSchemaName, sourceTableName, tmpTableName, commitOptions, copyWithData, truncateDataIfAny, cancellationToken);

		public static Task TruncateTableAsync(this NpgsqlConnection connection, NpgsqlTransaction? transaction, string tableName, bool cascade, CancellationToken cancellationToken = default)
			=> PostgreSqlCommands.TruncateTableAsync(connection, transaction, tableName, cascade, cancellationToken);

		public static Task TruncateTableAsync(this NpgsqlConnection connection, NpgsqlTransaction? transaction, string schemaName, string tableName, bool cascade, CancellationToken cancellationToken = default)
			=> PostgreSqlCommands.TruncateTableAsync(connection, transaction, schemaName, tableName, cascade, cancellationToken);

		public static Task DropTableAsync(this NpgsqlConnection connection, NpgsqlTransaction? transaction, string schemaName, string tableName, CancellationToken cancellationToken = default)
			=> PostgreSqlCommands.DropTableAsync(connection, transaction, schemaName, tableName, cancellationToken);

		public static Task DropTableAsync(this NpgsqlConnection connection, NpgsqlTransaction? transaction, string tableName, CancellationToken cancellationToken = default)
			=> PostgreSqlCommands.DropTableAsync(connection, transaction, tableName, cancellationToken);

		public static ITransactionContext BeginTransactionAndAttachToTransactionContext(this NpgsqlConnection connection, ITransactionContext transactionContext)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			var transaction = connection.BeginTransaction();
			transaction.AttachToTransactionContext(transactionContext);
			return transactionContext;
		}

		public static ITransactionContext BeginTransactionContext(this NpgsqlConnection connection)
			=> BeginTransactionContext(connection, null);

		public static ITransactionContext BeginTransactionContext(this NpgsqlConnection connection, Action<TransactionContextBuilder>? configure)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			var transaction = connection.BeginTransaction();
			var transactionContext = transaction.ToTransactionContext(configure);
			return transactionContext;
		}

#if NET5_0_OR_GREATER
		public static async Task<NpgsqlTransaction> BeginTransactionAndAttachToTransactionContextAsync(this NpgsqlConnection connection, ITransactionContext transactionContext, CancellationToken cancellationToken = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			var transaction = await connection.BeginTransactionAsync(cancellationToken);
			transaction.AttachToTransactionContext(transactionContext);
			return transaction;
		}

		public static Task<ITransactionContext> BeginTransactionContextAsync(this NpgsqlConnection connection, CancellationToken cancellationToken = default)
			=> BeginTransactionContextAsync(connection, null, cancellationToken);

		public static async Task<ITransactionContext> BeginTransactionContextAsync(this NpgsqlConnection connection, Action<TransactionContextBuilder>? configure, CancellationToken cancellationToken = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			var transaction = await connection.BeginTransactionAsync(cancellationToken);
			var transactionContext = transaction.ToTransactionContext(configure);
			return transactionContext;
		}
#endif
	}
}
