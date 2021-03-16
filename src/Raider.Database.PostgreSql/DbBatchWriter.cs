using Raider.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Database.PostgreSql
{
	public abstract class DbBatchWriter<T> : BatchWriter<T>, IDisposable
	{
		private readonly string? _connectionString;
		protected BulkInsert BulkInsert { get; }

		public DbBatchWriter(IDbBatchWriterOptions? options, Action<string, object?, object?, object?>? errorLogger = null)
			: base(options, errorLogger)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			options.Validate();

			_connectionString = options.ConnectionString;
			BulkInsert = new BulkInsert(options.ToDictionaryTableOptions(true, true));
		}

		public abstract IDictionary<string, object?>? ToDictionary(T obj);

		protected override async Task<ulong> WriteBatchAsync(IEnumerable<T> batch, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(_connectionString))
				throw new InvalidOperationException(nameof(_connectionString));

			var rows = batch?.Where(x => x != null).Select(obj => ToDictionary(obj)).Where(x => x != null).ToList();
			return await BulkInsert.WriteBatchAsync(rows, _connectionString, cancellationToken);
		}
	}
}
