using System;

namespace Raider.ServiceBus.Events.Storage.Model
{
	public class SavedEvent<TEvent>
		where TEvent : IEvent
	{
		public TEvent Event { get; set; }
		public Guid IdSavedEvent { get; set; }
	}
}
