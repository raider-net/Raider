using Raider.Identity;
using System;

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
	}
}
