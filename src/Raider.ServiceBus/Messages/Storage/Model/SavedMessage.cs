using System;

namespace Raider.ServiceBus.Messages.Storage.Model
{
	public class SavedMessage<TMessage>
		where TMessage : IBaseRequestMessage
	{
		public TMessage Message { get; set; }
		public Guid IdSavedMessage { get; set; }
	}
}
