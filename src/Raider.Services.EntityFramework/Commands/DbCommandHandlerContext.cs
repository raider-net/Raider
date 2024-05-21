using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Raider.Commands;
using Raider.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Services.EntityFramework.Commands
{
	public abstract class DbCommandHandlerContext : Raider.Services.Commands.CommandHandlerContext, ICommandHandlerContext, IDbCommandServiceContext
	{
		private readonly ConcurrentDictionary<Type, DbContext> _dbContextCache = new ConcurrentDictionary<Type, DbContext>();

		public IDbContextTransaction? DbContextTransaction { get; private set; }
		public bool IsTransactionCommitted { get; private set; }

		public DbCommandHandlerContext(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
		}

		public TContext CreateNewDbContextWithNewTransaction<TContext>(
			out IDbContextTransaction? newDbContextTransaction,
			IsolationLevel? transactionIsolationLevel = null,
			Func<bool>? isTransactionCommittedDelegate = null,
			string? connectionString = null)
			where TContext : DbContext
			=> DbContextFactory.CreateNewDbContext<TContext>(
				ServiceProvider,
				null,
				out newDbContextTransaction,
				TransactionUsage.CreateNew,
				transactionIsolationLevel,
				isTransactionCommittedDelegate,
				connectionString,
				CommandName,
				IdCommandEntry);

		public TContext CreateNewDbContextWithoutTransaction<TContext>(DbConnection? externalDbConnection = null, string? connectionString = null)
			where TContext : DbContext
			=> DbContextFactory.CreateNewDbContextWithoutTransaction<TContext>(
				ServiceProvider,
				externalDbConnection,
				connectionString,
				CommandName,
				IdCommandEntry);

		public TContext CreateNewDbContextWithExistingTransaction<TContext>(
			IDbContextTransaction dbContextTransaction,
			bool isTransactionCommitted = false,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			string? connectionString = null)
			where TContext : DbContext
			=> CreateNewDbContextInternal<TContext>(
				dbContextTransaction,
				isTransactionCommitted,
				transactionUsage,
				transactionIsolationLevel,
				connectionString);

		private TContext CreateNewDbContextInternal<TContext>(
			IDbContextTransaction? dbContextTransaction = null,
			bool isTransactionCommitted = false,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			string? connectionString = null)
			where TContext : DbContext
		{
			var oldTransaction = dbContextTransaction ?? (transactionUsage == TransactionUsage.ReuseOrCreateNew ? DbContextTransaction : null);
			var dbContext = DbContextFactory.CreateNewDbContext<TContext>(
				ServiceProvider,
				oldTransaction,
				out IDbContextTransaction? newDbContextTransaction,
				transactionUsage,
				transactionIsolationLevel,
				() => transactionUsage == TransactionUsage.ReuseOrCreateNew && IsTransactionCommitted,
				connectionString,
				CommandName,
				IdCommandEntry);

			if (transactionUsage == TransactionUsage.ReuseOrCreateNew)
			{
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
			}

			return dbContext;
		}

		public TContext GetOrCreateDbContext<TContext>(
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			string? connectionString = null)
			where TContext : DbContext
		{
			var result = _dbContextCache.GetOrAdd(typeof(TContext), (dbContextType) => CreateNewDbContextInternal<TContext>(
				null,
				false,
				transactionUsage,
				transactionIsolationLevel,
				connectionString)).CheckDbTransaction(transactionUsage);
			return (TContext)result;
		}

		public void SetDbContext<TContext>(TContext dbContext)
			where TContext : DbContext
		{
			var result = _dbContextCache.TryAdd(typeof(TContext), dbContext);
			if (!result)
				throw new InvalidOperationException($"{nameof(dbContext)} type {typeof(TContext).FullName} already exists.");
		}

		public void ResetDbContext<TContext>(TContext dbContext)
			where TContext : DbContext
			=> _dbContextCache.AddOrUpdate(typeof(TContext), dbContext, (key, oldValue) => dbContext);

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

		public override void Dispose()
		{
			if (IsDisposable)
			{
				foreach (var item in _dbContextCache)
				{
					item.Value.Dispose();
				}

				_dbContextCache.Clear();
			}
		}

		public override async ValueTask DisposeAsync()
		{
			if (IsDisposable)
			{
				foreach (var item in _dbContextCache)
				{
					await item.Value.DisposeAsync();
				}

				_dbContextCache.Clear();
			}
		}

		public override string GetDefaultClientErrorMessage(Exception ex)
		{
			return ex is DbUpdateConcurrencyException
				? ApplicationResources.OptimisticConcurrencyException
				: ApplicationResources.GlobalExceptionMessage;
		}
	}
}
