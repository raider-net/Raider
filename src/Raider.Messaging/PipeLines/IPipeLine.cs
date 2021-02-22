using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public interface IPipeLine<T>
	{
		void AddAspect(IAspect<T?> aspect);

		void Invoke(T? data);

		Task InvokeAsync(T? data, CancellationToken cancellationToken = default);
	}
}
