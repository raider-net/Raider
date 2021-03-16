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
		Task<ISubscriberMessage<TData>?> GetSubscriberMessageFromFIFOAsync<TData>(
			ISubscriber<TData> subscriber,
			List<int> readMessageStates,
			CancellationToken cancellationToken = default)
			where TData : IMessageData;

		Task<ISubscriberMessage<TData>?> GetSubscriberMessageFromNonFIFOAsync<TData>(
			ISubscriber<TData> subscriber,
			List<int> readMessageStates,
			DateTime utcNow,
			CancellationToken cancellationToken = default)
			where TData : IMessageData;

		//Task SetMessageStateAsync<TData>(ISubscriberMessage<TData> subscriberMessage, MessageResult result, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
		//	where TData : IMessageData;

		//Task SetMessageStateAsync<TData>(ISubscriberMessage<TData> subscriberMessage, MessageState state, int retryCount, DateTimeOffset? delayedToUtc, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
		//	where TData : IMessageData;

		//Task SetComponentStateAsync(IComponent component, ComponentState state, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default);

		Task WriteMessageAsync<TData>(IMessage<TData> message, IReadOnlyList<ISubscriber> subscribers, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
			where TData : IMessageData;
	}
}
