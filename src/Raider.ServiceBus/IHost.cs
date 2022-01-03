using System;

namespace Raider.ServiceBus
{
	public interface IHost
	{
		Guid IdHost { get; }
		string Name { get; }
		string? Description { get; }
		Guid IdHostType { get; }
		bool Disabled { get; }
		Guid CurrentRuntimeUniqueKey { get; }
		DateTime LastStartTimeUtc { get; }
		int IdHostStatus { get; }
		Guid SyncToken { get; }
	}
}
