using Raider.EntityFrameworkCore;
using Raider.Queries;
using System.Data;

namespace Raider.QueryServices.EntityFramework.Queries
{
	public class DbQueryHandlerOptions : Raider.QueryServices.Queries.QueryHandlerOptions, IQueryHandlerOptions
	{
		public TransactionUsage TransactionUsage { get; set; } = TransactionUsage.ReuseOrCreateNew;
		public IsolationLevel? TransactionIsolationLevel { get; set; }
	}
}
