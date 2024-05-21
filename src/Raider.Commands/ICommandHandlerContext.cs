using System;

namespace Raider.Commands
{
	public interface ICommandHandlerContext : IDisposable, IAsyncDisposable
	{
		bool IsDisposable { get; set; }
	}
}
