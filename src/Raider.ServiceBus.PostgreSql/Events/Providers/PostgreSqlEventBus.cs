using Microsoft.Extensions.DependencyInjection;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Events;
using Raider.ServiceBus.Events.Config;
using Raider.ServiceBus.Events.Internal;
using Raider.ServiceBus.Events.Storage.Model;
using Raider.ServiceBus.PostgreSql.Events.Storage;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.PostgreSql.Events.Providers
{
	internal class PostgreSqlEventBus : EventBus
	{
		private readonly PostgreSqlEventBusOptions _options;
		private readonly PostgreSqlEventBusStorage _storage;

		protected override Guid IdHost { get; }

		protected override IEventBusOptions Options => _options;

		public PostgreSqlEventBus(PostgreSqlEventBusOptions options, IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_storage = serviceProvider.GetRequiredService<PostgreSqlEventBusStorage>();

			if (!PostgreSqlEventBusInitializer.Instance.Initialized)
				throw new InvalidOperationException($"{nameof(PostgreSqlEventBus)} is not initialized. Run {nameof(PostgreSqlEventBusInitializer)}.{nameof(PostgreSqlEventBusInitializer.InitializeAsync)} first.");

			IdHost = PostgreSqlEventBusInitializer.Instance.Host.IdHost;
		}

		protected override ITransactionContext CreateTransactionContext()
			=> _storage.CreateTransactionContext();

		protected override Task<ITransactionContext> CreateTransactionContextAsync(CancellationToken cancellationToken = default)
			=> _storage.CreateTransactionContextAsync(cancellationToken);

		protected override IHostLogger GetHostLogger()
			=> _storage;

		protected override IHandlerMessageLogger GetHandlerEventLogger()
			=> _storage;

		protected override SavedEvent<TEvent> SaveEvent<TEvent>(TEvent @event, EventOptions? options = null)
			=> _storage.CreateEvent(@event, options);

		protected override Task<SavedEvent<TEvent>> SaveEventAsync<TEvent>(TEvent requestevent, EventOptions? options = null, CancellationToken cancellation = default)
			=> _storage.CreateEventAsync(requestevent, options, cancellation);
	}
}
