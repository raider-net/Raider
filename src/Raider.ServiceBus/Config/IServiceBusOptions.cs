using Raider.ServiceBus.Config.Fluent;
using Raider.Validation;
using System.Collections.Generic;

namespace Raider.ServiceBus.Config
{
	public interface IServiceBusOptions : IBusOptions, IValidable
	{
		List<ScenarioBuilder> Scenarios { get; }
	}
}
