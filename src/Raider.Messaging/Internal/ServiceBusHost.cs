using Microsoft.Extensions.DependencyInjection;
using Raider.Identity;
using System;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	internal class ServiceBusHost : IServiceBusHost
	{
		public string? ConnectionString { get; }
		public Guid IdServiceBusHost { get; }
		public Guid IdServiceBusHostRuntime { get; }
		public string Name { get; }
		public string? Description { get; set; }
		public DateTime StartedUtc { get; }
		public int? IdUser { get; }
		public RaiderIdentity<int>? User { get; private set; }
		public RaiderPrincipal<int>? Principal { get; private set; }

		public ServiceBusHost(ServiceBusHostOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			ConnectionString = string.IsNullOrWhiteSpace(options.ConncetionString)
				? throw new ArgumentException($"{nameof(options.ConncetionString)} == null", nameof(options))
				: options.ConncetionString;

			IdServiceBusHost = options.IdServiceBusHost;
			IdServiceBusHostRuntime = Guid.NewGuid();

			Name = string.IsNullOrWhiteSpace(options.Name)
				? throw new ArgumentException($"{nameof(options.Name)} == null", nameof(options))
				: options.Name;

			StartedUtc = DateTime.UtcNow;
			IdUser = options.IdUser;
		}

		public async Task Login(IServiceProvider serviceProvider)
		{
			if (!IdUser.HasValue)
				return;

			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			var authenticationManager = serviceProvider.GetService<IServiceBusAuthenticationManager>();
			if (authenticationManager == null)
				return;

			var authenticatedUser = await authenticationManager.CreateFromUserIdAsync(IdUser.Value);
			if (authenticatedUser == null)
				throw new InvalidOperationException($"401 Unauthorized - {nameof(IdUser)} = {IdUser}");

			Principal = RaiderPrincipal<int>.Create("ServiceBusAuth", authenticatedUser);
			User = Principal?.IdentityBase;

			authenticationManager.Principal = Principal;
			authenticationManager.User = User;
		}
	}
}
