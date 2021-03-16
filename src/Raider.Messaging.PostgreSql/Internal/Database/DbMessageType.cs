using Npgsql;
using Raider.Database.PostgreSql;
using Raider.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql.Database
{
	internal class DbMessageType
	{
		public const string MessageType = "MessageType";

		private readonly DictionaryTable _table;

		public int? IdMessageType { get; }
		public string? Type { get; }
		public string? FullName { get; }
		public DateTime? CreatedUtc { get; }
		public Guid? IdCreatedByServiceBusHostRuntime { get; set; }

		public DbMessageType()
		{
			_table = new DictionaryTable(new DictionaryTableOptions
			{
				SchemaName = nameof(Defaults.Schema.bus),
				TableName = nameof(MessageType),
				PropertyNames = new List<string>
				{
					//nameof(IdMessageType),
					nameof(Type),
					nameof(FullName),
					nameof(CreatedUtc),
					nameof(IdCreatedByServiceBusHostRuntime)
				}
			});
		}

		public async Task<int?> GetIdMessageTypeAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, Type type, CancellationToken cancellationToken = default)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			string typeName = type.Name;
			string fullName = type.ToFriendlyFullName();

			var sql = $"SELECT \"{nameof(IdMessageType)}\", \"{nameof(FullName)}\" FROM {nameof(Defaults.Schema.bus)}.\"{nameof(MessageType)}\" WHERE \"{nameof(Type)}\" = @p1";

			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			cmd.Parameters.AddWithValue("@p1", typeName);
			int? id = null;

			var count = 0;
			using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
			{
				while (await reader.ReadAsync(cancellationToken))
				{
					count++;
					if (1 < count)
						throw new InvalidOperationException($"{nameof(GetIdMessageTypeAsync)}: More than one {nameof(MessageType)} exists for {nameof(Type)} = {typeName}");

					id = reader.GetValueOrDefault<int?>(0);
					var dbFullName = reader.GetValueOrDefault<string>(1);
					if (dbFullName != fullName)
						throw new InvalidOperationException($"{nameof(GetIdMessageTypeAsync)}: Another {nameof(MessageType)} exists for {nameof(Type)} = {typeName} && {nameof(FullName)} = {fullName}");
				}
			}

			return id;
		}

		public async Task<int> InsertAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, IServiceBusHost serviceBusHost, Type type, CancellationToken cancellationToken = default)
		{
			if (serviceBusHost == null)
				throw new ArgumentNullException(nameof(serviceBusHost));
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			var sql = _table.ToInsertSql($"\"{nameof(IdMessageType)}\"");
			
			using var cmd = new NpgsqlCommand(sql, connection);
			if (transaction != null)
				cmd.Transaction = transaction;

			_table.SetParameters(cmd, new Dictionary<string, object?>
				{
					{ nameof(Type), type.Name },
					{ nameof(FullName), type.ToFriendlyFullName() },
					{ nameof(CreatedUtc), serviceBusHost.StartedUtc },
					{ nameof(IdCreatedByServiceBusHostRuntime), serviceBusHost.IdServiceBusHostRuntime },
				});

			var result = await cmd.ExecuteScalarAsync(cancellationToken);

			if (result == null)
				throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteScalarAsync)} returns null");

			if (result is int id)
				return id;

			throw new InvalidOperationException($"{nameof(InsertAsync)}: {nameof(cmd.ExecuteScalarAsync)} returns invalid int = {result}");
		}
	}
}
