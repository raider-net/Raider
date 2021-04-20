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
	internal class DbSubscriberMessage
	{
		public const string SubscriberMessage = "SubscriberMessage";

		private readonly DictionaryTable _table;

		public Guid? IdSubscriberMessage { get; }
		public Guid? IdMessage { get; }
		public int? IdSubscriber { get; }
		public Guid? IdSubscriberInstance { get; }
		public DateTime? LastAccessUtc { get; }
		public int? IdMessageState { get; }
		public string? Snapshot { get; }
		public int? RetryCount { get; }
		public DateTime? DelayedToUtc { get; }
		public Guid? ConcurrencyToken { get; }

		public DbSubscriberMessage()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(SubscriberMessage),
				PropertyNames = new List<string>
				{
					nameof(IdSubscriberMessage),
					nameof(IdMessage),
					nameof(IdSubscriber),
					nameof(IdSubscriberInstance),
					nameof(LastAccessUtc),
					nameof(IdMessageState),
					nameof(Snapshot),
					nameof(RetryCount),
					nameof(DelayedToUtc),
					nameof(ConcurrencyToken)
				}
			});
		}

		private static LoadedSubscriberMessage<TData> ReadSubscriberMessage<TData>(NpgsqlDataReader reader)
			where TData : IMessageData
		{
			//var idMessage = reader.GetValueOrDefault<Guid>(0);
			//var idPublisherInstance = reader.GetValueOrDefault<Guid>(1);
			//var idPreviousMessage = reader.GetValueOrDefault<Guid?>(2);
			//var createdUtc = reader.GetValueOrDefault<DateTime>(3);
			//var validToUtc = reader.GetValueOrDefault<DateTime?>(4);
			//var isRecovery = reader.GetValueOrDefault<bool>(5);
			//var idSubscriberMessage = reader.GetValueOrDefault<Guid>(6);
			//var idSubscriber = reader.GetValueOrDefault<int>(7);
			//var lastAccessUtc = reader.GetValueOrDefault<DateTime?>(8);
			//var idMessageState = reader.GetValueOrDefault<int>(9);
			//var snapshot = reader.GetValueOrDefault<int>(10);
			//var retryCount = reader.GetValueOrDefault<int>(11);
			//var delayedToUtc = reader.GetValueOrDefault<DateTime?>(12);
			//var concurrencyToken = reader.GetValueOrDefault<Guid>(13);

			var subscriberMessage = new LoadedSubscriberMessage<TData>
			{
				IdMessage = reader.GetValueOrDefault<Guid>(0),
				IdPublisherInstance = reader.GetValueOrDefault<Guid>(1),
				IdPreviousMessage = reader.GetValueOrDefault<Guid?>(2),
				CreatedUtc = reader.GetValueOrDefault<DateTime>(3),
				ValidToUtc = reader.GetValueOrDefault<DateTime?>(4),
				IsRecovery = reader.GetValueOrDefault<bool>(5),
				IdSubscriberMessage = reader.GetValueOrDefault<Guid>(6),
				IdSubscriber = reader.GetValueOrDefault<int>(7),
				LastAccessUtc = reader.GetValueOrDefault<DateTime?>(8),
				State = EnumHelper.ConvertIntToEnum<MessageState>(reader.GetValueOrDefault<int>(9)),
				Snapshot = reader.GetValueOrDefault<string>(10),
				RetryCount = reader.GetValueOrDefault<int>(11),
				DelayedToUtc = reader.GetValueOrDefault<DateTime?>(12),
				OriginalConcurrencyToken = reader.GetValueOrDefault<Guid>(13),
				//NewConcurrencyToken = Guid.NewGuid(), also in ctor
				Data = default
			};

			return subscriberMessage;
		}

		private static async Task<TData?> ReadMessageDataAsync<TData>(NpgsqlConnection connection, NpgsqlTransaction? transaction, Guid idMessage, CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			var sql = $@"SELECT ""Data"" FROM bus.""Message"" WHERE ""IdMessage"" = @idMsg";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			cmd.Parameters.AddWithValue("@idMsg", idMessage);

			string? json = null;
			using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
			{
				if (await reader.ReadAsync(cancellationToken))
				{
					json = reader.GetValueOrDefault<string>(0);
				}
			}

			return string.IsNullOrWhiteSpace(json)
				? default
				: System.Text.Json.JsonSerializer.Deserialize<TData>(json);
		}

		public async Task<ISubscriberMessage<TData>?> GetSubscriberMessageFromFIFOAsync<TData>(
			NpgsqlConnection connection,
			NpgsqlTransaction? transaction,
			ISubscriber<TData> subscriber,
			List<int> readMessageStates,
			DateTime utcNow,
			CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (readMessageStates == null || readMessageStates.Count == 0)
				return null;

			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			var inClausule = string.Join(", ", readMessageStates.Select(x => $"@s{x}"));

			var sql = $@"
SELECT
	m.""{nameof(DbMessage.IdMessage)}"",
	m.""{nameof(DbMessage.IdPublisherInstance)}"",
	m.""{nameof(DbMessage.IdPreviousMessage)}"",
	m.""{nameof(DbMessage.CreatedUtc)}"",
	m.""{nameof(DbMessage.ValidToUtc)}"",
	m.""{nameof(DbMessage.IsRecovery)}"",
	sm.""{nameof(IdSubscriberMessage)}"",
	sm.""{nameof(IdSubscriber)}"",
	sm.""{nameof(LastAccessUtc)}"",
	sm.""{nameof(IdMessageState)}"",
	sm.""{nameof(Snapshot)}"",
	sm.""{nameof(RetryCount)}"",
	sm.""{nameof(DelayedToUtc)}"",
	sm.""{nameof(ConcurrencyToken)}""
FROM {nameof(Defaults.Schema.bus)}.""{nameof(SubscriberMessage)}"" sm
JOIN {nameof(Defaults.Schema.bus)}.""{nameof(DbMessage.Message)}"" m ON sm.""{nameof(IdMessage)}"" = m.""{nameof(DbMessage.IdMessage)}""
WHERE sm.""{nameof(IdSubscriber)}"" = @idSubsc
	AND (sm.""{nameof(IdSubscriberInstance)}"" IS NULL OR sm.""{nameof(IdSubscriberInstance)}"" = @idSubscInst OR (sm.""{nameof(IdMessageState)}"" = 2 AND sm.""{nameof(LastAccessUtc)}"" < @lastAcc))
	AND sm.""{nameof(IdMessageState)}"" IN ({inClausule})
ORDER BY m.""{nameof(DbMessage.CreatedUtc)}""
LIMIT 1";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			cmd.Parameters.AddWithValue("@idSubsc", subscriber.IdComponent);
			cmd.Parameters.AddWithValue("@idSubscInst", subscriber.IdInstance);
			cmd.Parameters.AddWithValue("@lastAcc", (DateTime)utcNow.Subtract(subscriber.TimeoutForMessageProcessing));

			foreach (var readMessageState in readMessageStates)
				cmd.Parameters.AddWithValue($"@s{readMessageState}", readMessageState);

			LoadedSubscriberMessage<TData>? subscriberMessage = null;

			using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
			{
				if (await reader.ReadAsync(cancellationToken))
					subscriberMessage = ReadSubscriberMessage<TData>(reader);
			}

			if (subscriberMessage != null)
				subscriberMessage.Data = await ReadMessageDataAsync<TData>(connection, transaction, subscriberMessage.IdMessage, cancellationToken);

			return subscriberMessage;
		}

		public async Task<ISubscriberMessage<TData>?> GetSubscriberMessageFromNonFIFOAsync<TData>(
			NpgsqlConnection connection,
			NpgsqlTransaction? transaction,
			ISubscriber<TData> subscriber,
			List<int> readMessageStates,
			DateTime utcNow,
			CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (readMessageStates == null || readMessageStates.Count == 0)
				return null;

			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			var inClausule = string.Join(", ", readMessageStates.Select(x => $"@s{x}"));

			var sql = $@"
SELECT
	m.""{nameof(DbMessage.IdMessage)}"",
	m.""{nameof(DbMessage.IdPublisherInstance)}"",
	m.""{nameof(DbMessage.IdPreviousMessage)}"",
	m.""{nameof(DbMessage.CreatedUtc)}"",
	m.""{nameof(DbMessage.ValidToUtc)}"",
	m.""{nameof(DbMessage.IsRecovery)}"",
	sm.""{nameof(IdSubscriberMessage)}"",
	sm.""{nameof(IdSubscriber)}"",
	sm.""{nameof(LastAccessUtc)}"",
	sm.""{nameof(IdMessageState)}"",
	sm.""{nameof(Snapshot)}"",
	sm.""{nameof(RetryCount)}"",
	sm.""{nameof(DelayedToUtc)}"",
	sm.""{nameof(ConcurrencyToken)}""
FROM {nameof(Defaults.Schema.bus)}.""{nameof(SubscriberMessage)}"" sm
JOIN {nameof(Defaults.Schema.bus)}.""{nameof(DbMessage.Message)}"" m ON sm.""{nameof(IdMessage)}"" = m.""{nameof(DbMessage.IdMessage)}""
WHERE sm.""{nameof(IdSubscriber)}"" = @idSubsc
	AND (sm.""{nameof(IdSubscriberInstance)}"" IS NULL OR sm.""{nameof(IdSubscriberInstance)}"" = @idSubscInst OR (sm.""{nameof(IdMessageState)}"" = 2 AND sm.""{nameof(LastAccessUtc)}"" < @lastAcc))
	AND (sm.""{nameof(IdMessageState)}"" IN ({inClausule}) OR (sm.""{nameof(IdMessageState)}"" = 2 AND sm.""{nameof(LastAccessUtc)}"" < @lastAcc))
	AND (sm.""{nameof(DelayedToUtc)}"" IS NULL OR sm.""{nameof(DelayedToUtc)}"" < @delay)
ORDER BY m.""{nameof(DbMessage.CreatedUtc)}""
LIMIT 1";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			cmd.Parameters.AddWithValue("@idSubsc", subscriber.IdComponent);
			cmd.Parameters.AddWithValue("@idSubscInst", subscriber.IdInstance);

			foreach (var readMessageState in readMessageStates)
				cmd.Parameters.AddWithValue($"@s{readMessageState}", readMessageState);

			cmd.Parameters.AddWithValue("@lastAcc", (DateTime)utcNow.Subtract(subscriber.TimeoutForMessageProcessing));
			cmd.Parameters.AddWithValue("@delay", utcNow);

			LoadedSubscriberMessage<TData>? subscriberMessage = null;

			using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
			{
				if (await reader.ReadAsync(cancellationToken))
					subscriberMessage = ReadSubscriberMessage<TData>(reader);
			}

			if (subscriberMessage != null)
				subscriberMessage.Data = await ReadMessageDataAsync<TData>(connection, transaction, subscriberMessage.IdMessage, cancellationToken);

			return subscriberMessage;
		}

		public async Task InsertAsync<TData>(NpgsqlConnection connection, NpgsqlTransaction? transaction, List<int> subscriberIds, IMessage<TData> message, MessageState state, CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (subscriberIds == null || subscriberIds.Count == 0)
				throw new ArgumentNullException(nameof(subscriberIds));

			if (message == null)
				throw new ArgumentNullException(nameof(message));

			foreach (var idSubscriber in subscriberIds)
				await InsertAsync(connection, transaction, idSubscriber, message, state, cancellationToken);
		}

		public async Task InsertAsync<TData>(NpgsqlConnection connection, NpgsqlTransaction? transaction, int idSubscriber, IMessage<TData> message, MessageState state, CancellationToken cancellationToken = default)
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
					{ nameof(IdSubscriberMessage), guid },
					{ nameof(IdMessage), message.IdMessage },
					{ nameof(IdSubscriber), idSubscriber },
					{ nameof(IdSubscriberInstance), null },
					{ nameof(LastAccessUtc), message.CreatedUtc },
					{ nameof(IdMessageState), (int)state },
					{ nameof(Snapshot), null },
					{ nameof(RetryCount), 0 },
					{ nameof(DelayedToUtc), null },
					{ nameof(ConcurrencyToken), guid },
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}

		public async Task UpdateAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, Guid idSubscriberInstance, MessageResult messageResult, CancellationToken cancellationToken = default)
		{
			if (messageResult == null)
				throw new ArgumentNullException(nameof(messageResult));

			var sql = _table.ToUpdateSql(new List<string>
				{
					nameof(IdSubscriberInstance),
					nameof(LastAccessUtc),
					nameof(IdMessageState),
					nameof(Snapshot),
					nameof(RetryCount),
					nameof(DelayedToUtc),
					nameof(ConcurrencyToken),
				},
				where: $"\"{nameof(IdSubscriberMessage)}\"=@id AND (\"{nameof(IdSubscriberInstance)}\" IS NULL OR \"{nameof(ConcurrencyToken)}\"=@ct)");

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(IdSubscriberInstance), idSubscriberInstance },
					{ nameof(LastAccessUtc), messageResult.CreatedUtc },
					{ nameof(IdMessageState), (int)messageResult.State },
					{ nameof(Snapshot), messageResult.Snapshot },
					{ nameof(RetryCount), messageResult.RetryCount },
					{ nameof(DelayedToUtc), messageResult.DelayedToUtc },
					{ nameof(ConcurrencyToken), messageResult.NewConcurrencyToken },
					{ "@id", messageResult.IdSubscriberMessage },
					{ "@ct", messageResult.OriginalConcurrencyToken}
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
