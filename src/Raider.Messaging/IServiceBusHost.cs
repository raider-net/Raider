using System;
using System.Collections.Generic;

namespace Raider.Messaging
{
	public interface IServiceBusHost
	{
		string? ConnectionString { get; }
		Guid IdServiceBusHost { get; }
		IApplicationContext ApplicationContext { get; }
		string Name { get; }
		string? Description { get; }
		DateTime StartedUtc { get; }
		IReadOnlyDictionary<object, object> Properties { get; }
	}
}
