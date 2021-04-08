using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Raider.DependencyInjection;
using Raider.EntityFrameworkCore;
using Raider.QueryServices.EntityFramework.Queries;
using System;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Raider.QueryServices.EntityFramework
{
	public interface IQueryableBase
	{
	}

	public abstract class QueryableBase<T, TDbContext> : QueryServiceBase<DbQueryServiceContext>, IQueryableBase
		where TDbContext : DbContext
	{
		private readonly ServiceFactory _serviceFactory;
		protected TDbContext DbContext { get; private set; }

		public abstract IQueryable<T> Queryable { get; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public QueryableBase(ServiceFactory serviceFactory)
		{
			_serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
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
		{
			SetQueryServiceContext<THandlerContext, TBuilder>(_serviceFactory, this.GetType());
			SetDbContext(QueryServiceContext.GetOrCreateDbContext<TDbContext>(TransactionUsage.NONE), true);
		}

		protected virtual IQueryable<T> DefaultInternal<TProp>(Func<IQueryable<T>, IIncludableQueryable<T, TProp>>? includableConfigurator = null)
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

		public IQueryable<T> WithAcl()
			=> WithAcl<T>();

		public virtual IQueryable<T> Default<TProp>(Func<IQueryable<T>, IIncludableQueryable<T, TProp>>? includableConfigurator = null)
			=> DefaultInternal<TProp, TProp>(null, includableConfigurator);

		public virtual IQueryable<T> WithAcl<TProp>(Func<IQueryable<T>, IIncludableQueryable<T, TProp>>? includableConfigurator = null)
			=> DefaultInternal<TProp, TProp>(null, includableConfigurator);
	}
}
