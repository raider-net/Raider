using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Database.PostgreSql
{
	public static class PostgreSqlCommands
	{
		public static async Task<bool> ExistsAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, string tableName, CancellationToken cancellationToken = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrWhiteSpace(tableName))
				throw new ArgumentNullException(nameof(tableName));

			var cmd = new NpgsqlCommand($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{tableName}')", connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			var existsObj = await cmd.ExecuteScalarAsync(cancellationToken);

			if (existsObj is bool exists)
			{
				return exists;
			}
			else
			{
				throw new InvalidOperationException($"Invalid {nameof(existsObj)} = {existsObj}");
			}
		}

		public static async Task<bool> ExistsAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, string schemaName, string tableName, CancellationToken cancellationToken = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrWhiteSpace(schemaName))
				throw new ArgumentNullException(nameof(schemaName));

			if (string.IsNullOrWhiteSpace(tableName))
				throw new ArgumentNullException(nameof(tableName));

			var cmd = new NpgsqlCommand($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = '{schemaName}' AND table_name = '{tableName}')", connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			var existsObj = await cmd.ExecuteScalarAsync(cancellationToken);

			if (existsObj is bool exists)
			{
				return exists;
			}
			else
			{
				throw new InvalidOperationException($"Invalid {nameof(existsObj)} = {existsObj}");
			}
		}

		public static string GetRandomTmpTableName()
			=> $"tmp_{Guid.NewGuid():N}";

		public static Task<string> CopyTableAsTempIfNotExistsAsync(
			NpgsqlConnection connection,
			NpgsqlTransaction? transaction,
			string sourceSchemaName,
			string sourceTableName,
			TmpTableCommitOptions commitOptions,
			bool copyWithData,
			bool truncateDataIfAny,
			CancellationToken cancellationToken = default)
			=> CopyTableAsTempIfNotExistsAsync(connection, transaction, sourceSchemaName, sourceTableName, GetRandomTmpTableName(), commitOptions, copyWithData, truncateDataIfAny, cancellationToken);

		public static async Task<string> CopyTableAsTempIfNotExistsAsync(
			NpgsqlConnection connection,
			NpgsqlTransaction? transaction,
			string sourceSchemaName,
			string sourceTableName,
			string tmpTableName,
			TmpTableCommitOptions commitOptions,
			bool copyWithData,
			bool truncateDataIfAny,
			CancellationToken cancellationToken = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			var exists = await ExistsAsync(connection, transaction, tmpTableName, cancellationToken);
			if (exists)
			{
				if (truncateDataIfAny)
				{
					await TruncateTableAsync(connection, transaction, tmpTableName, false, cancellationToken);
				}

				return tmpTableName;
			}

			exists = await ExistsAsync(connection, transaction, sourceSchemaName, sourceTableName, cancellationToken);
			if (!exists)
				throw new InvalidOperationException($"Source {sourceSchemaName}.{sourceTableName} does not exists");

			var options = commitOptions switch
			{
				TmpTableCommitOptions.PreserveRows => "on commit preserve rows",
				TmpTableCommitOptions.DeleteRows => "on commit delete rows",
				TmpTableCommitOptions.Drop => "on commit drop",
				_ => throw new ArgumentException($"Invalid {nameof(commitOptions)}")
			};

			var cloneCommandText = @$" 
SET client_min_messages TO WARNING;

CREATE TEMPORARY TABLE ""{tmpTableName}"" {options}
AS
SELECT * FROM {sourceSchemaName}.""{sourceTableName}""{(copyWithData ? "" : " WHERE 1 = 2")};
";
			using var cloneCommand = new NpgsqlCommand(cloneCommandText, connection);
			if (transaction != null)
				cloneCommand.Transaction = transaction;

			await cloneCommand.ExecuteNonQueryAsync(cancellationToken);

			return tmpTableName;
		}

		public static Task TruncateTableAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, string tableName, bool cascade, CancellationToken cancellationToken = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrWhiteSpace(tableName))
				throw new ArgumentNullException(nameof(tableName));

			using var truncCommand = new NpgsqlCommand(@$"TRUNCATE TABLE ""{tableName}""{(cascade ? " CASCADE" : "")}", connection);
			if (transaction != null)
				truncCommand.Transaction = transaction;

			return truncCommand.ExecuteNonQueryAsync(cancellationToken);
		}

		public static Task TruncateTableAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, string schemaName, string tableName, bool cascade, CancellationToken cancellationToken = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrWhiteSpace(schemaName))
				throw new ArgumentNullException(nameof(schemaName));

			if (string.IsNullOrWhiteSpace(tableName))
				throw new ArgumentNullException(nameof(tableName));

			using var truncCommand = new NpgsqlCommand(@$"TRUNCATE TABLE {schemaName}.""{tableName}""{(cascade ? " CASCADE" : "")}", connection);
			if (transaction != null)
				truncCommand.Transaction = transaction;

			return truncCommand.ExecuteNonQueryAsync(cancellationToken);
		}

		public static Task DropTableAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, string tableName, CancellationToken cancellationToken = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrWhiteSpace(tableName))
				throw new ArgumentNullException(nameof(tableName));

			using var dropCommand = new NpgsqlCommand(@$"DROP TABLE ""{tableName}""", connection);
			if (transaction != null)
				dropCommand.Transaction = transaction;

			return dropCommand.ExecuteNonQueryAsync(cancellationToken);
		}

		public static Task DropTableAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, string schemaName, string tableName, CancellationToken cancellationToken = default)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (string.IsNullOrWhiteSpace(schemaName))
				throw new ArgumentNullException(nameof(schemaName));

			if (string.IsNullOrWhiteSpace(tableName))
				throw new ArgumentNullException(nameof(tableName));

			using var dropCommand = new NpgsqlCommand(@$"DROP TABLE {schemaName}.""{tableName}""", connection);
			if (transaction != null)
				dropCommand.Transaction = transaction;

			return dropCommand.ExecuteNonQueryAsync(cancellationToken);
		}
	}
}
