using Raider.Commands;

namespace Raider.Services.Commands
{
	public abstract class CommandBase : ICommand
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

		public virtual string? SerializeResult(ICommandResult result)
			=> result == null
			? null
			: System.Text.Json.JsonSerializer.Serialize(result, _jsonSerializerOptions);

		public virtual TCommand? Deserialize<TCommand>(string command)
			where TCommand : CommandBase
			=> System.Text.Json.JsonSerializer.Deserialize<TCommand>(command, _jsonSerializerOptions);
	}

	public abstract class CommandBase<TResult> : CommandBase, ICommand<TResult>
	{
		public new TCommand? Deserialize<TCommand>(string command)
			where TCommand : CommandBase<TResult>
			=> System.Text.Json.JsonSerializer.Deserialize<TCommand>(command, _jsonSerializerOptions);

		public virtual string? SerializeResult(ICommandResult<TResult> result)
			=> result == null
			? null
			: System.Text.Json.JsonSerializer.Serialize(result, _jsonSerializerOptions);
	}
}
