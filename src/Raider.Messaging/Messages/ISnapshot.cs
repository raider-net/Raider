using System;

namespace Raider.Messaging.Messages
{
	public interface ISnapshot
	{
		Guid IdSnapshot { get; }
		string? SnapshotIdentifier { get; }
		DateTimeOffset LastAccessUtc { get; }
		public Guid OriginalConcurrencyToken { get; }
		public Guid NewConcurrencyToken { get; }

		bool HasData { get; }
		object? GetData();
	}

	public interface ISnapshot<TData> : ISnapshot
		where TData : IMessageData
	{
		bool ISnapshot.HasData => true;

		object? ISnapshot.GetData()
			=> Data;

		TData? Data { get; }
	}
}
