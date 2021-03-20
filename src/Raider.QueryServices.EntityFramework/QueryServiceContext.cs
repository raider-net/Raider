using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Raider.DependencyInjection;
using Raider.EntityFrameworkCore;
using Raider.Localization;
using Raider.Logging;
using Raider.QueryServices.EntityFramework.Queries;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

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

	public class QueryServiceContext : Raider.QueryServices.QueryServiceContext, IServiceContext, IQueryServiceContext
	{
		private QueryHandlerContext? _queryHandlerContext;

		public IDbContextTransaction? DbContextTransaction => _queryHandlerContext.DbContextTransaction;

		public QueryServiceContext()
			: base()
		{
		}

		protected override void OnSetQueryHandlerContext(QueryServices.Queries.QueryHandlerContext queryHandlerContext)
		{
			_queryHandlerContext = (QueryHandlerContext)queryHandlerContext;
		}

		public TContext CreateNewDbContext<TContext>(
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext
			=> _queryHandlerContext == null
				? throw new InvalidOperationException($"{nameof(_queryHandlerContext)} == null")
				: _queryHandlerContext.CreateNewDbContext<TContext>(transactionUsage, transactionIsolationLevel);

		public TContext GetOrCreateDbContext<TContext>(
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null) 
			where TContext : DbContext
			=> _queryHandlerContext == null
				? throw new InvalidOperationException($"{nameof(_queryHandlerContext)} == null")
				: _queryHandlerContext.GetOrCreateDbContext<TContext>(transactionUsage, transactionIsolationLevel);
	}
}
