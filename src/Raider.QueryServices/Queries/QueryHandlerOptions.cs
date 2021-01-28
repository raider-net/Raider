using Raider.Queries;
using System.Data;

namespace Raider.QueryServices.Queries
{
	public enum TransactionUsage
	{
		NONE = 0,
		ReuseOrCreateNew = 1
	}

	public class QueryHandlerOptions : IQueryHandlerOptions
	{
		public bool SerializeQuery { get; set; } = true;
		public TransactionUsage TransactionUsage { get; set; } = TransactionUsage.ReuseOrCreateNew;
		public IsolationLevel? TransactionIsolationLevel { get; set; }
	}
}
