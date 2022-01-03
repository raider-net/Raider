using Microsoft.Extensions.DependencyInjection;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Messages;
using Raider.ServiceBus.Messages.Config;
using Raider.ServiceBus.Messages.Internal;
using Raider.ServiceBus.Messages.Storage.Model;
using Raider.ServiceBus.PostgreSql.Messages.Storage;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.PostgreSql.Messages.Providers
{
	internal class PostgreSqlMessageBus : MessageBus
	{
		private readonly PostgreSqlMessageBusOptions _options;
		private readonly PostgreSqlMessageBusStorage _storage;

		protected override Guid IdHost { get; }

		protected override IMessageBusOptions Options => _options;

		public PostgreSqlMessageBus(PostgreSqlMessageBusOptions options, IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_storage = serviceProvider.GetRequiredService<PostgreSqlMessageBusStorage>();

			if (!PostgreSqlMessageBusInitializer.Instance.Initialized)
				throw new InvalidOperationException($"{nameof(PostgreSqlMessageBus)} is not initialized. Run {nameof(PostgreSqlMessageBusInitializer)}.{nameof(PostgreSqlMessageBusInitializer.InitializeAsync)} first.");

			IdHost = PostgreSqlMessageBusInitializer.Instance.Host.IdHost;
		}

		protected override ITransactionContext CreateTransactionContext()
			=> _storage.CreateTransactionContext();

		protected override Task<ITransactionContext> CreateTransactionContextAsync(CancellationToken cancellationToken = default)
			=> _storage.CreateTransactionContextAsync(cancellationToken);

		protected override IHostLogger GetHostLogger()
			=> _storage;

		protected override IHandlerMessageLogger GetHandlerMessageLogger()
			=> _storage;

		protected override SavedMessage<TMessage> SaveRequestMessage<TMessage>(TMessage requestMessage, ServiceBus.Messages.MessageOptions? options = null)
			=> _storage.CreateRequestMessage(requestMessage, options);

		protected override Guid SaveResponseMessage<TResponse>(IResult<TResponse> responseMessage, IMessageHandlerContext handlerContext)
			=> _storage.CreateResponseMessage(responseMessage, handlerContext);

		protected override Task<SavedMessage<TMessage>> SaveRequestMessageAsync<TMessage>(TMessage requestMessage, ServiceBus.Messages.MessageOptions? options = null, CancellationToken cancellation = default)
			=> _storage.CreateRequestMessageAsync(requestMessage, options, cancellation);

		protected override Task<Guid> SaveResponseMessageAsync<TResponse>(IResult<TResponse> responseMessage, IMessageHandlerContext handlerContext, CancellationToken cancellation = default)
			=> _storage.CreateResponseMessageAsync(responseMessage, handlerContext, cancellation);
	}
}
