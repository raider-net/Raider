using Raider.Messaging.Messages;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Raider.Messaging.PostgreSql
{
	internal class LoadedSubscriberMessage<TData> : ISubscriberMessage<TData>, IMessage<TData>, ISubscriberMessage, IMessage
		where TData : IMessageData
	{
		public Guid IdMessage { get; set; }
		public Guid IdPublisherInstance { get; set; }
		public Guid? IdPreviousMessage { get; set; }
		public DateTimeOffset CreatedUtc { get; set; }
		public DateTimeOffset? ValidToUtc { get; set; }
		public bool IsRecovery { get; set; }
		public TData? Data { get; set; }


		public Guid IdSubscriberMessage { get; set; }
		public int IdSubscriber { get; set; }
		public DateTimeOffset? LastAccessUtc { get; set; }
		public MessageState State { get; set; }
		public string? Snapshot { get; set; }
		public int RetryCount { get; set; }
		public DateTimeOffset? DelayedToUtc { get; set; }
		public Guid OriginalConcurrencyToken { get; set; }
		public Guid NewConcurrencyToken { get; set; }

		public LoadedSubscriberMessage()
		{
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
