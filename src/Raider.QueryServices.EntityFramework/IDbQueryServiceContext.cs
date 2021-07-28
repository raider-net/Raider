using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Raider.EntityFrameworkCore;
using System.Data;

namespace Raider.QueryServices.EntityFramework
{
	public interface IDbQueryServiceContext : Raider.QueryServices.IQueryServiceContext
	{
		IDbContextTransaction? DbContextTransaction { get;  }

		TContext CreateNewDbContext<TContext>(
			IDbContextTransaction? dbContextTransaction = null,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext;

		TContext GetOrCreateDbContext<TContext>(TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew, IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext;
	}
}
