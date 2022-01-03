using Microsoft.Extensions.DependencyInjection;
using Raider.Converters;
using Raider.ServiceBus.Components;
using Raider.ServiceBus.PostgreSql.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raider.ServiceBus.PostgreSql
{
	internal class PostgreSqlServiceBus : IServiceBus
	{
		private readonly IPostgreSqlBusOptions _options;
		private readonly IServiceProvider _serviceProvider;
		private readonly PostgreSqlServiceBusStorage _storage;

		public Guid IdHost { get; }

		public PostgreSqlServiceBus(IPostgreSqlBusOptions options, IServiceProvider serviceProvider)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_storage = serviceProvider.GetRequiredService<PostgreSqlServiceBusStorage>();

			if (!ServiceBusInitializer.Instance.Initialized)
				throw new InvalidOperationException($"{nameof(PostgreSqlServiceBus)} is not initialized. Run {nameof(ServiceBusInitializer)}.{nameof(ServiceBusInitializer.InitializeAsync)} first.");

			IdHost = ServiceBusInitializer.Instance.Host.IdHost;
		}
	}
}
