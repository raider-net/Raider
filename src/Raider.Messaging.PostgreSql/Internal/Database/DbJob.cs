using Npgsql;
using Raider.Database.PostgreSql;
using Raider.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbJob
	{
		public const string Job = "Job";

		private readonly DictionaryTable _table;

		public Guid? IdJob { get; }
		public string? Name { get; }
		public string? Description { get; }
		public int? IdScenario { get; }
		public DateTime? CreatedUtc { get; }
		public Guid? IdCreatedByServiceBusHostRuntime { get; set; }

		public DbJob()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(Job),
				PropertyNames = new List<string>
				{
					nameof(IdJob),
					nameof(Name),
					nameof(Description),
					nameof(IdScenario),
					nameof(CreatedUtc),
					nameof(IdCreatedByServiceBusHostRuntime)
				}
			});
		}

		public async Task<bool> ExistsAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IJob job, CancellationToken cancellationToken = default)
		{
			if (job == null)
				throw new ArgumentNullException(nameof(job));

			var sql = $"SELECT \"{nameof(Name)}\", \"{nameof(IdScenario)}\" FROM {nameof(Defaults.Schema.bus)}.\"{nameof(Job)}\" WHERE \"{nameof(IdJob)}\" = @p1";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			cmd.Parameters.AddWithValue("@p1", job.IdComponent);

			var count = 0;
			using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
			{
				while (await reader.ReadAsync(cancellationToken))
				{
					count++;
					if (1 < count)
						throw new InvalidOperationException($"{nameof(ExistsAsync)}: More than one {nameof(Job)} exists for {nameof(IdJob)} = {job.IdComponent}");

					var dbName = reader.GetValueOrDefault<string>(0);
					if (dbName != job.Name)
						throw new InvalidOperationException($"{nameof(ExistsAsync)}: Another {nameof(Job)} exists for {nameof(IdJob)} = {job.IdComponent} && {nameof(Name)} = {job.Name}");

					var dbIdScenario = reader.GetValueOrDefault<int?>(1);
					if (dbIdScenario != job.IdScenario)
						throw new InvalidOperationException($"{nameof(ExistsAsync)}: Another {nameof(Job)} exists for {nameof(IdJob)} = {job.IdComponent} && {nameof(IdScenario)} = {job.IdScenario}");
				}
			}

			return 0 < count;
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
					{ nameof(IdJob), job.IdComponent },
					{ nameof(Name), job.Name },
					{ nameof(Description), job.Description },
					{ nameof(IdScenario), job.IdScenario },
					{ nameof(CreatedUtc), job.LastActivityUtc },
					{ nameof(IdCreatedByServiceBusHostRuntime), serviceBusHost.IdServiceBusHostRuntime },
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
