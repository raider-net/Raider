using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Raider.EntityFrameworkCore;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.QueryServices.EntityFramework
{
	public interface IDbQueryServiceContext : Raider.QueryServices.IQueryServiceContext, Raider.EntityFrameworkCore.IDbContextTransactionManager
	{
		TContext CreateNewDbContext<TContext>(
			IDbContextTransaction? dbContextTransaction = null,
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

		void Commit();

		void Rollback();

		Task CommitAsync(CancellationToken cancellationToken = default);

		Task RollbackAsync(CancellationToken cancellationToken = default);
	}
}
