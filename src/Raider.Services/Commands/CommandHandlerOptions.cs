using Raider.Commands;
using Raider.EntityFrameworkCore;
using System.Data;

namespace Raider.Services.Commands
{
	public class CommandHandlerOptions : ICommandHandlerOptions
	{
		public bool SerializeCommand { get; set; } = true;
		public TransactionUsage TransactionUsage { get; set; } = TransactionUsage.ReuseOrCreateNew;
		public IsolationLevel? TransactionIsolationLevel { get; set; }
	}
}
