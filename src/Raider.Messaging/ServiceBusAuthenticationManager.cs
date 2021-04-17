using Raider.Identity;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public abstract class ServiceBusAuthenticationManager : IServiceBusAuthenticationManager
	{
		public RaiderIdentity<int>? User { get; internal set; }
		public RaiderPrincipal<int>? Principal { get; internal set; }

		RaiderIdentity<int>? IServiceBusAuthenticationManager.User { get => User; set => User = value; }
		RaiderPrincipal<int>? IServiceBusAuthenticationManager.Principal { get => Principal; set => Principal = value; }

		public abstract Task<AuthenticatedUser?> CreateFromUserIdAsync(int? idUser);
	}
}
