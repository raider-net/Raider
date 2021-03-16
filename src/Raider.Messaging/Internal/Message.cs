using System;

namespace Raider.Messaging.Messages
{
	internal class Message<TData> : IMessage<TData>
		where TData : IMessageData
	{
		public Guid IdMessage { get; set; }
		public Guid? IdPreviousMessage { get; set; }
		public Guid IdPublisherInstance { get; set; }
		public DateTimeOffset CreatedUtc { get; set; }
		public DateTimeOffset? ValidToUtc { get; set; }
		public bool IsRecovery { get; set; }
		public TData? Data { get; set; }
	}
}
