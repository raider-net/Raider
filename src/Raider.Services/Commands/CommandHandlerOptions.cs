using Raider.Commands;

namespace Raider.Services.Commands
{
	public class CommandHandlerOptions : ICommandHandlerOptions
	{
		public bool LogCommandEntry { get; set; } = true;
		public bool SerializeCommand { get; set; } = false;
	}
}
