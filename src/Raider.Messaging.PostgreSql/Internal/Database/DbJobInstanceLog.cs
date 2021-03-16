using Npgsql;
using Raider.Database.PostgreSql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbJobInstanceLog
	{
		public const string JobInstanceLog = "JobInstanceLog";

		private readonly DictionaryTable _table;

		public Guid? IdJobInstance { get; }
		public int? IdLogLevel { get; set; }
		public DateTime? CreatedUtc { get; set; }
		public string? LogMessageType { get; set; }
		public string? Message { get; set; }
		public string? Detail { get; set; }
		public Guid? IdSnapshot { get; }

		public DbJobInstanceLog()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(JobInstanceLog),
				PropertyNames = new List<string>
				{
					nameof(IdJobInstance),
					nameof(IdLogLevel),
					nameof(CreatedUtc),
					nameof(LogMessageType),
					nameof(Message),
					nameof(Detail),
					nameof(IdSnapshot),
				}
			});
		}

		public async Task InsertAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IJob job, LogBase log, CancellationToken cancellationToken = default)
		{
			if (job == null)
				throw new ArgumentNullException(nameof(job));
			if (log == null)
				throw new ArgumentNullException(nameof(log));

			var sql = _table.ToInsertSql();

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(IdJobInstance), job.IdInstance },
					{ nameof(IdLogLevel), log.IdLogLevel },
					{ nameof(CreatedUtc), log.CreatedUtc },
					{ nameof(LogMessageType), log.LogMessageType  },
					{ nameof(Message), log.Message  },
					{ nameof(Detail), log.Detail  },
					{ nameof(IdSnapshot), log.IdSnapshot  }
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
