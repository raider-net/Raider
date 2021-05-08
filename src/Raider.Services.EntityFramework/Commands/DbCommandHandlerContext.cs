using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Raider.Commands;
using Raider.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Services.EntityFramework.Commands
{
	public abstract class DbCommandHandlerContext : Raider.Services.Commands.CommandHandlerContext, ICommandHandlerContext, IDbCommandServiceContext
	{
		private readonly ConcurrentDictionary<Type, DbContext> _dbContextCache = new ConcurrentDictionary<Type, DbContext>();

		public IDbContextTransaction? DbContextTransaction { get; private set; }

		public DbCommandHandlerContext(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
		}

		public TContext CreateNewDbContext<TContext>(TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew, IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext
		{
			var dbContext = DbContextFactory.CreateNewDbContext<TContext>(ServiceProvider, DbContextTransaction, out IDbContextTransaction? newDbContextTransaction, transactionUsage, transactionIsolationLevel);
			DbContextTransaction = newDbContextTransaction;
			return dbContext;
		}

		public TContext GetOrCreateDbContext<TContext>(TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew, IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext
		{
			var result = _dbContextCache.GetOrAdd(typeof(TContext), (dbContextType) => CreateNewDbContext<TContext>(transactionUsage, transactionIsolationLevel)).CheckDbTransaction(transactionUsage);
			return (TContext)result;
		}

		public override TService GetService<TService, TServiceContext>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (typeof(IQueryableBase).IsAssignableFrom(typeof(TService)))
				throw new NotSupportedException($"For {nameof(IQueryableBase)} use Constructor {nameof(IQueryableBase)}({nameof(DbCommandHandlerContext)}) or {nameof(IQueryableBase)}({nameof(DbServiceContext)}) isntead");

			return base.GetService<TService, TServiceContext>(memberName, sourceFilePath, sourceLineNumber);
		}

		public override Task<TService> GetServiceAsync<TService, TServiceContext>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0,
			CancellationToken cancellationToken = default)
		{
			if (typeof(IQueryableBase).IsAssignableFrom(typeof(TService)))
				throw new NotSupportedException($"For {nameof(IQueryableBase)} use Constructor {nameof(IQueryableBase)}({nameof(DbCommandHandlerContext)}) or {nameof(IQueryableBase)}({nameof(DbServiceContext)}) isntead");

			return base.GetServiceAsync<TService, TServiceContext>(memberName, sourceFilePath, sourceLineNumber, cancellationToken);
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

		public override string GetDefaultClientErrorMessage(Exception ex)
		{
			return ex is DbUpdateConcurrencyException
				? ApplicationResources.OptimisticConcurrencyException
				: ApplicationResources.GlobalExceptionMessage;
		}
	}
}
