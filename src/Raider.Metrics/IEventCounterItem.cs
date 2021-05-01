using System;
using System.Collections.Generic;

namespace Raider.Metrics
{
	public interface IEventCounterItem
	{
		int IdEventCounter { get; }
		string? Name { get; }
		string? DisplayName { get; }
		double? Value { get; }
		float? IntervalSec { get; }
		string? Series { get; }
		string? CounterType { get; }
		string? Metadata { get; }
		string? DisplayUnits { get; }
		Dictionary<string, object>? OtherValues { get; }

		event Action<EventCounterData>? OnUpdate;

		void Update(IDictionary<string, object> payload, bool forece = false);

		EventCounterData ToEventCounterData();
	}
}
