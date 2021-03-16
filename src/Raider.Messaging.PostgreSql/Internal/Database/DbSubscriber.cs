using Npgsql;
using Raider.Database.PostgreSql;
using Raider.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbSubscriber
	{
		public const string Subscriber = "Subscriber";

		private readonly DictionaryTable _table;

		public Guid? IdSubscriber { get; }
		public string? Name { get; }
		public string? Description { get; }
		public int? IdScenario { get; }
		public DateTime? CreatedUtc { get; }
		public Guid? IdCreatedByServiceBusHostRuntime { get; set; }
		public int? IdSubscribingMessageType { get; set; }
		public bool? ReadMessagesFromSequentialFIFO { get; set; }
		public int? TimeoutForMessageProcessingInSeconds { get; set; }
		public int? MaxMessageProcessingRetryCount { get; set; }

		public DbSubscriber()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(Subscriber),
				PropertyNames = new List<string>
				{
					nameof(IdSubscriber),
					nameof(Name),
					nameof(Description),
					nameof(IdScenario),
					nameof(CreatedUtc),
					nameof(IdCreatedByServiceBusHostRuntime),
					nameof(IdSubscribingMessageType),
					nameof(ReadMessagesFromSequentialFIFO),
					nameof(TimeoutForMessageProcessingInSeconds),
					nameof(MaxMessageProcessingRetryCount)
				}
			});
		}

		public async Task<bool> ExistsAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, ISubscriber subscriber, int idSubscribingMessageType, CancellationToken cancellationToken = default)
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			var sql = $"SELECT \"{nameof(Name)}\", \"{nameof(IdScenario)}\", \"{nameof(IdSubscribingMessageType)}\" FROM {nameof(Defaults.Schema.bus)}.\"{nameof(Subscriber)}\" WHERE \"{nameof(IdSubscriber)}\" = @p1";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			cmd.Parameters.AddWithValue("@p1", subscriber.IdComponent);

			var count = 0;
			using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
			{
				while (await reader.ReadAsync(cancellationToken))
				{
					count++;
					if (1 < count)
						throw new InvalidOperationException($"{nameof(ExistsAsync)}: More than one {nameof(Subscriber)} exists for {nameof(IdSubscriber)} = {subscriber.IdComponent}");

					var dbName = reader.GetValueOrDefault<string>(0);
					if (dbName != subscriber.Name)
						throw new InvalidOperationException($"{nameof(ExistsAsync)}: Another {nameof(Subscriber)} exists for {nameof(IdSubscriber)} = {subscriber.IdComponent} && {nameof(Name)} = {subscriber.Name}");

					var dbIdScenario = reader.GetValueOrDefault<int?>(1);
					if (dbIdScenario != subscriber.IdScenario)
						throw new InvalidOperationException($"{nameof(ExistsAsync)}: Another {nameof(Subscriber)} exists for {nameof(IdSubscriber)} = {subscriber.IdComponent} && {nameof(IdScenario)} = {subscriber.IdScenario}");

					var dbIdSubscribingMessageType = reader.GetValueOrDefault<int?>(2);
					if (dbIdSubscribingMessageType != idSubscribingMessageType)
						throw new InvalidOperationException($"{nameof(ExistsAsync)}: Another {nameof(Subscriber)} exists for {nameof(IdSubscriber)} = {subscriber.IdComponent} && {nameof(dbIdSubscribingMessageType)} = {idSubscribingMessageType}");
				}
			}

			return 0 < count;
		}

		public async Task InsertAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IServiceBusHost serviceBusHost, ISubscriber subscriber, int idSubscribingMessageType, CancellationToken cancellationToken = default)
		{
			if (serviceBusHost == null)
				throw new ArgumentNullException(nameof(serviceBusHost));
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			var sql = _table.ToInsertSql();

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(IdSubscriber), subscriber.IdComponent },
					{ nameof(Name), subscriber.Name },
					{ nameof(Description), subscriber.Description },
					{ nameof(IdScenario), subscriber.IdScenario },
					{ nameof(CreatedUtc), subscriber.LastActivityUtc },
					{ nameof(IdCreatedByServiceBusHostRuntime), serviceBusHost.IdServiceBusHostRuntime },
					{ nameof(IdSubscribingMessageType), idSubscribingMessageType },
					{ nameof(ReadMessagesFromSequentialFIFO), subscriber.ReadMessagesFromSequentialFIFO },
					{ nameof(TimeoutForMessageProcessingInSeconds), subscriber.TimeoutForMessageProcessing.TotalSeconds },
					{ nameof(MaxMessageProcessingRetryCount), subscriber.MaxMessageProcessingRetryCount },
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
