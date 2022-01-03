using Raider.Exceptions;
using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Messages.Config;
using Raider.ServiceBus.Messages.Internal;
using Raider.ServiceBus.Messages.Storage.Model;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Messages.Providers
{
	public class InMemoryMessageBus : MessageBus
	{
		private readonly InMemoryMessageBusOptions _options;
		private readonly ISerializer _serialzier;

		protected override Guid IdHost { get; }

		protected override IMessageBusOptions Options => _options;

		public InMemoryMessageBus(InMemoryMessageBusOptions options, IServiceProvider serviceProvider)
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

		private IHandlerMessageLogger? _handlerMessageLogger;
		protected override IHandlerMessageLogger GetHandlerMessageLogger()
			=> _handlerMessageLogger ??= _options.MessageLogger(ServiceProvider);

		protected override SavedMessage<TMessage> SaveRequestMessage<TMessage>(TMessage requestMessage, Messages.MessageOptions? options = null)
		{
			var requestMessageType = requestMessage.GetType();

			string? data = null;

			if (_options.EnableMessageSerialization)
				data = _serialzier.SerializeAsString(requestMessage);

			return new SavedMessage<TMessage>
			{
				IdSavedMessage = Guid.NewGuid(),
				Message = _options.EnableMessageSerialization ? (TMessage)_serialzier.Deserialize(requestMessageType, data!)! : requestMessage
			};
		}

		protected override Guid SaveResponseMessage<TResponse>(IResult<TResponse> responseMessage, IMessageHandlerContext handlerContext)
			=> Guid.NewGuid();

		protected override Task<SavedMessage<TMessage>> SaveRequestMessageAsync<TMessage>(TMessage requestMessage, Messages.MessageOptions? options = null, CancellationToken cancellation = default)
		{
			var requestMessageType = requestMessage.GetType();

			string? data = null;

			if (_options.EnableMessageSerialization)
				data = _serialzier.SerializeAsString(requestMessage);

			return Task.FromResult(
				new SavedMessage<TMessage>
				{
					IdSavedMessage = Guid.NewGuid(),
					Message = _options.EnableMessageSerialization ? (TMessage)_serialzier.Deserialize(requestMessageType, data!)! : requestMessage
				});
		}

		protected override Task<Guid> SaveResponseMessageAsync<TResponse>(IResult<TResponse> responseMessage, IMessageHandlerContext handlerContext, CancellationToken cancellation = default)
			=> Task.FromResult(Guid.NewGuid());
	}
}
