using System;

namespace Raider.Messaging.Messages
{
	public interface ISubscriberMessage : IMessage
	{
		Guid IdSubscriberMessage { get; }
		DateTimeOffset LastAccessUtc { get; }
		int IdSubscriber { get; }
		SubscriberMessageState State { get; }
		int RetryCount { get; }
		DateTimeOffset? DelayedToUtc { get; }

		internal void UpdateMessage(SubscriberMessageState state, int retryCount, DateTimeOffset? delayedToUtc);
	}

	public interface ISubscriberMessage<TData> : IMessage<TData>, ISubscriberMessage
		where TData : IMessageData
	{
	}
}
