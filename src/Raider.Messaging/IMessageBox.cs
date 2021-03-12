using Raider.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Data;
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

		Task SetMessageStateAsync<TData>(ISubscriberMessage<TData> subscriberMessage, MessageResult result, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
			where TData : IMessageData;

		Task SetMessageStateAsync<TData>(ISubscriberMessage<TData> subscriberMessage, MessageState state, int retryCount, DateTimeOffset? delayedToUtc, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
			where TData : IMessageData;

		Task SetComponentStateAsync(IComponent component, ComponentState state, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default);

		Task<bool> WriteAsync<TData>(List<IMessage<TData>> messages, IReadOnlyList<ISubscriber> subscribers, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
			where TData : IMessageData;
	}
}
