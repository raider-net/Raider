using System;

namespace Raider.Messaging.Messages
{
	internal class Message<TData> : IMessage<TData>
		where TData : IMessageData
	{
		public Guid IdMessage { get; set; }
		public Guid? IdPreviousMessage { get; set; }
		public Guid BusinessId { get; set; }
		public int IdScenario { get; set; }
		public int IdPublisher { get; set; }
		public DateTimeOffset CreatedUtc { get; set; }
		public bool IsRecovery { get; set; }
		public TData? Data { get; set; }
	}
}
