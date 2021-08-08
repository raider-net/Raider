using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Raider.EntityFrameworkCore;
using Raider.QueryServices.EntityFramework.Queries;
using System;
using System.Data;

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

		public IDbContextTransaction? DbContextTransaction => _queryHandlerContext.DbContextTransaction;

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
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			string? connectionString = null)
			where TContext : DbContext
			=> _queryHandlerContext == null
				? throw new InvalidOperationException($"{nameof(_queryHandlerContext)} == null")
				: _queryHandlerContext.CreateNewDbContext<TContext>(dbContextTransaction, transactionUsage, transactionIsolationLevel, connectionString);

		public TContext GetOrCreateDbContext<TContext>(
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			string? connectionString = null) 
			where TContext : DbContext
			=> _queryHandlerContext == null
				? throw new InvalidOperationException($"{nameof(_queryHandlerContext)} == null")
				: _queryHandlerContext.GetOrCreateDbContext<TContext>(transactionUsage, transactionIsolationLevel, connectionString);
	}
}
