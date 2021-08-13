using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Raider.EntityFrameworkCore;
using Raider.QueryServices.EntityFramework.Queries;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.QueryServices.EntityFramework
{
	internal static class DbContextExtensions
	{
		public static DbContext CheckDbTransaction(this DbContext dbContext, TransactionUsage transactionUsage)
		{
			if (transactionUsage == TransactionUsage.NONE && dbContext.Database.CurrentTransaction != null)
				throw new InvalidOperationException($"DbContext has transaction, but expected {nameof(TransactionUsage)} is {transactionUsage}");

			if (transactionUsage == TransactionUsage.ReuseOrCreateNew && dbContext.Database.CurrentTransaction == null)
				throw new InvalidOperationException($"DbContext has no transaction, but expected {nameof(TransactionUsage)} is {transactionUsage}");

			return dbContext;
		}
	}

	public class DbQueryServiceContext : Raider.QueryServices.QueryServiceContext, IServiceContext, IDbQueryServiceContext
	{
		private DbQueryHandlerContext? _queryHandlerContext;

		public IDbContextTransaction? DbContextTransaction => _queryHandlerContext?.DbContextTransaction;
		public bool IsTransactionCommitted => _queryHandlerContext?.IsTransactionCommitted ?? false;

		public DbQueryServiceContext()
			: base()
		{
		}

		protected override void OnSetQueryHandlerContext(QueryServices.Queries.QueryHandlerContext queryHandlerContext)
		{
			_queryHandlerContext = (DbQueryHandlerContext)queryHandlerContext;
		}

		public TContext CreateNewDbContext<TContext>(
			IDbContextTransaction? dbContextTransaction = null,
			bool isTransactionCommitted = false,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			string? connectionString = null)
			where TContext : DbContext
			=> _queryHandlerContext == null
				? throw new InvalidOperationException($"{nameof(_queryHandlerContext)} == null")
				: _queryHandlerContext.CreateNewDbContext<TContext>(
						dbContextTransaction,
						isTransactionCommitted,
						transactionUsage,
						transactionIsolationLevel,
						connectionString);

		public TContext GetOrCreateDbContext<TContext>(
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			string? connectionString = null) 
			where TContext : DbContext
			=> _queryHandlerContext == null
				? throw new InvalidOperationException($"{nameof(_queryHandlerContext)} == null")
				: _queryHandlerContext.GetOrCreateDbContext<TContext>(
						transactionUsage,
						transactionIsolationLevel,
						connectionString);

		public bool HasTransaction()
			=> _queryHandlerContext?.HasTransaction() ?? false;

		public void Commit()
			=> _queryHandlerContext?.Commit();

		public void Rollback()
			=> _queryHandlerContext?.Rollback();

		public Task CommitAsync(CancellationToken cancellationToken = default)
		{
			if (_queryHandlerContext != null)
				return _queryHandlerContext.CommitAsync(cancellationToken);

			return Task.CompletedTask;
		}

		public Task RollbackAsync(CancellationToken cancellationToken = default)
		{
			if (_queryHandlerContext != null)
				return _queryHandlerContext.RollbackAsync(cancellationToken);

			return Task.CompletedTask;
		}
	}
}
