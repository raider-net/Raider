using Npgsql;
using Raider.Database.PostgreSql;
using Raider.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbMessage
	{
		public const string Message = "Message";

		private readonly DictionaryTable _table;

		public Guid? IdMessage { get; }
		public int? IdMessageType { get; }
		public Guid? IdPublisherInstance { get; }
		public Guid? IdPreviousMessage { get; }
		public DateTime? CreatedUtc { get; }
		public DateTime? ValidToUtc { get; }
		public bool? IsRecovery { get; set; }
		public string? Data { get; }

		public DbMessage()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(Message),
				PropertyNames = new List<string>
				{
					nameof(IdMessage),
					nameof(IdMessageType),
					nameof(IdPublisherInstance),
					nameof(IdPreviousMessage),
					nameof(CreatedUtc),
					nameof(ValidToUtc),
					nameof(IsRecovery),
					nameof(Data),
				}
			});
		}

		public async Task InsertAsync<TData>(NpgsqlConnection connection, NpgsqlTransaction? transaction, IMessage<TData> message, int idMessageType, CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			var sql = _table.ToInsertSql();

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(IdMessage), message.IdMessage },
					{ nameof(IdMessageType), idMessageType },
					{ nameof(IdPublisherInstance), message.IdPublisherInstance },
					{ nameof(IdPreviousMessage), message.IdPreviousMessage },
					{ nameof(CreatedUtc), message.CreatedUtc },
					{ nameof(ValidToUtc), message.ValidToUtc },
					{ nameof(IsRecovery), message.IsRecovery },
					{ nameof(Data), message.Data?.Serialize() },
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
