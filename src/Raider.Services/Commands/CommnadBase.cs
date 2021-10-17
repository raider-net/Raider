using Raider.Commands;

namespace Raider.Services.Commands
{
	public abstract class CommandBase : ICommand
	{
		public abstract string CommandIdentifier { get; }

		public virtual string Serialize()
			=> System.Text.Json.JsonSerializer.Serialize(this);

		public virtual TCommand? Deserialize<TCommand>(string command)
			where TCommand : CommandBase
			=> System.Text.Json.JsonSerializer.Deserialize<TCommand>(command);
	}

	public abstract class CommandBase<TResult> : CommandBase, ICommand<TResult>
	{
		public new TCommand? Deserialize<TCommand>(string command)
			where TCommand : CommandBase<TResult>
			=> System.Text.Json.JsonSerializer.Deserialize<TCommand>(command);
	}
}
