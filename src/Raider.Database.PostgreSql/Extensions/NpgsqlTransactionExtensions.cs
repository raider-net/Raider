using Raider.Transactions;
using System;

namespace Npgsql
{
	public static class NpgsqlTransactionExtensions
	{
		public static ITransactionContext AttachToTransactionContext(this NpgsqlTransaction transaction, ITransactionContext transactionContext)
		{
			if (transaction == null)
				throw new ArgumentNullException(nameof(transaction));

			if (transactionContext == null)
				throw new ArgumentNullException(nameof(transactionContext));

			transactionContext.OnCommitted((transactionContext, cancellationToken) => transaction.CommitAsync(cancellationToken));
			transactionContext.OnCommitted((transactionContext) => transaction.Commit());
			transactionContext.OnRollback((transactionContext, cancellationToken) => transaction.RollbackAsync(cancellationToken));
			transactionContext.OnRollback((transactionContext) => transaction.Rollback());

#if NETSTANDARD2_0 || NETSTANDARD2_1
			transactionContext.OnDisposed(transactionContext => transaction.Dispose());
#elif NET5_0
			transactionContext.OnDisposed(transactionContext => transaction.DisposeAsync());
#endif

			transactionContext.AddUniqueItem(nameof(NpgsqlTransaction), transaction);
			transactionContext.AddUniqueItem(nameof(NpgsqlConnection), transaction.Connection);
			return transactionContext;
		}

		public static ITransactionContext ToTransactionContext(this NpgsqlTransaction transaction)
			=> ToTransactionContext(transaction, null);

		public static ITransactionContext ToTransactionContext(this NpgsqlTransaction transaction, Action<TransactionContextBuilder>? configure)
		{
			if (transaction == null)
				throw new ArgumentNullException(nameof(transaction));

			var builder = TransactionContextBuilder.Create();

			builder.OnCommitted((transactionContext, cancellationToken) => transaction.CommitAsync(cancellationToken));
			builder.OnCommitted((transactionContext) => transaction.Commit());
			builder.OnRollback((transactionContext, cancellationToken) => transaction.RollbackAsync(cancellationToken));
			builder.OnRollback((transactionContext) => transaction.Rollback());

#if NETSTANDARD2_0 || NETSTANDARD2_1
			builder.OnDisposed(transactionContext => transaction.Dispose());
#elif NET5_0
			builder.OnDisposed(transactionContext => transaction.DisposeAsync());
#endif

			configure?.Invoke(builder);

			var transactionContext = builder.Build();

			transactionContext.AddUniqueItem(nameof(NpgsqlTransaction), transaction);
			transactionContext.AddUniqueItem(nameof(NpgsqlConnection), transaction.Connection);
			return transactionContext;
		}
	}
}
