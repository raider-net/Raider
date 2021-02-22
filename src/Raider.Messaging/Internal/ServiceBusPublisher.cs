using Microsoft.Extensions.Logging;
using Raider.Messaging.Messages;
using Raider.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	internal class ServiceBusPublisher : IServiceBus
	{
		private readonly IMessageBox _messageBox;
		private readonly IServiceBusRegister _register;
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _logger;

		public ServiceBusPublisher(IMessageBox messageBox, IServiceBusRegister register, ILoggerFactory loggerFactory)
		{
			_messageBox = messageBox ?? throw new ArgumentNullException(nameof(messageBox));
			_register = register ?? throw new ArgumentNullException(nameof(register));
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_logger = _loggerFactory.CreateLogger<ServiceBusPublisher>();

			if (!_register.RegistrationFinished)
				throw new InvalidOperationException("Component registration was not finished.");

			_register.InitializePublishers(_messageBox, _loggerFactory);
		}

		public Task<IMessage<TData>> PublishMessageAsync<TData>(int idPublisher, TData mesage, IMessage? previousMessage = null, bool isRecovery = false, CancellationToken token = default) where TData : IMessageData
		{
			var publisher = _register.TryGetPublisher<TData>(idPublisher);
			if (publisher == null)
				throw new ArgumentException($"Not registered {nameof(idPublisher)}: {idPublisher}", nameof(idPublisher));

			return publisher.PublishMessageAsync(mesage, previousMessage, isRecovery, token);
		}
	}
}
