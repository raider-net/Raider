using System;
using System.Collections.Generic;

namespace Raider.Metrics
{
	public class EventCounterItem : IEventCounterItem
	{
		public int IdEventCounter { get; set; }
		public string? Name { get; set; }
		public string? DisplayName { get; set; }
		public double? Mean { get; set; }
		public double? StandardDeviation { get; set; }
		public int? Count { get; set; }
		public double? Min { get; set; }
		public double? Max { get; set; }
		public float? IntervalSec { get; set; }
		public string? Series { get; set; }
		public string? CounterType { get; set; }
		public string? Metadata { get; set; }
		public string? DisplayUnits { get; set; }
		public Dictionary<string, object>? OtherValues { get; set; }

		public event Action<EventCounterData>? OnUpdate;

		double? IEventCounterItem.Value => Mean;

		public EventCounterItem(int idEventCounter)
		{
			IdEventCounter = idEventCounter;
		}

		public EventCounterItem(int idEventCounter, IDictionary<string, object> payload)
			: this(idEventCounter)
			=> Update(payload, true);

		private bool _initialized;
		public void Update(IDictionary<string, object> payload, bool forece = false)
		{
			if (payload == null)
				return;

			if (!forece && _initialized)
			{
				foreach (var kvp in payload)
				{
					if (kvp.Key.Equals(nameof(Mean), StringComparison.Ordinal))
						Mean = (double)kvp.Value;
					else if (kvp.Key.Equals(nameof(StandardDeviation), StringComparison.Ordinal))
						StandardDeviation = (double)kvp.Value;
					else if (kvp.Key.Equals(nameof(Count), StringComparison.Ordinal))
						Count = (int)kvp.Value;
					else if (kvp.Key.Equals(nameof(Min), StringComparison.Ordinal))
						Min = (double)kvp.Value;
					else if (kvp.Key.Equals(nameof(Max), StringComparison.Ordinal))
						Max = (double)kvp.Value;
					else if (kvp.Key.Equals(nameof(IntervalSec), StringComparison.Ordinal))
						IntervalSec = (float)kvp.Value;
				}
			}
			else
			{
				foreach (var kvp in payload)
				{
					if (kvp.Key.Equals(nameof(Name), StringComparison.Ordinal))
						Name = (string)kvp.Value;
					else if (kvp.Key.Equals(nameof(DisplayName), StringComparison.Ordinal))
						DisplayName = (string)kvp.Value;
					else if (kvp.Key.Equals(nameof(Mean), StringComparison.Ordinal))
						Mean = (double)kvp.Value;
					else if (kvp.Key.Equals(nameof(StandardDeviation), StringComparison.Ordinal))
						StandardDeviation = (double)kvp.Value;
					else if (kvp.Key.Equals(nameof(Count), StringComparison.Ordinal))
						Count = (int)kvp.Value;
					else if (kvp.Key.Equals(nameof(Min), StringComparison.Ordinal))
						Min = (double)kvp.Value;
					else if (kvp.Key.Equals(nameof(Max), StringComparison.Ordinal))
						Max = (double)kvp.Value;
					else if (kvp.Key.Equals(nameof(IntervalSec), StringComparison.Ordinal))
						IntervalSec = (float)kvp.Value;
					else if (kvp.Key.Equals(nameof(Series), StringComparison.Ordinal))
						Series = (string)kvp.Value;
					else if (kvp.Key.Equals(nameof(CounterType), StringComparison.Ordinal))
						CounterType = (string)kvp.Value;
					else if (kvp.Key.Equals(nameof(Metadata), StringComparison.Ordinal))
						Metadata = (string)kvp.Value;
					else if (kvp.Key.Equals(nameof(DisplayUnits), StringComparison.Ordinal))
						DisplayUnits = (string)kvp.Value;
					else
					{
						if (OtherValues == null)
							OtherValues = new Dictionary<string, object>();

						OtherValues.Add(kvp.Key, kvp.Value);
					}
				}
			}

			var originalInit = _initialized;
			_initialized = true;

			OnUpdate?.Invoke(ToEventCounterData());
		}

		public EventCounterData ToEventCounterData()
			=> new()
			{
				CounterType = Metrics.CounterType.Mean,
				IdEventCounter = IdEventCounter,
				Name = Name,
				Mean = Mean,
				StandardDeviation = StandardDeviation,
				Count = Count,
				Min = Min,
				Max = Max,
				IntervalSec = IntervalSec
			};
	}
}
