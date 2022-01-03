using Microsoft.Extensions.DependencyInjection;
using Raider.ServiceBus.PostgreSql.Events.Storage;
using Raider.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Events.Internal
{
	public class PostgreSqlEventBusInitializer
	{
		private readonly static Lazy<PostgreSqlEventBusInitializer> _instance = new(() => new PostgreSqlEventBusInitializer());
		public readonly static PostgreSqlEventBusInitializer Instance = _instance.Value;

		public bool Initialized { get; private set; }
		public IHost Host { get; private set; }

		private PostgreSqlEventBusInitializer()
		{
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

				var storage = serviceProvider.GetRequiredService<PostgreSqlEventBusStorage>();
				Host = await storage.InitializeHostAsync(cancellationToken);

				Initialized = true;
			}
		}
	}
}
