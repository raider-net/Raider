using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.Events.Internal
{
	internal abstract class EventHandlerProcessorBase
	{
		protected abstract IEnumerable<IEventHandler> CreateHandlers(IServiceProvider serviceProvider);
	}
}
