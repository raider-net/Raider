using Raider.Identity;

namespace Raider.Messaging
{
	public interface IServiceBusAuthentication
	{
		RaiderPrincipal<int>? Principal { get; }
	}
}
