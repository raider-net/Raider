﻿using Microsoft.EntityFrameworkCore;
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

	public abstract class QueryableBase<T, TDbContext> : QueryServiceBase<QueryServiceContext>, IQueryableBase
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
			SetQueryServiceContext(serviceContext ?? throw new ArgumentNullException(nameof(serviceContext)));
			SetDbContext(QueryServiceContext.GetOrCreateDbContext<TDbContext>(transactionUsage, transactionIsolationLevel), asNoTracking);
		}

		public QueryableBase(QueryHandlerContext queryHandlerContext,
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null,
			bool asNoTracking = false,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (queryHandlerContext == null)
				throw new ArgumentNullException(nameof(queryHandlerContext));

			SetQueryServiceContext(queryHandlerContext.GetQueryServiceContext<QueryServiceContext>(GetType(), memberName, sourceFilePath, sourceLineNumber));
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
			SetQueryServiceContext<THandlerContext, TBuilder>(_serviceFactory, this.GetType());
			SetDbContext(QueryServiceContext.GetOrCreateDbContext<TDbContext>(TransactionUsage.NONE), true);
		}

		public abstract IQueryable<T> Default(Func<IQueryable<T>, IQueryable<T>>? queryableConfigurator = null);

		public abstract IQueryable<T> WithAcl(Func<IQueryable<T>, IQueryable<T>>? queryableConfigurator = null);
	}
}
