using Npgsql;
using Raider.Database.PostgreSql;
using Raider.Enums;
using Raider.Extensions;
using Raider.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbMessageTempQueue
	{
		public const string MessageTempQueue = "MessageTempQueue";

		private readonly DictionaryTable _table;

		public Guid? IdMessageTempQueue { get; }
		public Guid? IdMessage { get; }
		public DateTime? LastAccessUtc { get; }
		public int? IdMessageState { get; }
		public int? RetryCount { get; }
		public DateTime? DelayedToUtc { get; }
		public Guid? ConcurrencyToken { get; }

		public DbMessageTempQueue()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(MessageTempQueue),
				PropertyNames = new List<string>
				{
					nameof(IdMessageTempQueue),
					nameof(IdMessage),
					nameof(LastAccessUtc),
					nameof(IdMessageState),
					nameof(RetryCount),
					nameof(DelayedToUtc),
					nameof(ConcurrencyToken)
				}
			});
		}

		private static LoadedMessageTempQueue ReadMessageTempQueue(NpgsqlDataReader reader)
		{
			var loadedMessageTempQueue = new LoadedMessageTempQueue
			{
				IdMessageTempQueue = reader.GetValueOrDefault<Guid>(0),
				IdMessage = reader.GetValueOrDefault<Guid>(1),
				LastAccessUtc = reader.GetValueOrDefault<DateTime?>(2),
				State = EnumHelper.ConvertIntToEnum<MessageState>(reader.GetValueOrDefault<int>(3)),
				RetryCount = reader.GetValueOrDefault<int>(4),
				DelayedToUtc = reader.GetValueOrDefault<DateTime?>(5),
				OriginalConcurrencyToken = reader.GetValueOrDefault<Guid>(6),
				//NewConcurrencyToken = Guid.NewGuid(), also in ctor
			};

			return loadedMessageTempQueue;
		}

		public async Task<LoadedMessageTempQueue?> GetMessageTempQueueAsync(
			NpgsqlConnection connection,
			NpgsqlTransaction? transaction,
			List<int> readMessageStates,
			DateTime utcNow,
			TimeSpan timeoutForMessageProcessing,
			CancellationToken cancellationToken = default)
		{
			if (readMessageStates == null || readMessageStates.Count == 0)
				return null;

			var inClausule = string.Join(", ", readMessageStates.Select(x => $"@s{x}"));

			var sql = $@"
SELECT
	mtq.""{nameof(IdMessageTempQueue)}"",
	mtq.""{nameof(IdMessage)}"",
	mtq.""{nameof(LastAccessUtc)}"",
	mtq.""{nameof(IdMessageState)}"",
	mtq.""{nameof(RetryCount)}"",
	mtq.""{nameof(DelayedToUtc)}"",
	mtq.""{nameof(ConcurrencyToken)}""
FROM {nameof(Defaults.Schema.bus)}.""{nameof(MessageTempQueue)}"" mtq
JOIN {nameof(Defaults.Schema.bus)}.""{nameof(DbMessage.Message)}"" m ON mtq.""{nameof(IdMessage)}"" = m.""{nameof(DbMessage.IdMessage)}""
WHERE 
	(mtq.""{nameof(IdMessageState)}"" IN ({inClausule}) OR (mtq.""{nameof(IdMessageState)}"" = 2 AND mtq.""{nameof(LastAccessUtc)}"" < @lastAcc))
	AND (mtq.""{nameof(DelayedToUtc)}"" IS NULL OR mtq.""{nameof(DelayedToUtc)}"" < @delay)
ORDER BY m.""{nameof(DbMessage.CreatedUtc)}""
LIMIT 1";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			foreach (var readMessageState in readMessageStates)
				cmd.Parameters.AddWithValue($"@s{readMessageState}", readMessageState);

			cmd.Parameters.AddWithValue("@lastAcc", (DateTime)utcNow.Subtract(timeoutForMessageProcessing));
			cmd.Parameters.AddWithValue("@delay", utcNow);

			LoadedMessageTempQueue? loadedMessageTempQueue = null;

			using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
			{
				if (await reader.ReadAsync(cancellationToken))
					loadedMessageTempQueue = ReadMessageTempQueue(reader);
			}

			return loadedMessageTempQueue;
		}

		public async Task InsertAsync<TData>(NpgsqlConnection connection, NpgsqlTransaction? transaction, IMessage<TData> message, MessageState state, CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			var sql = _table.ToInsertSql();

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			var guid = Guid.NewGuid();
			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(IdMessageTempQueue), guid },
					{ nameof(IdMessage), message.IdMessage },
					{ nameof(LastAccessUtc), message.CreatedUtc },
					{ nameof(IdMessageState), (int)state },
					{ nameof(RetryCount), 0 },
					{ nameof(DelayedToUtc), null },
					{ nameof(ConcurrencyToken), guid },
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}

		public async Task UpdateAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, Guid idMessageTempQueue, Guid originalConcurrencyToken, MessageResult messageResult, CancellationToken cancellationToken = default)
		{
			if (messageResult == null)
				throw new ArgumentNullException(nameof(messageResult));

			var sql = _table.ToUpdateSql(new List<string>
				{
					nameof(LastAccessUtc),
					nameof(IdMessageState),
					nameof(RetryCount),
					nameof(DelayedToUtc),
					nameof(ConcurrencyToken),
				},
				where: $"\"{nameof(IdMessageTempQueue)}\"=@id AND \"{nameof(ConcurrencyToken)}\"=@ct");

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(LastAccessUtc), messageResult.CreatedUtc },
					{ nameof(IdMessageState), (int)messageResult.State },
					{ nameof(RetryCount), messageResult.RetryCount },
					{ nameof(DelayedToUtc), messageResult.DelayedToUtc },
					{ nameof(ConcurrencyToken), Guid.NewGuid() },
					{ "@id", idMessageTempQueue },
					{ "@ct", originalConcurrencyToken}
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
