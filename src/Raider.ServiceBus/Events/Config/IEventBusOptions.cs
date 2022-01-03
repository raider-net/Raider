using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Config;
using Raider.Validation;
using System;

namespace Raider.ServiceBus.Events.Config
{
	public interface IEventBusOptions : IBusOptions, IValidable
	{
		Type EventHandlerContextType { get; set; }
		Func<IServiceProvider, EventHandlerContext> EventHandlerContextFactory { get; set; }
		Func<IServiceProvider, IHandlerMessageLogger> EventLogger { get; set; }
	}
}
