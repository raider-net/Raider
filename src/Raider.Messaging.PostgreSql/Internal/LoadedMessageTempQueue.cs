using Raider.Messaging.Messages;
using System;

namespace Raider.Messaging.PostgreSql
{
	internal class LoadedMessageTempQueue
	{
		public Guid IdMessageTempQueue { get; set; }
		public Guid IdMessage { get; set; }
		public DateTimeOffset? LastAccessUtc { get; set; }
		public MessageState State { get; set; }
		public int RetryCount { get; set; }
		public DateTimeOffset? DelayedToUtc { get; set; }
		public Guid OriginalConcurrencyToken { get; set; }
		public Guid NewConcurrencyToken { get; set; }

		public LoadedMessageTempQueue()
		{
			NewConcurrencyToken = Guid.NewGuid();
		}
	}
}
