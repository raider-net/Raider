﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Raider.EntityFrameworkCore;
using Raider.Services.EntityFramework.Commands;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Services.EntityFramework
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

	public class DbServiceContext : Raider.Services.ServiceContext, IServiceContext, IDbCommandServiceContext
	{
		private DbCommandHandlerContext? _commandHandlerContext;

		public IDbContextTransaction? DbContextTransaction => _commandHandlerContext?.DbContextTransaction;
		public bool IsTransactionCommitted => _commandHandlerContext?.IsTransactionCommitted ?? false;

		public DbServiceContext()
			: base()
		{
		}

		protected override void OnSetCommandHandlerContext(Services.Commands.CommandHandlerContext commandHandlerContext)
		{
			_commandHandlerContext = (DbCommandHandlerContext)commandHandlerContext;
		}

		public TContext CreateNewDbContextWithNewTransaction<TContext>(
			out IDbContextTransaction? newDbContextTransaction,
			IsolationLevel? transactionIsolationLevel = null,
			Func<bool>? isTransactionCommittedDelegate = null,
			string? connectionString = null)
			where TContext : DbContext
			=> _commandHandlerContext == null
				? throw new InvalidOperationException($"{nameof(_commandHandlerContext)} == null")
				: _commandHandlerContext.CreateNewDbContextWithNewTransaction<TContext>(
						out newDbContextTransaction,
						transactionIsolationLevel,
						isTransactionCommittedDelegate,
						connectionString);

		public TContext CreateNewDbContextWithoutTransaction<TContext>(DbConnection? externalDbConnection = null, string? connectionString = null)
			where TContext : DbContext
			=> _commandHandlerContext == null
				? throw new InvalidOperationException($"{nameof(_commandHandlerContext)} == null")
				: _commandHandlerContext.CreateNewDbContextWithoutTransaction<TContext>(
						externalDbConnection,
						connectionString);

		public TContext CreateNewDbContextWithExistingTransaction<TContext>(
			IDbContextTransaction dbContextTransaction,
			bool isTransactionCommitted = false,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			string? connectionString = null)
			where TContext : DbContext
			=> _commandHandlerContext == null
				? throw new InvalidOperationException($"{nameof(_commandHandlerContext)} == null")
				: _commandHandlerContext.CreateNewDbContextWithExistingTransaction<TContext>(
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
			=> _commandHandlerContext == null
				? throw new InvalidOperationException($"{nameof(_commandHandlerContext)} == null")
				: _commandHandlerContext.GetOrCreateDbContext<TContext>(
						transactionUsage,
						transactionIsolationLevel,
						connectionString);

		public void SetDbContext<TContext>(TContext dbContext)
			where TContext : DbContext
		{
			if (_commandHandlerContext == null)
				throw new InvalidOperationException($"{nameof(_commandHandlerContext)} == null");

			_commandHandlerContext?.SetDbContext(dbContext);
		}

		public void ResetDbContext<TContext>(TContext dbContext)
			where TContext : DbContext
		{
			if (_commandHandlerContext == null)
				throw new InvalidOperationException($"{nameof(_commandHandlerContext)} == null");

			_commandHandlerContext?.ResetDbContext(dbContext);
		}

		public bool HasTransaction()
			=> _commandHandlerContext?.HasTransaction() ?? false;

		public void Commit()
			=> _commandHandlerContext?.Commit();

		public void Rollback()
			=> _commandHandlerContext?.Rollback();

		public Task CommitAsync(CancellationToken cancellationToken = default)
		{
			if (_commandHandlerContext != null)
				return _commandHandlerContext.CommitAsync(cancellationToken);

			return Task.CompletedTask;
		}

		public Task RollbackAsync(CancellationToken cancellationToken = default)
		{
			if (_commandHandlerContext != null)
				return _commandHandlerContext.RollbackAsync(cancellationToken);

			return Task.CompletedTask;
		}
	}
}
