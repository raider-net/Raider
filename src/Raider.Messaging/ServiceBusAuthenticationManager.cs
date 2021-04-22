using Raider.Identity;
using System;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public abstract class ServiceBusAuthenticationManager : IServiceBusAuthenticationManager
	{
		public abstract Task<AuthenticatedUser?> CreateFromUserIdAsync(int? idUser, IServiceProvider serviceProvider);
	}
}
