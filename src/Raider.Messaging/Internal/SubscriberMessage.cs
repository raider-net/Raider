using System;

namespace Raider.Messaging.Messages
{
	internal class SubscriberMessage<TData> : Message<TData>, ISubscriberMessage<TData>
		where TData : IMessageData
	{
		public Guid IdSubscriberMessage { get; set; }
		public DateTimeOffset LastAccessUtc { get; set; }
		public int IdSubscriber { get; set; }
		public MessageState State { get; set; }
		public int RetryCount { get; set; }
		public DateTimeOffset? DelayedToUtc { get; set; }

		public SubscriberMessage()
		{
		}

		public SubscriberMessage(IMessage<TData> message, int idSubscriber)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			IdMessage = message.IdMessage;
			IdPreviousMessage = message.IdPreviousMessage;
			BusinessId = message.BusinessId;
			IdScenario = message.IdScenario;
			IdPublisher = message.IdPublisher;
			CreatedUtc = message.CreatedUtc;
			IsRecovery = message.IsRecovery;
			Data = message.Data;

			IdSubscriberMessage = Guid.NewGuid();
			LastAccessUtc = DateTimeOffset.UtcNow;
			IdSubscriber = idSubscriber;
			State = MessageState.Pending;
			RetryCount = 0;
			DelayedToUtc = null;
		}

		void ISubscriberMessage.UpdateMessage(MessageState state, int retryCount, DateTimeOffset? delayedToUtc)
		{
			LastAccessUtc = DateTimeOffset.UtcNow;
			State = state;
			RetryCount = retryCount;
			DelayedToUtc = delayedToUtc;
		}
	}
}
