using Raider.Queries;

namespace Raider.QueryServices.Queries
{
	public class QueryBase<TResult> : IQuery<TResult>
	{
		public virtual string Serialize()
			=> System.Text.Json.JsonSerializer.Serialize(this);

		public virtual TQuery? Deserialize<TQuery>(string query)
			where TQuery : QueryBase<TResult>
			=> System.Text.Json.JsonSerializer.Deserialize<TQuery>(query);
	}
}
