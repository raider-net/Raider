using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public interface IAspect<T>
	{
		IAspect<T?>? Next { get; }

		Action<T?>? Execute { get; }

		Func<T?, CancellationToken, Task>? ExecuteAsync { get; }

		internal IAspect<T?> SetNext(IAspect<T?> next);
	}
}
