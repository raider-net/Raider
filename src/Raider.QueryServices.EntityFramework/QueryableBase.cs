using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Raider.EntityFrameworkCore;
using Raider.QueryServices.EntityFramework.Queries;
using System;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Raider.QueryServices.EntityFramework
{
	public interface IQueryableBase : IDisposable, IAsyncDisposable
	{
	}

	public abstract class QueryableBase<T, TDbContext> : QueryServiceBase<DbQueryServiceContext>, IQueryableBase, IDisposable, IAsyncDisposable
		where TDbContext : DbContext
	{
		private bool disposable;
		private bool _disposed;
		private readonly IServiceProvider _serviceProvider;
		protected TDbContext DbContext { get; private set; }

		public abstract IQueryable<T> Queryable { get; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public QueryableBase(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public QueryableBase(DbQueryServiceContext serviceContext,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			bool asNoTracking = false)
		{
			SetQueryServiceContext(serviceContext ?? throw new ArgumentNullException(nameof(serviceContext)));
			SetDbContext(QueryServiceContext.GetOrCreateDbContext<TDbContext>(transactionUsage, transactionIsolationLevel), asNoTracking);
		}

		public QueryableBase(DbQueryHandlerContext queryHandlerContext,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			bool asNoTracking = false,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (queryHandlerContext == null)
				throw new ArgumentNullException(nameof(queryHandlerContext));

			SetQueryServiceContext(queryHandlerContext.GetQueryServiceContext<DbQueryServiceContext>(GetType(), memberName, sourceFilePath, sourceLineNumber));
			SetDbContext(QueryServiceContext.GetOrCreateDbContext<TDbContext>(transactionUsage, transactionIsolationLevel), asNoTracking);
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		protected void SetDbContext(TDbContext dbContext, bool asNoTracking)
		{
			DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

			if (asNoTracking)
				DbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
		}

		protected void SetDbContext<THandlerContext, TBuilder>()
			where THandlerContext : DbQueryHandlerContext
			where TBuilder : DbQueryHandlerContext.Builder<THandlerContext>
			=> SetDbContext<THandlerContext, TBuilder>(null, false);

		protected void SetDbContext<THandlerContext, TBuilder>(IDbContextTransaction? dbContextTransaction, bool isTransactionCommitted)
			where THandlerContext : DbQueryHandlerContext
			where TBuilder : DbQueryHandlerContext.Builder<THandlerContext>
		{
			SetQueryServiceContext<THandlerContext, TBuilder>(_serviceProvider, this.GetType());
			disposable = true;
			QueryServiceContext.SetIsDisposable();
			if (dbContextTransaction == null)
				SetDbContext(QueryServiceContext.GetOrCreateDbContext<TDbContext>(TransactionUsage.NONE), true);
			else
				SetDbContext(QueryServiceContext.CreateNewDbContextWithExistingTransaction<TDbContext>(dbContextTransaction, isTransactionCommitted, TransactionUsage.ReuseOrCreateNew), true);
		}

		protected IQueryable<T> DefaultInternal<TProp>(Func<IQueryable<T>, IIncludableQueryable<T, TProp>>? includableConfigurator = null)
			=> DefaultInternal<TProp, TProp>(null, includableConfigurator);

		protected virtual IQueryable<T> DefaultInternal<T1, T2>(
			Func<IQueryable<T>, IIncludableQueryable<T, T1>>? aclIncludableConfigurator = null,
			Func<IQueryable<T>, IIncludableQueryable<T, T2>>? includableConfigurator = null)
		{
			var queryable = aclIncludableConfigurator?.Invoke(Queryable) ?? Queryable;
			queryable = includableConfigurator?.Invoke(queryable) ?? queryable;
			return queryable;
		}

		public IQueryable<T> Default()
			=> Default<T>();

		public IQueryable<T> WithReadOnlyAcl()
			=> WithReadOnlyAcl<T>();

		public IQueryable<T> WithWriteAcl()
			=> WithWriteAcl<T>();

		public virtual IQueryable<T> Default<TProp>(Func<IQueryable<T>, IIncludableQueryable<T, TProp>>? includableConfigurator = null)
			=> DefaultInternal<TProp, TProp>(null, includableConfigurator);

		public virtual IQueryable<T> WithReadOnlyAcl<TProp>(Func<IQueryable<T>, IIncludableQueryable<T, TProp>>? includableConfigurator = null)
			=> DefaultInternal<TProp, TProp>(null, includableConfigurator);

		public virtual IQueryable<T> WithWriteAcl<TProp>(Func<IQueryable<T>, IIncludableQueryable<T, TProp>>? includableConfigurator = null)
			=> WithReadOnlyAcl(includableConfigurator);

		public async ValueTask DisposeAsync()
		{
			if (_disposed)
				return;

			_disposed = true;

			await DisposeAsyncCoreAsync().ConfigureAwait(false);

			Dispose(disposing: false);
			GC.SuppressFinalize(this);
		}

		protected virtual async ValueTask DisposeAsyncCoreAsync()
		{
			if (disposable)
			{
				if (DbContext != null)
					await DbContext.DisposeAsync();

				if (QueryServiceContext != null)
					await QueryServiceContext.DisposeAsync();
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (disposable)
				{
					if (DbContext != null)
						DbContext.Dispose();

					if (QueryServiceContext != null)
						QueryServiceContext.Dispose();
				}
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
