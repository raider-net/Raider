using Raider.Commands;
using Raider.EntityFrameworkCore;
using System.Data;

namespace Raider.Services.Commands
{
	public class CommandHandlerOptions : ICommandHandlerOptions
	{
		public bool LogCommandEntry { get; set; } = true;
		public bool SerializeCommand { get; set; } = false;
		public TransactionUsage TransactionUsage { get; set; } = TransactionUsage.ReuseOrCreateNew;
		public IsolationLevel? TransactionIsolationLevel { get; set; }
	}
}
