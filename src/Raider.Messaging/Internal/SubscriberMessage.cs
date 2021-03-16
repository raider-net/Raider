using System;

namespace Raider.Messaging.Messages
{
	internal class SubscriberMessage<TData> : Message<TData>, ISubscriberMessage<TData>
		where TData : IMessageData
	{
		public Guid IdSubscriberMessage { get; private set; }
		public DateTimeOffset? LastAccessUtc { get; private set; }
		public int IdSubscriber { get; private set; }
		public MessageState State { get; private set; }
		public int RetryCount { get; private set; }
		public DateTimeOffset? DelayedToUtc { get; private set; }
		public Guid OriginalConcurrencyToken { get; private set; }
		public Guid NewConcurrencyToken { get; private set; }

		public SubscriberMessage(IMessage<TData> message, int idSubscriber, Guid originalConcurrencyToken)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			IdMessage = message.IdMessage;
			IdPreviousMessage = message.IdPreviousMessage;
			IdPublisherInstance = message.IdPublisherInstance;
			CreatedUtc = message.CreatedUtc;
			ValidToUtc = message.ValidToUtc;
			IsRecovery = message.IsRecovery;
			Data = message.Data;

			IdSubscriberMessage = Guid.NewGuid();
			LastAccessUtc = DateTimeOffset.UtcNow;
			IdSubscriber = idSubscriber;
			State = MessageState.Pending;
			RetryCount = 0;
			DelayedToUtc = null;
			OriginalConcurrencyToken = originalConcurrencyToken;
			NewConcurrencyToken = Guid.NewGuid();
		}

		//void ISubscriberMessage.UpdateMessage(MessageState state, int retryCount, DateTimeOffset? delayedToUtc)
		//{
		//	LastAccessUtc = DateTimeOffset.UtcNow;
		//	State = state;
		//	RetryCount = retryCount;
		//	DelayedToUtc = delayedToUtc;
		//	OriginalConcurrencyToken = NewConcurrencyToken;
		//	NewConcurrencyToken = Guid.NewGuid();
		//}
	}
}
