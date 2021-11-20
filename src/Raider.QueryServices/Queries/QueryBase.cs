using Raider.Queries;

namespace Raider.QueryServices.Queries
{
	public abstract class QueryBase<TResult> : IQuery<TResult>
	{
		protected readonly System.Text.Json.JsonSerializerOptions _jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
		{
			WriteIndented = false,
			ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
			Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

		public abstract string CommandIdentifier { get; }

		public virtual string Serialize()
			=> System.Text.Json.JsonSerializer.Serialize(this, _jsonSerializerOptions);

		public virtual string? SerializeResult(IQueryResult<TResult> result)
			=> result == null
			? null
			: System.Text.Json.JsonSerializer.Serialize(result, _jsonSerializerOptions);

		public virtual TQuery? Deserialize<TQuery>(string query)
			where TQuery : QueryBase<TResult>
			=> System.Text.Json.JsonSerializer.Deserialize<TQuery>(query, _jsonSerializerOptions);
	}
}
