using Raider.ServiceBus.Model;
using System;

namespace Raider.ServiceBus.Config.Components
{
	public interface IComponentQueue
	{
		IComponent Component { get; }
		Guid IdComponentQueue { get; }
		Type MessageType { get; }
		IMessageType MessageTypeModel { get; }
		string Name { get; }
		//Type CrlType { get; }
		string? Description { get; }
		bool IsFIFO { get; }
		int ProcessingTimeoutInSeconds { get; }
		int MaxRetryCount { get; }
	}
}
