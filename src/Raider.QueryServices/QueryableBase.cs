using Microsoft.EntityFrameworkCore;
using Raider.DependencyInjection;
using Raider.QueryServices.Queries;
using System;
using System.Data;
using System.Linq;

namespace Raider.QueryServices
{
	public abstract class QueryableBase<T, TDbContext> : QueryServiceBase
		where TDbContext : DbContext
	{
		private readonly ServiceFactory _serviceFactory;
		protected TDbContext DbContext { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public QueryableBase(ServiceFactory serviceFactory)
		{
			_serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
		}

		public QueryableBase(QueryServiceContext serviceContext,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			bool asNoTracking = false)
		{
			QueryServiceContext = serviceContext ?? throw new ArgumentNullException(nameof(serviceContext));
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
			where THandlerContext : QueryHandlerContext
			where TBuilder : QueryHandlerContext.Builder<THandlerContext>
		{
			var contextFactory = _serviceFactory.GetRequiredInstance<ContextFactory>();
			QueryServiceContext = contextFactory.CreateQueryServiceContext<THandlerContext, TBuilder>(this.GetType());
			SetDbContext(QueryServiceContext.GetOrCreateDbContext<TDbContext>(TransactionUsage.NONE), true);
		}

		public abstract IQueryable<T> Default(Func<IQueryable<T>, IQueryable<T>>? queryableConfigurator = null);

		public abstract IQueryable<T> WithAcl(Func<IQueryable<T>, IQueryable<T>>? queryableConfigurator = null);
	}
}
