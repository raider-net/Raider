using Raider.EntityFrameworkCore;
using Raider.Queries;
using System.Data;

namespace Raider.QueryServices.Queries
{
	public class QueryHandlerOptions : IQueryHandlerOptions
	{
		public bool LogQueryEntry { get; set; } = true;
		public bool SerializeQuery { get; set; } = false;
		public TransactionUsage TransactionUsage { get; set; } = TransactionUsage.ReuseOrCreateNew;
		public IsolationLevel? TransactionIsolationLevel { get; set; }
	}
}
