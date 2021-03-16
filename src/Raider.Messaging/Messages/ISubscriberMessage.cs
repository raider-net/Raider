using System;

namespace Raider.Messaging.Messages
{
	public interface ISubscriberMessage : IMessage
	{
		Guid IdSubscriberMessage { get; }
		DateTimeOffset? LastAccessUtc { get; }
		int IdSubscriber { get; }
		MessageState State { get; }
		int RetryCount { get; }
		DateTimeOffset? DelayedToUtc { get; }
		Guid OriginalConcurrencyToken { get; }
		Guid NewConcurrencyToken { get; }

		//internal void UpdateMessage(MessageState state, int retryCount, DateTimeOffset? delayedToUtc);
	}

	public interface ISubscriberMessage<TData> : IMessage<TData>, ISubscriberMessage
		where TData : IMessageData
	{
	}
}
