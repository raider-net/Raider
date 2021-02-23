using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Raider.DependencyInjection;
using System;
using System.Data;
using System.Data.Common;

namespace Raider.EntityFrameworkCore
{
	public static class DbContextFactory
	{
		public static TContext CreateNewDbContext<TContext>(
			ServiceFactory serviceFactory,
			IDbContextTransaction? existingDbContextTransaction,
			out IDbContextTransaction? newDbContextTransaction,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext
		{
			var dbContext = serviceFactory.GetRequiredInstance<TContext>();

			return SetDbTransaction(dbContext, existingDbContextTransaction, out newDbContextTransaction, transactionUsage, transactionIsolationLevel);
		}

		public static TContext CreateNewDbContext<TContext>(
			IServiceProvider serviceProvider,
			IDbContextTransaction? existingDbContextTransaction,
			out IDbContextTransaction? newDbContextTransaction,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext
		{
			var dbContext = serviceProvider.GetRequiredService<TContext>();

			return SetDbTransaction(dbContext, existingDbContextTransaction, out newDbContextTransaction, transactionUsage, transactionIsolationLevel);
		}

		private static TContext SetDbTransaction<TContext>(
			TContext dbContext,
			IDbContextTransaction? existingDbContextTransaction,
			out IDbContextTransaction? newDbContextTransaction,
			TransactionUsage transactionUsage,
			IsolationLevel? transactionIsolationLevel)
			where TContext : DbContext
		{
			newDbContextTransaction = null;

			if (dbContext == null)
				throw new ArgumentNullException(nameof(dbContext));

			if (transactionUsage == TransactionUsage.NONE)
				return dbContext;

			if (transactionUsage == TransactionUsage.ReuseOrCreateNew)
			{
				if (existingDbContextTransaction == null)
				{
					if (transactionIsolationLevel.HasValue)
					{
						newDbContextTransaction = dbContext.Database.BeginTransaction(transactionIsolationLevel.Value);
					}
					else
					{
						newDbContextTransaction = dbContext.Database.BeginTransaction();
					}

					return dbContext;
				}
				else
				{
					if (dbContext.Database.CurrentTransaction == null)
					{
						newDbContextTransaction = existingDbContextTransaction;
						dbContext.Database.UseTransaction((DbTransaction)newDbContextTransaction);
						return dbContext;
					}
					else
					{
						if (dbContext.Database.CurrentTransaction.TransactionId != existingDbContextTransaction.TransactionId)
							throw new InvalidOperationException($"DbContext already has set another transaction with id {dbContext.Database.CurrentTransaction.TransactionId}");

						return dbContext;
					}
				}
			}

			if (transactionUsage == TransactionUsage.CreateNew)
			{
				if (dbContext.Database.CurrentTransaction == null)
				{
					if (transactionIsolationLevel.HasValue)
					{
						newDbContextTransaction = dbContext.Database.BeginTransaction(transactionIsolationLevel.Value);
					}
					else
					{
						newDbContextTransaction = dbContext.Database.BeginTransaction();
					}

					return dbContext;
				}
				else
				{
					throw new InvalidOperationException($"DbContext already has set another transaction with id {dbContext.Database.CurrentTransaction.TransactionId}");
				}
			}

			return dbContext;
		}
	}
}
