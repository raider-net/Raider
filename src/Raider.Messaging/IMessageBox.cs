using Raider.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public interface IMessageBox
	{
		Task<ISubscriberMessage<TData>?> GetFirstSubscriberMessageAsync<TData>(ISubscriber<TData> subscriber, CancellationToken cancellationToken = default)
			where TData : IMessageData;

		Task<ISubscriberMessage<TData>?> GetNextSubscriberMessageAsync<TData>(ISubscriber<TData> subscriber, CancellationToken cancellationToken = default)
			where TData : IMessageData;

		Task SetMessageStateAsync<TData>(ISubscriberMessage<TData> subscriberMessage, SubscribedMessageResult result, CancellationToken cancellationToken = default)
			where TData : IMessageData;

		Task SetMessageStateAsync<TData>(ISubscriberMessage<TData> subscriberMessage, SubscriberMessageState state, int retryCount, DateTimeOffset? delayedToUtc, CancellationToken cancellationToken = default)
			where TData : IMessageData;

		Task SetSubscriberStateAsync(ISubscriber subscriber, ComponentState state, CancellationToken cancellationToken = default);

		Task<bool> WriteAsync<TData>(List<IMessage<TData>> messages, IReadOnlyList<ISubscriber> subscribers, CancellationToken cancellationToken = default)
			where TData : IMessageData;
	}
}
