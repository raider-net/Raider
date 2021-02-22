using Microsoft.Extensions.Logging;
using Raider.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	internal class Publisher<TData> : IPublisher<TData>
			where TData : IMessageData
	{
		private ILogger? _logger;

		public IReadOnlyList<ISubscriber> Subscribers { get; private set; }

		public int IdComponent { get; }
		public bool Initialized { get; private set; }
		public string Name { get; }
		public int IdScenario { get; }
		public ComponentState State { get; set; }
		public IServiceBusRegister? Register { get; set; }
		public IMessageBox? MessageBox { get; private set; }

		public Type PublishMessageDataType { get; } = typeof(TData);

		public Publisher(int idPublisher, string name, int idScenario)
		{
			IdComponent = idPublisher;
			Name = string.IsNullOrWhiteSpace(name)
				? throw new ArgumentNullException(nameof(name))
				: name;
			IdScenario = idScenario;
			Subscribers = new List<ISubscriber>();
		}

		public async Task<IMessage<TData>> PublishMessageAsync(TData data, IMessage? previousMessage = null, bool isRecovery = false, CancellationToken token = default)
		{
			if (!Initialized || MessageBox == null)
				throw new InvalidOperationException("Not initialized");

			if (Subscribers.Count == 0)
				throw new InvalidOperationException($"Not subscriber registered for message type {typeof(TData).FullName}");

			var message = new Message<TData>
			{
				IdMessage = Guid.NewGuid(),
				IdPreviousMessage = previousMessage?.IdMessage,
				IdScenario = IdScenario,
				IdPublisher = IdComponent,
				CreatedUtc = DateTimeOffset.UtcNow,
				IsRecovery = isRecovery,
				Data = data
			};

			await MessageBox.WriteAsync(new List<IMessage<TData>> { message }, Subscribers, token);

			return message;
		}

		void IPublisher.Initialize(IMessageBox messageBox, ILoggerFactory loggerFactory)
		{
			MessageBox = messageBox ?? throw new ArgumentNullException(nameof(messageBox));

			if (loggerFactory == null)
				throw new ArgumentNullException(nameof(loggerFactory));

			_logger = loggerFactory.CreateLogger(GetType());

			if (Register == null)
				throw new InvalidOperationException($"{nameof(Register)} == null");

			Subscribers = Register.GetMessageSubscribers<TData>();

			Initialized = true;
		}
	}
}
