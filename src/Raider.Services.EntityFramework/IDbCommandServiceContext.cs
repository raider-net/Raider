using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Raider.EntityFrameworkCore;
using System.Data;

namespace Raider.Services.EntityFramework
{
	public interface IDbCommandServiceContext : Raider.Services.ICommandServiceContext
	{
		IDbContextTransaction? DbContextTransaction { get; }

		TContext CreateNewDbContext<TContext>(
			IDbContextTransaction? dbContextTransaction = null,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			string? connectionString = null)
			where TContext : DbContext;

		TContext GetOrCreateDbContext<TContext>(
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			string? connectionString = null)
			where TContext : DbContext;
	}
}
