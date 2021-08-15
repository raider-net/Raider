using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
		public bool IsTransactionCommitted { get; private set; }

		public DbQueryHandlerContext(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
		}

		public TContext CreateNewDbContext<TContext>(
			IDbContextTransaction? dbContextTransaction = null,
			bool isTransactionCommitted = false,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			string? connectionString = null)
			where TContext : DbContext
		{
			var oldTransaction = dbContextTransaction ?? DbContextTransaction;
			var dbContext = DbContextFactory.CreateNewDbContext<TContext>(
				ServiceProvider,
				oldTransaction,
				out IDbContextTransaction? newDbContextTransaction,
				transactionUsage,
				transactionIsolationLevel,
				() => IsTransactionCommitted,
				connectionString);

			if (oldTransaction != null)
			{
				if (oldTransaction != newDbContextTransaction)
				{
					IsTransactionCommitted = true;
				}
				else if (DbContextTransaction == newDbContextTransaction)
				{
					//IsTransactionCommitted == IsTransactionCommitted
				}
				else if (dbContextTransaction == newDbContextTransaction)
				{
					IsTransactionCommitted = isTransactionCommitted;
				}
			}

			DbContextTransaction = newDbContextTransaction;
			return dbContext;
		}

		public TContext GetOrCreateDbContext<TContext>(
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			string? connectionString = null)
			where TContext : DbContext
		{
			var result = _dbContextCache.GetOrAdd(typeof(TContext), (dbContextType) => CreateNewDbContext<TContext>(
				null,
				false,
				transactionUsage,
				transactionIsolationLevel,
				connectionString)).CheckDbTransaction(transactionUsage);
			return (TContext)result;
		}

		public override bool HasTransaction()
			=> DbContextTransaction != null;

		protected override void CommitSafe()
		{
			if (DbContextTransaction == null || IsTransactionCommitted)
				return;

			DbContextTransaction.Commit();
			IsTransactionCommitted = true;
		}

		protected override void RollbackSafe()
		{
			if (DbContextTransaction == null || IsTransactionCommitted)
				return;

			DbContextTransaction.Rollback();
			IsTransactionCommitted = true; //naozaj? po commitnuti transakcie sa uz neda pozuit transakcia na dalsi commit alebo rollback?
		}

		public override void Commit()
		{
			DbContextTransaction?.Commit();
			IsTransactionCommitted = true;
		}

		public override void Rollback()
		{
			DbContextTransaction?.Rollback();
			IsTransactionCommitted = true;
		}

		public override void DisposeTransaction()
			=> DbContextTransaction?.Dispose();

		protected override async Task CommitSafeAsync(CancellationToken cancellationToken = default)
		{
			if (DbContextTransaction == null || IsTransactionCommitted)
				return;

			await DbContextTransaction.CommitAsync(cancellationToken);
			IsTransactionCommitted = true;
		}

		protected override async Task RollbackSafeAsync(CancellationToken cancellationToken = default)
		{
			if (DbContextTransaction == null || IsTransactionCommitted)
				return;

			await DbContextTransaction.RollbackAsync(cancellationToken);
			IsTransactionCommitted = true;
		}

		public override async Task CommitAsync(CancellationToken cancellationToken = default)
		{
			if (DbContextTransaction != null)
			{
				await DbContextTransaction.CommitAsync(cancellationToken);
				IsTransactionCommitted = true;
			}
		}

		public override async Task RollbackAsync(CancellationToken cancellationToken = default)
		{
			if (DbContextTransaction != null)
			{
				await DbContextTransaction.RollbackAsync(cancellationToken);
				IsTransactionCommitted = true;
			}
		}

		public override ValueTask DisposeTransactionAsync()
		{
			if (DbContextTransaction != null)
				return DbContextTransaction.DisposeAsync();

			return ValueTask.CompletedTask;
		}
	}
}
