using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.Config.Components
{
	public interface IComponent
	{
		IScenario Scenario{ get; }
		Guid IdComponent { get; }
		string Name { get; }
		Type CrlType { get; }
		string ResolvedCrlType { get; }
		string? Description { get; }
		int ThrottleDelayInMilliseconds { get; }
		int InactivityTimeoutInSeconds { get; }
		int ShutdownTimeoutInSeconds { get; }
		Guid? IdCurrentSession { get; }
		int IdComponentStatus { get; }
		DateTime? LastStartTimeUtc { get; }
		List<IComponentQueue> ComponentQueues { get; }
	}
}
