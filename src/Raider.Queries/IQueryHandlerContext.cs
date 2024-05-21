using System;

namespace Raider.Queries
{
	public interface IQueryHandlerContext: IDisposable, IAsyncDisposable
	{
		bool IsDisposable { get; set; }
	}
}
