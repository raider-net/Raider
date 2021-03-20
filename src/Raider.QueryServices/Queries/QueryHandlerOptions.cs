using Raider.Queries;

namespace Raider.QueryServices.Queries
{
	public class QueryHandlerOptions : IQueryHandlerOptions
	{
		public bool LogQueryEntry { get; set; } = true;
		public bool SerializeQuery { get; set; } = false;
	}
}
