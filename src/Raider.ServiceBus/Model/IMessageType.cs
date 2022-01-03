using System;

namespace Raider.ServiceBus.Model
{
	public interface IMessageType : Raider.Serializer.IDictionaryObject
	{
		Guid IdMessageType { get; }
		string Name { get; }
		string? Description { get; }
		int IdMessageMetaType { get; }
		string CrlType { get; }
	}
}
