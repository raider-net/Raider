﻿using System;

#nullable disable

namespace Raider.Messaging
{
	public class ServiceBusConfig
	{
		public ServiceBusOptions ServiceBusOptions { get; set; }
		public ServiceBusMode Mode { get; set; }
		public bool AllowJobs { get; set; }
		public Action<IServiceBusRegister> RegisterConfiguration { get; set; }

	}
}
