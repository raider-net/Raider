using Raider.Commands;

namespace Raider.Services.Commands
{
	public abstract class CommandBase : ICommand
	{
		public virtual string Serialize()
			=> System.Text.Json.JsonSerializer.Serialize(this);

		public virtual TCommand? Deserialize<TCommand>(string command)
			where TCommand : CommandBase
			=> System.Text.Json.JsonSerializer.Deserialize<TCommand>(command);
	}

	public class CommandBase<TResult> : ICommand<TResult>
	{
		public virtual string Serialize()
			=> System.Text.Json.JsonSerializer.Serialize(this);

		public virtual TCommand? Deserialize<TCommand>(string command)
			where TCommand : CommandBase<TResult>
			=> System.Text.Json.JsonSerializer.Deserialize<TCommand>(command);
	}
}
