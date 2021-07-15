using System;
using System.Collections.Generic;

namespace Raider.Messaging
{
	public class ServiceBusOptions
	{
		public string? ConncetionString { get; set; }
		public int ServiceHostStartMaxRetryCount { get; set; }
		public Guid IdServiceBusHost { get; set; }
		public string? Name { get; set; }
		public string? Description { get; set; }
		public int? IdUser { get; set; }
		public Dictionary<object, object> Properties { get; }

		public ServiceBusOptions()
		{
			Properties = new Dictionary<object, object>();
		}

		public void AddProperties(Dictionary<object, object> properties)
		{
			foreach (var prop in properties)
				Properties.Add(prop.Key, prop.Value);
		}
	}
}
