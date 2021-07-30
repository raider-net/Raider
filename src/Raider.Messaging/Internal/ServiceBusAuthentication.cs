using Raider.Identity;

namespace Raider.Messaging.Internal
{
	public class ServiceBusAuthentication : IServiceBusAuthentication
	{
		public RaiderPrincipal<int>? Principal => ServiceBus.Principal;
	}
}
