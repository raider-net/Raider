using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Raider.EntityFrameworkCore;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.QueryServices.EntityFramework
{
	public interface IDbQueryServiceContext : Raider.QueryServices.IQueryServiceContext, Raider.EntityFrameworkCore.IDbContextTransactionManager
	{
		TContext CreateNewDbContextWithNewTransaction<TContext>(
			out IDbContextTransaction? newDbContextTransaction,
			IsolationLevel? transactionIsolationLevel = null,
			Func<bool>? isTransactionCommittedDelegate = null,
			string? connectionString = null)
			where TContext : DbContext;

		TContext CreateNewDbContextWithoutTransaction<TContext>(DbConnection? externalDbConnection = null, string? connectionString = null)
			where TContext : DbContext;
		TContext CreateNewDbContextWithExistingTransaction<TContext>(
			IDbContextTransaction dbContextTransaction,
			bool isTransactionCommitted = false,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			string? connectionString = null)
			where TContext : DbContext;

		TContext GetOrCreateDbContext<TContext>(
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			string? connectionString = null)
			where TContext : DbContext;

		void SetDbContext<TContext>(TContext dbContext)
			where TContext : DbContext;

		void ResetDbContext<TContext>(TContext dbContext)
			where TContext : DbContext;

		void Commit();

		void Rollback();

		Task CommitAsync(CancellationToken cancellationToken = default);

		Task RollbackAsync(CancellationToken cancellationToken = default);
	}
}
