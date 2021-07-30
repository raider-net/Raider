using Microsoft.Extensions.Logging;
using Raider.Messaging.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public interface ISubscriber : IComponent
	{
		Type SubscribingMessageDataType { get; }
		bool ReadMessagesFromSequentialFIFO { get; }
		TimeSpan TimeoutForMessageProcessing { get; }
		int MaxMessageProcessingRetryCount { get; }

		internal Task InitializeAsync(IServiceProvider serviceProvider, IServiceBusStorage storage, IMessageBox messageBox, ILoggerFactory loggerFactory, CancellationToken cancellationToken);
		Task<bool> Resume(CancellationToken cancellationToken = default);
	}

	public interface ISubscriber<TData> : ISubscriber, IComponent
			where TData : IMessageData
	{
		Task<MessageResult> ProcessMessageAsync(SubscriberContext context, ISubscriberMessage<TData> message, CancellationToken cancellationToken = default);
	}
}
