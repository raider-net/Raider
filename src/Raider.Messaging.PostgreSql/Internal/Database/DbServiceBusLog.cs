using Npgsql;
using Raider.Database.PostgreSql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbServiceBusLog
	{
		public const string ServiceBusLog = "ServiceBusLog";

		private readonly DictionaryTable _table;

		public Guid? IdServiceBusHostRuntime { get; }
		public int? IdLogLevel { get; set; }
		public DateTime? CreatedUtc { get; set; }
		public string? LogMessageType { get; set; }
		public string? Message { get; set; }
		public string? Detail { get; set; }

		public DbServiceBusLog()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(ServiceBusLog),
				PropertyNames = new List<string>
				{
					nameof(IdServiceBusHostRuntime),
					nameof(IdLogLevel),
					nameof(CreatedUtc),
					nameof(LogMessageType),
					nameof(Message),
					nameof(Detail),
				}
			});
		}

		public async Task InsertAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IServiceBusHost serviceBusHost, LogBase log, CancellationToken cancellationToken = default)
		{
			if (serviceBusHost == null)
				throw new ArgumentNullException(nameof(serviceBusHost));
			if (log == null)
				throw new ArgumentNullException(nameof(log));

			var sql = _table.ToInsertSql();

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(IdServiceBusHostRuntime), serviceBusHost.IdServiceBusHostRuntime },
					{ nameof(IdLogLevel), log.IdLogLevel },
					{ nameof(CreatedUtc), log.CreatedUtc },
					{ nameof(LogMessageType), log.LogMessageType  },
					{ nameof(Message), log.Message  },
					{ nameof(Detail), log.Detail  }
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
