using Npgsql;
using Raider.Database.PostgreSql;
using Raider.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbScenario
	{
		public const string Scenario = "Scenario";

		private readonly DictionaryTable _table;

		public Guid? IdScenario { get; }
		public string? Name { get; }
		public string? Description { get; }
		public DateTime? CreatedUtc { get; }
		public Guid? IdCreatedByServiceBusHostRuntime { get; set; }

		public DbScenario()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(Scenario),
				PropertyNames = new List<string>
				{
					nameof(IdScenario),
					nameof(Name),
					nameof(Description),
					nameof(CreatedUtc),
					nameof(IdCreatedByServiceBusHostRuntime)
				}
			});
		}

		public async Task<bool> ExistsAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IScenario scenario, CancellationToken cancellationToken = default)
		{
			if (scenario == null)
				throw new ArgumentNullException(nameof(scenario));

			var sql = $"SELECT \"{nameof(Name)}\" FROM {nameof(Defaults.Schema.bus)}.\"{nameof(Scenario)}\" WHERE \"{nameof(IdScenario)}\" = @p1";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			cmd.Parameters.AddWithValue("@p1", scenario.IdScenario);

			var count = 0;
			using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
			{
				while (await reader.ReadAsync(cancellationToken))
				{
					count++;
					if (1 < count)
						throw new InvalidOperationException($"{nameof(ExistsAsync)}: More than one {nameof(Scenario)} exists for {nameof(IdScenario)} = {scenario.IdScenario}");

					var dbName = reader.GetValueOrDefault<string>(0);
					if (dbName != scenario.Name)
						throw new InvalidOperationException($"{nameof(ExistsAsync)}: Another {nameof(Scenario)} exists for {nameof(IdScenario)} = {scenario.IdScenario} && {nameof(Name)} = {scenario.Name}");
				}
			}

			return 0 < count;
		}

		public async Task InsertAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IServiceBusHost serviceBusHost, IScenario scenario, DateTime createdUtc, CancellationToken cancellationToken = default)
		{
			if (serviceBusHost == null)
				throw new ArgumentNullException(nameof(serviceBusHost));
			if (scenario == null)
				throw new ArgumentNullException(nameof(scenario));

			var sql = _table.ToInsertSql();

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(IdScenario), scenario.IdScenario },
					{ nameof(Name), scenario.Name },
					{ nameof(Description), scenario.Description },
					{ nameof(CreatedUtc), createdUtc },
					{ nameof(IdCreatedByServiceBusHostRuntime), serviceBusHost.ApplicationContext.TraceInfo.RuntimeUniqueKey },
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
