using Npgsql;
using Raider.Database.PostgreSql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbJobInstance
	{
		public const string JobInstance = "JobInstance";

		private readonly DictionaryTable _table;

		public Guid? IdJobInstance { get; }
		public Guid? IdServiceBusHostRuntime { get; set; }
		public int? IdJob { get; }
		public DateTime? LastActivityUtc { get; }
		public int? IdComponentState { get; }

		public DbJobInstance()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(JobInstance),
				PropertyNames = new List<string>
				{
					nameof(IdJobInstance),
					nameof(IdServiceBusHostRuntime),
					nameof(IdJob),
					nameof(LastActivityUtc),
					nameof(IdComponentState)
				}
			});
		}

		public async Task InsertAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IServiceBusHost serviceBusHost, IJob job, CancellationToken cancellationToken = default)
		{
			if (serviceBusHost == null)
				throw new ArgumentNullException(nameof(serviceBusHost));
			if (job == null)
				throw new ArgumentNullException(nameof(job));

			var sql = _table.ToInsertSql();

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(IdJobInstance), job.IdInstance },
					{ nameof(IdServiceBusHostRuntime), serviceBusHost.ApplicationContext.TraceInfo.RuntimeUniqueKey },
					{ nameof(IdJob), job.IdComponent },
					{ nameof(LastActivityUtc), job.LastActivityUtc },
					{ nameof(IdComponentState), (int)job.State }
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}

		public async Task UpdateAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IJob job, CancellationToken cancellationToken = default)
		{
			if (job == null)
				throw new ArgumentNullException(nameof(job));

			var sql = _table.ToUpdateSql(new List<string> { nameof(LastActivityUtc), nameof(IdComponentState) }, where: $"\"{nameof(IdJobInstance)}\"=@id");

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(LastActivityUtc), job.LastActivityUtc },
					{ nameof(IdComponentState), (int)job.State },
					{ "@id", job.IdInstance }
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
