using Microsoft.Extensions.DependencyInjection;
using Raider.Identity;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	internal class ServiceBusHost : IServiceBusHost
	{
		private readonly int? _idUser;

		public string? ConnectionString { get; }
		public Guid IdServiceBusHost { get; }
		public IApplicationContext ApplicationContext { get; }
		public RaiderPrincipal<int>? Principal => ApplicationContext.TraceInfo.Principal;
		public string Name { get; }
		public string? Description { get; set; }
		public DateTime StartedUtc { get; }
		public IReadOnlyDictionary<object, object> Properties { get; }

		public ServiceBusHost(ServiceBusOptions options, IServiceProvider serviceProvider)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			ConnectionString = string.IsNullOrWhiteSpace(options.ConncetionString)
				? throw new ArgumentException($"{nameof(options.ConncetionString)} == null", nameof(options))
				: options.ConncetionString;

			IdServiceBusHost = options.IdServiceBusHost;

			Name = string.IsNullOrWhiteSpace(options.Name)
				? throw new ArgumentException($"{nameof(options.Name)} == null", nameof(options))
				: options.Name;

			StartedUtc = DateTime.UtcNow;
			_idUser = options.IdUser;
			Properties = new ReadOnlyDictionary<object, object>(options.Properties);
			ApplicationContext = serviceProvider.GetRequiredService<IApplicationContext>();
		}

		public async Task Login(IServiceProvider serviceProvider)
		{
			if (!_idUser.HasValue)
				return;

			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			using (var spScope = serviceProvider.CreateScope())
			{
				var appCtx = spScope.ServiceProvider.GetRequiredService<IApplicationContext>();
				var ti = appCtx.TraceInfo;
			}

			var authenticationManager = serviceProvider.GetService<IServiceBusAuthenticationManager>();
			if (authenticationManager == null)
				return;

			var authenticatedUser = await authenticationManager.CreateFromUserIdAsync(_idUser.Value, serviceProvider);
			if (authenticatedUser == null)
				throw new InvalidOperationException($"401 Unauthorized - {nameof(_idUser)} = {_idUser}");

			var principal = RaiderPrincipal<int>.Create("ServiceBusAuth", authenticatedUser);

			ApplicationContext.AddTraceFrame(TraceFrame.Create(), principal);
		}
	}
}
