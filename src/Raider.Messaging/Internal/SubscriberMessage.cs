using System;
using System.Diagnostics.CodeAnalysis;

namespace Raider.Messaging.Messages
{
	internal class SubscriberMessage<TData> : Message<TData>, ISubscriberMessage<TData>
		where TData : IMessageData
	{
		public Guid IdSubscriberMessage { get; private set; }
		public DateTimeOffset? LastAccessUtc { get; private set; }
		public int IdSubscriber { get; private set; }
		public MessageState State { get; private set; }
		public string? Snapshot { get; set; }
		public int RetryCount { get; private set; }
		public DateTimeOffset? DelayedToUtc { get; private set; }
		public Guid OriginalConcurrencyToken { get; private set; }
		public Guid NewConcurrencyToken { get; private set; }

		public SubscriberMessage(IMessage<TData> message, int idSubscriber, Guid originalConcurrencyToken, string? snapshot = null)
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
			Snapshot = snapshot;
			RetryCount = 0;
			DelayedToUtc = null;
			OriginalConcurrencyToken = originalConcurrencyToken;
			NewConcurrencyToken = Guid.NewGuid();
		}

		public bool TryGetSnapshotData<T>([NotNullWhen(true)] out T? snapshot)
		{
			snapshot = default;

			if (string.IsNullOrWhiteSpace(Snapshot))
				return false;

			snapshot = System.Text.Json.JsonSerializer.Deserialize<T>(Snapshot);
			return snapshot != null;
		}

		public void SetSnapshotData<T>(T data)
		{
			if (data == null)
				Snapshot = null;

			Snapshot = System.Text.Json.JsonSerializer.Serialize(data);
		}
	}
}
