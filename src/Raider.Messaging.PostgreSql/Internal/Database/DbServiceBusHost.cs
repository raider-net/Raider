using Npgsql;
using Raider.Database.PostgreSql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbServiceBusHost
	{
		public const string ServiceBusHost = "ServiceBusHost";

		private readonly DictionaryTable _table;

		public Guid? IdServiceBusHost { get; }
		public string? Name { get; }
		public string? Description { get; }
		public DateTime? CreatedUtc { get; }

		public DbServiceBusHost()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(ServiceBusHost),
				PropertyNames = new List<string>
				{
					nameof(IdServiceBusHost),
					nameof(Name),
					nameof(Description),
					nameof(CreatedUtc)
				}
			});
		}

		public async Task<bool> ExistsAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IServiceBusHost serviceBusHost, CancellationToken cancellationToken = default)
		{
			if (serviceBusHost == null)
				throw new ArgumentNullException(nameof(serviceBusHost));

			var sql = $"SELECT EXISTS (SELECT 1 FROM {nameof(Defaults.Schema.bus)}.\"{nameof(ServiceBusHost)}\" WHERE \"{nameof(IdServiceBusHost)}\" = @p1 AND \"{nameof(Name)}\" = @p2)";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			cmd.Parameters.AddWithValue("@p1", serviceBusHost.IdServiceBusHost);
			cmd.Parameters.AddWithValue("@p2", serviceBusHost.Name);

			var result = await cmd.ExecuteScalarAsync(cancellationToken);
			if (result is bool exists)
				return exists;

			throw new InvalidOperationException($"{nameof(ExistsAsync)}: Invalid {nameof(cmd.ExecuteScalarAsync)}");
		}

		public async Task InsertAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IServiceBusHost serviceBusHost, CancellationToken cancellationToken = default)
		{
			if (serviceBusHost == null)
				throw new ArgumentNullException(nameof(serviceBusHost));

			var sql = _table.ToInsertSql();

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(IdServiceBusHost), serviceBusHost.IdServiceBusHost },
					{ nameof(Name), serviceBusHost.Name },
					{ nameof(Description), serviceBusHost.Description },
					{ nameof(CreatedUtc), serviceBusHost.StartedUtc },
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
