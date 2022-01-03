using Raider.ServiceBus.Model;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.Messages
{
	public interface IMessageTypeRegistry
	{
		IEnumerable<IMessageType>? GetAllMessageTypes();

		IMessageType? GetMessageType(Type type);

		Type? GetType(IMessageType type);
	}
}
