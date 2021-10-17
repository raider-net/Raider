using Raider.Queries;

namespace Raider.QueryServices.Queries
{
	public abstract class QueryBase<TResult> : IQuery<TResult>
	{
		public abstract string CommandIdentifier { get; }

		public virtual string Serialize()
			=> System.Text.Json.JsonSerializer.Serialize(this);

		public virtual TQuery? Deserialize<TQuery>(string query)
			where TQuery : QueryBase<TResult>
			=> System.Text.Json.JsonSerializer.Deserialize<TQuery>(query);
	}
}
