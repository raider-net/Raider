using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public interface IComponent
	{
		int IdComponent { get; }
		Guid IdInstance { get; }
		bool Initialized { get; }
		bool Started { get; }
		string Name { get; }
		string? Description { get; }
		int IdScenario { get; }
		DateTime LastActivityUtc { get; }
		ComponentState State { get; }

		internal Task StartAsync(IServiceBusStorageContext context, CancellationToken cancellationToken);
	}
}
