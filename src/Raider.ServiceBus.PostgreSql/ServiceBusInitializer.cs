using Microsoft.Extensions.DependencyInjection;
using Raider.ServiceBus.Components;
using Raider.ServiceBus.Config.Components;
using Raider.ServiceBus.Model;
using Raider.ServiceBus.PostgreSql.Storage;
using Raider.Threading;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.PostgreSql
{
	public class ServiceBusInitializer
	{
		private readonly static Lazy<ServiceBusInitializer> _instance = new(() => new ServiceBusInitializer());
		public readonly static ServiceBusInitializer Instance = _instance.Value;

		public bool Initialized { get; private set; }
		public IHost Host { get; private set; }
		public IReadOnlyList<IScenario> Scenarios { get; private set; }
		internal IReadOnlyDictionary<Type, IMessageType> MessageTypes { get; private set; }

		private ServiceBusInitializer()
		{
			Scenarios = new List<IScenario>();
		}

		private readonly AsyncLock _initLock = new();
		public async Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
		{
			if (Initialized)
				return;

			using (await _initLock.LockAsync())
			{
				if (Initialized)
					return;

				if (serviceProvider == null)
					throw new ArgumentNullException(nameof(serviceProvider));

				var storage = serviceProvider.GetRequiredService<PostgreSqlServiceBusStorage>();
				var result = await storage.InitializeHostAsync(cancellationToken);
				Host = result.Host;
				Scenarios = result.Scenarios;
				MessageTypes = result.MessageTypes;

				Initialized = true;
			}
		}
	}
}
