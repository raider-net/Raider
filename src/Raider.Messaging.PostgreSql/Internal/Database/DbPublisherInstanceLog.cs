using Npgsql;
using Raider.Database.PostgreSql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbPublisherInstanceLog
	{
		public const string PublisherInstanceLog = "PublisherInstanceLog";

		private readonly DictionaryTable _table;

		public Guid? IdPublisherInstance { get; }
		public int? IdLogLevel { get; set; }
		public DateTime? CreatedUtc { get; set; }
		public string? LogMessageType { get; set; }
		public string? Message { get; set; }
		public string? Detail { get; set; }

		public DbPublisherInstanceLog()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(PublisherInstanceLog),
				PropertyNames = new List<string>
				{
					nameof(IdPublisherInstance),
					nameof(IdLogLevel),
					nameof(CreatedUtc),
					nameof(LogMessageType),
					nameof(Message),
					nameof(Detail),
				}
			});
		}

		public async Task InsertAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IPublisher publisher, LogBase log, CancellationToken cancellationToken = default)
		{
			if (publisher == null)
				throw new ArgumentNullException(nameof(publisher));
			if (log == null)
				throw new ArgumentNullException(nameof(log));

			var sql = _table.ToInsertSql();

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(IdPublisherInstance), publisher.IdInstance },
					{ nameof(IdLogLevel), log.IdLogLevel },
					{ nameof(CreatedUtc), log.CreatedUtc },
					{ nameof(LogMessageType), log.LogMessageType  },
					{ nameof(Message), log.Message  },
					{ nameof(Detail), log.Detail  },
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
