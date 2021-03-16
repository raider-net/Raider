using System;

#nullable disable

namespace Raider.Messaging
{
	public class ServiceBusConfig
	{
		public ServiceBusHostOptions ServiceBusHostOptions { get; set; }
		public ServiceBusMode Mode { get; set; }
		public bool AllowJobs { get; set; }
		public Action<IServiceBusRegister> RegisterConfiguration { get; set; }
		public bool ThrowIfNotSubscribedMessageFound { get; set; }
		public bool ThrowIfNotPublishedMessageFound { get; set; }

	}
}
