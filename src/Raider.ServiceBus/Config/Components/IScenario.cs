using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.Config.Components
{
	public interface IScenario
	{
		Guid IdScenario { get; }
		string Name { get; }
		string? Description { get; }
		bool Disabled { get; }
		DateTime? LastStartTimeUtc { get; }
		List<IComponent> InboundComponents { get; }
		List<IComponent> BusinessProcesses { get; }
		List<IComponent> OutboundComponents { get; }
	}
}
