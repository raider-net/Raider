using System;

namespace Raider.Messaging
{
	public class ServiceBusHostOptions
	{
		public string? ConncetionString { get; set; }
		public int ServiceHostStartMaxRetryCount { get; set; }
		public Guid IdServiceBusHost { get; set; }
		public string? Name { get; set; }
		public string? Description { get; set; }
		public int? IdUser { get; set; }
	}
}
