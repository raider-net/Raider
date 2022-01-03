using Microsoft.Extensions.DependencyInjection;
using Raider.ServiceBus.PostgreSql.Messages.Storage;
using Raider.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Messages.Internal
{
	public class PostgreSqlMessageBusInitializer
	{
		private readonly static Lazy<PostgreSqlMessageBusInitializer> _instance = new(() => new PostgreSqlMessageBusInitializer());
		public readonly static PostgreSqlMessageBusInitializer Instance = _instance.Value;

		public bool Initialized { get; private set; }
		public IHost Host { get; private set; }

		private PostgreSqlMessageBusInitializer()
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

				var storage = serviceProvider.GetRequiredService<PostgreSqlMessageBusStorage>();
				Host = await storage.InitializeHostAsync(cancellationToken);

				Initialized = true;
			}
		}
	}
}
