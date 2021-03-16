using System;

namespace Raider.Messaging
{
	internal class ServiceBusHost : IServiceBusHost
	{
		public string? ConnectionString { get; }
		public Guid IdServiceBusHost { get; }
		public Guid IdServiceBusHostRuntime { get; }
		public string Name { get; }
		public string? Description { get; set; }
		public DateTime StartedUtc { get; }
		public int? IdUser { get; }

		public ServiceBusHost(ServiceBusHostOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			ConnectionString = string.IsNullOrWhiteSpace(options.ConncetionString)
				? throw new ArgumentException($"{nameof(options.ConncetionString)} == null", nameof(options))
				: options.ConncetionString;

			IdServiceBusHost = options.IdServiceBusHost;
			IdServiceBusHostRuntime = Guid.NewGuid();

			Name = string.IsNullOrWhiteSpace(options.Name)
				? throw new ArgumentException($"{nameof(options.Name)} == null", nameof(options))
				: options.Name;

			StartedUtc = DateTime.UtcNow;
			IdUser = options.IdUser;
		}
	}
}
