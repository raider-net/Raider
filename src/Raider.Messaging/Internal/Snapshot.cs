using System;

namespace Raider.Messaging.Messages
{
	internal class Snapshot<TData> : ISnapshot<TData>
		where TData : IMessageData
	{
		public Guid IdSnapshot { get; private set; }
		public string? SnapshotIdentifier { get; private set; }
		public DateTimeOffset LastAccessUtc { get; private set; }
		public TData? Data { get; private set; }
		public Guid OriginalConcurrencyToken { get; private set; }
		public Guid NewConcurrencyToken { get; private set; }

		//public Snapshot(IMessage<TData> message, int idSubscriber, Guid originalConcurrencyToken)
		//{
		//	if (message == null)
		//		throw new ArgumentNullException(nameof(message));

		//	IdMessage = message.IdMessage;
		//	IdPreviousMessage = message.IdPreviousMessage;
		//	IdScenario = message.IdScenario;
		//	IdPublisherInstance = message.IdPublisherInstance;
		//	CreatedUtc = message.CreatedUtc;
		//	IsRecovery = message.IsRecovery;
		//	Data = message.Data;

		//	IdSubscriberMessage = Guid.NewGuid();
		//	LastAccessUtc = DateTimeOffset.UtcNow;
		//	IdSubscriber = idSubscriber;
		//	State = MessageState.Pending;
		//	RetryCount = 0;
		//	DelayedToUtc = null;
		//	OriginalConcurrencyToken = originalConcurrencyToken;
		//	NewConcurrencyToken = Guid.NewGuid();
		//}

		//void ISubscriberMessage.UpdateMessage(TData? Data)
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
