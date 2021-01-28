using Microsoft.EntityFrameworkCore;
using Raider.QueryServices.Queries;
using Raider.Trace;
using System;

namespace Raider.QueryServices
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

	public class QueryServiceContext : QueryHandlerContext
	{
		public Type ForServiceType { get; }
		public bool AllowCommit { get; set; }

		public QueryServiceContext(ITraceFrame currentTraceFrame, QueryHandlerContext queryHandlerContext, Type serviceType)
			: base(currentTraceFrame, queryHandlerContext, serviceType ?? throw new ArgumentNullException(nameof(serviceType)))
		{
			ForServiceType = serviceType;
		}
	}
}
