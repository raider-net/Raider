using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Raider.EntityFrameworkCore;
using Raider.Services.EntityFramework.Commands;
using System;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Raider.Services.EntityFramework
{
	public interface IQueryableBase
	{
	}

	public abstract class QueryableBase<T, TDbContext> : ServiceBase<DbServiceContext>, IQueryableBase
		where TDbContext : DbContext
	{
		private readonly IServiceProvider _serviceProvider;
		protected TDbContext DbContext { get; private set; }

		public abstract IQueryable<T> Queryable { get; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public QueryableBase(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public QueryableBase(DbServiceContext serviceContext,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			bool asNoTracking = false)
		{
			SetServiceContext(serviceContext ?? throw new ArgumentNullException(nameof(serviceContext)));
			SetDbContext(ServiceContext.GetOrCreateDbContext<TDbContext>(transactionUsage, transactionIsolationLevel), asNoTracking);
		}

		public QueryableBase(DbCommandHandlerContext commandHandlerContext,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			bool asNoTracking = false,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (commandHandlerContext == null)
				throw new ArgumentNullException(nameof(commandHandlerContext));

			SetServiceContext(commandHandlerContext.GetServiceContext<DbServiceContext>(GetType(), memberName, sourceFilePath, sourceLineNumber));
			SetDbContext(ServiceContext.GetOrCreateDbContext<TDbContext>(transactionUsage, transactionIsolationLevel), asNoTracking);
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		protected void SetDbContext(TDbContext dbContext, bool asNoTracking)
		{
			DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

			if (asNoTracking)
				DbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
		}

		protected void SetDbContext<THandlerContext, TBuilder>()
			where THandlerContext : DbCommandHandlerContext
			where TBuilder : DbCommandHandlerContext.Builder<THandlerContext>
			=> SetDbContext<THandlerContext, TBuilder>(null, false);

		protected void SetDbContext<THandlerContext, TBuilder>(IDbContextTransaction? dbContextTransaction, bool isTransactionCommitted)
			where THandlerContext : DbCommandHandlerContext
			where TBuilder : DbCommandHandlerContext.Builder<THandlerContext>
		{
			SetServiceContext<THandlerContext, TBuilder>(_serviceProvider, this.GetType());
			if (dbContextTransaction == null)
				SetDbContext(ServiceContext.GetOrCreateDbContext<TDbContext>(TransactionUsage.NONE), true);
			else
				SetDbContext(ServiceContext.CreateNewDbContextWithExistingTransaction<TDbContext>(dbContextTransaction, isTransactionCommitted, TransactionUsage.ReuseOrCreateNew), true);
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
			=> DefaultInternal<TProp, TProp>(null, includableConfigurator);
	}
}
