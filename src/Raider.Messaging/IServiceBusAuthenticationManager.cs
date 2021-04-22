using Raider.Identity;
using System;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public interface IServiceBusAuthenticationManager
	{
		Task<AuthenticatedUser?> CreateFromUserIdAsync(int? idUser, IServiceProvider serviceProvider);
	}
}
