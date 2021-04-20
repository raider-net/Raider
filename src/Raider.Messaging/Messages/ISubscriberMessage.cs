using System;
using System.Diagnostics.CodeAnalysis;

namespace Raider.Messaging.Messages
{
	public interface ISubscriberMessage : IMessage
	{
		Guid IdSubscriberMessage { get; }
		DateTimeOffset? LastAccessUtc { get; }
		int IdSubscriber { get; }
		MessageState State { get; }
		string? Snapshot { get; set; }
		int RetryCount { get; }
		DateTimeOffset? DelayedToUtc { get; }
		Guid OriginalConcurrencyToken { get; }
		Guid NewConcurrencyToken { get; }

		bool TryGetSnapshotData<T>([NotNullWhen(true)] out T? snapshot);
		void SetSnapshotData<T>(T data);
	}

	public interface ISubscriberMessage<TData> : IMessage<TData>, ISubscriberMessage
		where TData : IMessageData
	{
	}
}
