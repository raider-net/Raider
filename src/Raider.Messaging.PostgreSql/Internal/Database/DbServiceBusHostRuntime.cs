using Npgsql;
using Raider.Database.PostgreSql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbServiceBusHostRuntime
	{
		public const string ServiceBusHostRuntime = "ServiceBusHostRuntime";

		private readonly DictionaryTable _table;

		public Guid? IdServiceBusHostRuntime { get; }
		public Guid? IdServiceBusHost { get; }
		public DateTime? StartedUtc { get; }
		public DateTime? EndedUtc { get; }
		public int? IdUser { get; }

		public DbServiceBusHostRuntime()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(ServiceBusHostRuntime),
				PropertyNames = new List<string>
				{
					nameof(IdServiceBusHostRuntime),
					nameof(IdServiceBusHost),
					nameof(StartedUtc),
					nameof(EndedUtc),
					nameof(IdUser)
				}
			});
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
					{ nameof(IdServiceBusHostRuntime), serviceBusHost.ApplicationContext.TraceInfo.RuntimeUniqueKey },
					{ nameof(IdServiceBusHost), serviceBusHost.IdServiceBusHost },
					{ nameof(StartedUtc), serviceBusHost.StartedUtc },
					{ nameof(EndedUtc), null },
					{ nameof(IdUser), serviceBusHost.ApplicationContext.TraceInfo.IdUser },
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}

		public async Task UpdateAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IServiceBusHost serviceBusHost, DateTime endedUtc, CancellationToken cancellationToken = default)
		{
			if (serviceBusHost == null)
				throw new ArgumentNullException(nameof(serviceBusHost));

			var sql = _table.ToUpdateSql(new List<string> { nameof(EndedUtc) }, where: $"\"{nameof(IdServiceBusHostRuntime)}\"=@id");

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(EndedUtc),  endedUtc },
					{ "@id", serviceBusHost.ApplicationContext.TraceInfo.RuntimeUniqueKey }
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
