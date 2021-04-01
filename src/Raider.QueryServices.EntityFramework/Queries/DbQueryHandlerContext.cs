using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Raider.DependencyInjection;
using Raider.EntityFrameworkCore;
using Raider.Queries;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.QueryServices.EntityFramework.Queries
{
	public abstract class DbQueryHandlerContext : Raider.QueryServices.Queries.QueryHandlerContext, IQueryHandlerContext, IDbQueryServiceContext
	{
		private readonly ConcurrentDictionary<Type, DbContext> _dbContextCache = new ConcurrentDictionary<Type, DbContext>();

		public IDbContextTransaction? DbContextTransaction { get; private set; }

		public DbQueryHandlerContext(ServiceFactory serviceFactory)
			: base(serviceFactory)
		{
		}

		public TContext CreateNewDbContext<TContext>(TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew, IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext
		{
			var dbContext = DbContextFactory.CreateNewDbContext<TContext>(ServiceFactory, DbContextTransaction, out IDbContextTransaction? newDbContextTransaction, transactionUsage, transactionIsolationLevel);
			DbContextTransaction = newDbContextTransaction;
			return dbContext;
		}

		public TContext GetOrCreateDbContext<TContext>(TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew, IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext
		{
			var result = _dbContextCache.GetOrAdd(typeof(TContext), (dbContextType) => CreateNewDbContext<TContext>(transactionUsage, transactionIsolationLevel)).CheckDbTransaction(transactionUsage);
			return (TContext)result;
		}

		public override bool HasTransaction()
			=> DbContextTransaction != null;

		public override void Commit()
			=> DbContextTransaction?.Commit();

		public override void Rollback()
			=> DbContextTransaction?.Rollback();

		public override void DisposeTransaction()
			=> DbContextTransaction?.Dispose();

		public override Task CommitAsync(CancellationToken cancellationToken = default)
		{
			if (DbContextTransaction != null)
				return DbContextTransaction.CommitAsync(cancellationToken);

			return Task.CompletedTask;
		}

		public override Task RollbackAsync(CancellationToken cancellationToken = default)
		{
			if (DbContextTransaction != null)
				return DbContextTransaction.RollbackAsync(cancellationToken);

			return Task.CompletedTask;
		}

		public override ValueTask DisposeTransactionAsync()
		{
			if (DbContextTransaction != null)
				return DbContextTransaction.DisposeAsync();

			return ValueTask.CompletedTask;
		}
	}
}
