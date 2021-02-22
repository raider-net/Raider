using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public class Aspect<T> : IAspect<T?>
	{
		public IAspect<T?>? Next { get; internal set; }

		public virtual Action<T?>? Execute { get; }

		public virtual Func<T?, CancellationToken, Task>? ExecuteAsync { get; }

		IAspect<T?> IAspect<T?>.SetNext(IAspect<T?> next)
			=> SetNext(next);

		internal IAspect<T?> SetNext(IAspect<T?> next)
			=> Next = next;
	}
}
