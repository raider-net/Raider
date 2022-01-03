using Raider.Exceptions;
using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Events.Config;
using Raider.ServiceBus.Events.Internal;
using Raider.ServiceBus.Events.Storage.Model;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Events.Providers
{
	public class InMemoryEventBus : EventBus
	{
		private readonly InMemoryEventBusOptions _options;
		private readonly ISerializer _serialzier;

		protected override Guid IdHost { get; }

		protected override IEventBusOptions Options => _options;

		public InMemoryEventBus(InMemoryEventBusOptions options, IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			var sb = _options.Validate();
			var error = sb?.ToString();
			if (!string.IsNullOrWhiteSpace(error))
				throw new ConfigurationException(error);

			_serialzier = _options.MessageSerializer(serviceProvider);

			if (_serialzier == null)
				throw new InvalidOperationException($"{nameof(_serialzier)} == NULL");
		}

		private IHostLogger? _hostLogger;

		protected override ITransactionContext CreateTransactionContext()
			=> TransactionContextBuilder.Create().Build();

		protected override Task<ITransactionContext> CreateTransactionContextAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult(TransactionContextBuilder.Create().Build());

		protected override IHostLogger GetHostLogger()
			=> _hostLogger ??= _options.HostLogger(ServiceProvider);

		private IHandlerMessageLogger? _handlerEventLogger;
		protected override IHandlerMessageLogger GetHandlerEventLogger()
			=> _handlerEventLogger ??= _options.EventLogger(ServiceProvider);

		protected override SavedEvent<TEvent> SaveEvent<TEvent>(TEvent @event, EventOptions? options = null)
		{
			var requestMessageType = @event.GetType();

			string? data = null;

			if (_options.EnableMessageSerialization)
				data = _serialzier.SerializeAsString(@event);

			return new SavedEvent<TEvent>
			{
				IdSavedEvent = Guid.NewGuid(),
				Event = _options.EnableMessageSerialization ? (TEvent)_serialzier.Deserialize(requestMessageType, data!)! : @event
			};
		}

		protected override Task<SavedEvent<TEvent>> SaveEventAsync<TEvent>(TEvent @event, EventOptions? options = null, CancellationToken cancellation = default)
		{
			var requestMessageType = @event.GetType();

			string? data = null;

			if (_options.EnableMessageSerialization)
				data = _serialzier.SerializeAsString(@event);

			return Task.FromResult(
				new SavedEvent<TEvent>
				{
					IdSavedEvent = Guid.NewGuid(),
					Event = _options.EnableMessageSerialization ? (TEvent)_serialzier.Deserialize(requestMessageType, data!)! : @event
				});
		}
	}
}
