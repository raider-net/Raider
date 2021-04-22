using Npgsql;
using Raider.Database.PostgreSql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbPublisherInstance
	{
		public const string PublisherInstance = "PublisherInstance";

		private readonly DictionaryTable _table;

		public Guid? IdPublisherInstance { get; }
		public Guid? IdServiceBusHostRuntime { get; set; }
		public int? IdPublisher { get; }
		public DateTime? LastActivityUtc { get; }
		public int? IdComponentState { get; }

		public DbPublisherInstance()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(PublisherInstance),
				PropertyNames = new List<string>
				{
					nameof(IdPublisherInstance),
					nameof(IdServiceBusHostRuntime),
					nameof(IdPublisher),
					nameof(LastActivityUtc),
					nameof(IdComponentState)
				}
			});
		}

		public async Task InsertAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IServiceBusHost serviceBusHost, IPublisher publisher, CancellationToken cancellationToken = default)
		{
			if (serviceBusHost == null)
				throw new ArgumentNullException(nameof(serviceBusHost));
			if (publisher == null)
				throw new ArgumentNullException(nameof(publisher));

			var sql = _table.ToInsertSql();

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(IdPublisherInstance), publisher.IdInstance },
					{ nameof(IdServiceBusHostRuntime), serviceBusHost.ApplicationContext.TraceInfo.RuntimeUniqueKey },
					{ nameof(IdPublisher), publisher.IdComponent },
					{ nameof(LastActivityUtc), publisher.LastActivityUtc },
					{ nameof(IdComponentState), (int)publisher.State }
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}

		public async Task UpdateAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IPublisher publisher, CancellationToken cancellationToken = default)
		{
			if (publisher == null)
				throw new ArgumentNullException(nameof(publisher));

			var sql = _table.ToUpdateSql(new List<string> { nameof(LastActivityUtc), nameof(IdComponentState) }, where: $"\"{nameof(IdPublisherInstance)}\"=@id");

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(LastActivityUtc), publisher.LastActivityUtc },
					{ nameof(IdComponentState), (int)publisher.State },
					{ "@id", publisher.IdInstance }
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
