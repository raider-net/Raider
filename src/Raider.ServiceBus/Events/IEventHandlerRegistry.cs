using System;

namespace Raider.ServiceBus.Events
{
	public interface IEventHandlerRegistry
	{
		bool TryRegisterHandlerAndInterceptor(Type type);
	}
}
