using Raider.ServiceBus.Model;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.Events
{
	public interface IEventTypeRegistry
	{
		IEnumerable<IMessageType>? GetAllEventTypes();

		IMessageType? GetEventType(Type type);

		Type? GetType(IMessageType type);
	}
}
