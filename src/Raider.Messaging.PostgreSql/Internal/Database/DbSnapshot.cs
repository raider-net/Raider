using Npgsql;
using Raider.Database.PostgreSql;
using Raider.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbSnapshot
	{
		public const string Snapshot = "Snapshot";

		private readonly DictionaryTable _table;

		public Guid? IdSnapshot { get; }
		public Guid? IdJobInstance { get; }
		public string? SnapshotIdentifier { get; }
		public DateTime? LastAccessUtc { get; }
		public string? Data { get; }

		public DbSnapshot()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(Snapshot),
				PropertyNames = new List<string>
				{
					nameof(IdSnapshot),
					nameof(IdJobInstance),
					nameof(SnapshotIdentifier),
					nameof(LastAccessUtc),
					nameof(Data)
				}
			});
		}

		public async Task InsertAsync<TData>(NpgsqlConnection connection, NpgsqlTransaction? transaction, Guid idJobInstance, ISnapshot<TData> snapshot, CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (snapshot == null)
				throw new ArgumentNullException(nameof(snapshot));

			var sql = _table.ToInsertSql();

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(IdSnapshot), snapshot.IdSnapshot },
					{ nameof(IdJobInstance), idJobInstance },
					{ nameof(SnapshotIdentifier), snapshot.SnapshotIdentifier },
					{ nameof(LastAccessUtc), snapshot.LastAccessUtc },
					{ nameof(Data), snapshot.Data?.Serialize() },
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}

		public async Task UpdateJobInstanceAsync<TData>(NpgsqlConnection connection, NpgsqlTransaction? transaction, Guid idJobInstance, ISnapshot<TData> snapshot, CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (snapshot == null)
				throw new ArgumentNullException(nameof(snapshot));

			var sql = _table.ToUpdateSql(new List<string>
				{
					nameof(IdJobInstance)
				},
				where: $"\"{nameof(IdSnapshot)}\"=@id AND \"{nameof(SnapshotIdentifier)}\"=@snapIdent");

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(IdJobInstance), idJobInstance },
					{ "@id", snapshot.IdSnapshot },
					{ "@snapIdent", snapshot.SnapshotIdentifier}
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}

		public async Task UpdateAsync<TData>(NpgsqlConnection connection, NpgsqlTransaction? transaction, Guid idJobInstance, ISnapshot<TData> snapshot, CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (snapshot == null)
				throw new ArgumentNullException(nameof(snapshot));

			var sql = _table.ToUpdateSql(new List<string>
				{
					nameof(LastAccessUtc),
					nameof(Data),
				},
				where: $"\"{nameof(IdSnapshot)}\"=@id AND \"{nameof(IdJobInstance)}\"=@idJobInst AND \"{nameof(SnapshotIdentifier)}\"=@snapIdent");

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(LastAccessUtc), snapshot.LastAccessUtc },
					{ nameof(Data), snapshot.Data?.Serialize() },
					{ "@id", snapshot.IdSnapshot },
					{ "@idJobInst", idJobInstance},
					{ "@snapIdent", snapshot.SnapshotIdentifier}
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
