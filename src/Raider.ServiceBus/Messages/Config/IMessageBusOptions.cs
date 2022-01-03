using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Config;
using Raider.Validation;
using System;

namespace Raider.ServiceBus.Messages.Config
{
	public interface IMessageBusOptions : IBusOptions, IValidable
	{
		Type MessageHandlerContextType { get; set; }
		Func<IServiceProvider, MessageHandlerContext> MessageHandlerContextFactory { get; set; }
		Func<IServiceProvider, IHandlerMessageLogger> MessageLogger { get; set; }
	}
}
