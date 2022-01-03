using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Resolver;
using Raider.Validation;
using System;

namespace Raider.ServiceBus.Config
{
	public interface IBusOptions : IValidable
	{
		string Name { get; set; }
		ITypeResolver TypeResolver { get; set; }
		Func<IServiceProvider, ISerializer> MessageSerializer { get; set; }
		Func<IServiceProvider, IHostLogger> HostLogger { get; set; }

		/// <summary>
		/// The default behavior is to disable message serialization.
		/// When serialization is enabled, it creates an independent message instance (deep copy of the original message).
		/// </summary>
		bool EnableMessageSerialization { get; set; }
	}
}
