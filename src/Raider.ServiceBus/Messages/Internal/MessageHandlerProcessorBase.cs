using System;

namespace Raider.ServiceBus.Messages.Internal
{
	internal abstract class MessageHandlerProcessorBase
	{
		protected abstract IMessageHandler CreateHandler(IServiceProvider serviceProvider);
	}
}
