using Npgsql;
using Raider.Database.PostgreSql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbSubscriberInstance
	{
		public const string SubscriberInstance = "SubscriberInstance";

		private readonly DictionaryTable _table;

		public Guid? IdSubscriberInstance { get; }
		public Guid? IdServiceBusHostRuntime { get; set; }
		public int? IdSubscriber { get; }
		public DateTime? LastActivityUtc { get; }
		public int? IdComponentState { get; }
		public bool ReadMessagesFromSequentialFIFO { get; }
		public TimeSpan TimeoutForMessageProcessingInSeconds { get; }
		public int MaxMessageProcessingRetryCount { get; }

		public DbSubscriberInstance()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(SubscriberInstance),
				PropertyNames = new List<string>
				{
					nameof(IdSubscriberInstance),
					nameof(IdServiceBusHostRuntime),
					nameof(IdSubscriber),
					nameof(LastActivityUtc),
					nameof(ReadMessagesFromSequentialFIFO),
					nameof(TimeoutForMessageProcessingInSeconds),
					nameof(MaxMessageProcessingRetryCount),
					nameof(IdComponentState)
				}
			});
		}

		public async Task InsertAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IServiceBusHost serviceBusHost, ISubscriber subscriber, CancellationToken cancellationToken = default)
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
					{ nameof(IdSubscriberInstance), subscriber.IdInstance },
					{ nameof(IdServiceBusHostRuntime), serviceBusHost.ApplicationContext.TraceInfo.RuntimeUniqueKey },
					{ nameof(IdSubscriber), subscriber.IdComponent },
					{ nameof(LastActivityUtc), subscriber.LastActivityUtc },
					{ nameof(ReadMessagesFromSequentialFIFO), subscriber.ReadMessagesFromSequentialFIFO },
					{ nameof(TimeoutForMessageProcessingInSeconds), subscriber.TimeoutForMessageProcessing.TotalSeconds },
					{ nameof(MaxMessageProcessingRetryCount), subscriber.MaxMessageProcessingRetryCount },
					{ nameof(IdComponentState), (int)subscriber.State }
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}

		public async Task UpdateAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, ISubscriber subscriber, CancellationToken cancellationToken = default)
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			var sql = _table.ToUpdateSql(new List<string>
				{
					nameof(LastActivityUtc),
					nameof(IdComponentState) 
				},
				where: $"\"{nameof(IdSubscriberInstance)}\"=@id");

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(LastActivityUtc), subscriber.LastActivityUtc },
					{ nameof(IdComponentState), (int)subscriber.State },
					{ "@id", subscriber.IdInstance }
				});

			var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

			if (result != 1)
				throw new InvalidOperationException($"{nameof(UpdateAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result}");
		}
	}
}
