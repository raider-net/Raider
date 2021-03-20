using Raider.Commands;
using Raider.EntityFrameworkCore;
using System.Data;

namespace Raider.Services.EntityFramework.Commands
{
	public class DbCommandHandlerOptions : Raider.Services.Commands.CommandHandlerOptions, ICommandHandlerOptions
	{
		public TransactionUsage TransactionUsage { get; set; } = TransactionUsage.ReuseOrCreateNew;
		public IsolationLevel? TransactionIsolationLevel { get; set; }
	}
}
