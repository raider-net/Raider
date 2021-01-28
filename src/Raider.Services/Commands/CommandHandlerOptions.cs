using Raider.Commands;
using System.Data;

namespace Raider.Services.Commands
{
	public enum TransactionUsage
	{
		NONE = 0,
		ReuseOrCreateNew = 1
	}

	public class CommandHandlerOptions : ICommandHandlerOptions
	{
		public bool SerializeCommand { get; set; } = true;
		public TransactionUsage TransactionUsage { get; set; } = TransactionUsage.ReuseOrCreateNew;
		public IsolationLevel? TransactionIsolationLevel { get; set; }
	}
}
