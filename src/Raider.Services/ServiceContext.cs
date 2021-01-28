using Microsoft.EntityFrameworkCore;
using Raider.Services.Commands;
using Raider.Trace;
using System;

namespace Raider.Services
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

	public class ServiceContext : CommandHandlerContext
	{
		public Type ForServiceType { get; }
		public bool AllowCommit { get; set; }

		public ServiceContext(ITraceFrame currentTraceFrame, CommandHandlerContext commandHandlerContext, Type serviceType)
			: base(currentTraceFrame, commandHandlerContext, serviceType ?? throw new ArgumentNullException(nameof(serviceType)))
		{
			ForServiceType = serviceType;
		}
	}
}
