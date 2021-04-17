using Raider.Identity;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public interface IServiceBusAuthenticationManager
	{
		public RaiderIdentity<int>? User { get; internal set; }
		public RaiderPrincipal<int>? Principal { get; internal set; }

		Task<AuthenticatedUser?> CreateFromUserIdAsync(int? idUser);
	}
}
