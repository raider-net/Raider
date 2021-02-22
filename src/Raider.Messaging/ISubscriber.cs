using Microsoft.Extensions.Logging;
using Raider.Messaging.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public interface ISubscriber : IComponent
	{
		Type SubscribeMessageDataType { get; }
		bool ReadMessagesFromSequentialIFIFO { get; }
		TimeSpan MessageInProcessTimeout { get; }
		int MessageProcessRetryCount { get; }

		internal Task InitializeAsync(IMessageBox messageBox, ILoggerFactory loggerFactory, CancellationToken cancellationToken);
	}

	public interface ISubscriber<TData> : ISubscriber, IComponent
			where TData : IMessageData
	{
		Task<SubscribedMessageResult> ProcessMessageAsync(ISubscriberMessage<TData> message, CancellationToken token = default);
	}
}
