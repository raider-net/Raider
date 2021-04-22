using Npgsql;
using Raider.Database.PostgreSql;
using Raider.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbPublisher
	{
		public const string Publisher = "Publisher";

		private readonly DictionaryTable _table;

		public Guid? IdPublisher { get; }
		public string? Name { get; }
		public string? Description { get; }
		public int? IdScenario { get; }
		public DateTime? CreatedUtc { get; }
		public Guid? IdCreatedByServiceBusHostRuntime { get; set; }
		public int? IdPublishingMessageType { get; set; }

		public DbPublisher()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(Publisher),
				PropertyNames = new List<string>
				{
					nameof(IdPublisher),
					nameof(Name),
					nameof(Description),
					nameof(IdScenario),
					nameof(CreatedUtc),
					nameof(IdCreatedByServiceBusHostRuntime),
					nameof(IdPublishingMessageType)
				}
			});
		}

		public async Task<bool> ExistsAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IPublisher publisher, int idPublishingMessageType, CancellationToken cancellationToken = default)
		{
			if (publisher == null)
				throw new ArgumentNullException(nameof(publisher));

			var sql = $"SELECT \"{nameof(Name)}\", \"{nameof(IdScenario)}\", \"{nameof(IdPublishingMessageType)}\" FROM {nameof(Defaults.Schema.bus)}.\"{nameof(Publisher)}\" WHERE \"{nameof(IdPublisher)}\" = @p1";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			cmd.Parameters.AddWithValue("@p1", publisher.IdComponent);

			var count = 0;
			using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
			{
				while (await reader.ReadAsync(cancellationToken))
				{
					count++;
					if (1 < count)
						throw new InvalidOperationException($"{nameof(ExistsAsync)}: More than one {nameof(Publisher)} exists for {nameof(IdPublisher)} = {publisher.IdComponent}");

					var dbName = reader.GetValueOrDefault<string>(0);
					if (dbName != publisher.Name)
						throw new InvalidOperationException($"{nameof(ExistsAsync)}: Another {nameof(Publisher)} exists for {nameof(IdPublisher)} = {publisher.IdComponent} && {nameof(Name)} = {publisher.Name}");

					var dbIdScenario = reader.GetValueOrDefault<int?>(1);
					if (dbIdScenario != publisher.IdScenario)
						throw new InvalidOperationException($"{nameof(ExistsAsync)}: Another {nameof(Publisher)} exists for {nameof(IdPublisher)} = {publisher.IdComponent} && {nameof(IdScenario)} = {publisher.IdScenario}");

					var dbIdPublishingMessageType = reader.GetValueOrDefault<int?>(2);
					if (dbIdPublishingMessageType != idPublishingMessageType)
						throw new InvalidOperationException($"{nameof(ExistsAsync)}: Another {nameof(Publisher)} exists for {nameof(IdPublisher)} = {publisher.IdComponent} && {nameof(dbIdPublishingMessageType)} = {idPublishingMessageType}");
				}
			}

			return 0 < count;
		}

		public async Task InsertAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IServiceBusHost serviceBusHost, IPublisher publisher, int idPublishingMessageType, CancellationToken cancellationToken = default)
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
					{ nameof(IdPublisher), publisher.IdComponent },
					{ nameof(Name), publisher.Name },
					{ nameof(Description), publisher.Description },
					{ nameof(IdScenario), publisher.IdScenario },
					{ nameof(CreatedUtc), publisher.LastActivityUtc },
					{ nameof(IdCreatedByServiceBusHostRuntime), serviceBusHost.ApplicationContext.TraceInfo.RuntimeUniqueKey },
					{ nameof(IdPublishingMessageType), idPublishingMessageType },
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
