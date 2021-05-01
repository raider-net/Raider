using System;
using System.Collections.Generic;

namespace Raider.Metrics
{
	public class IncrementingEventCounterItem : IEventCounterItem
	{
		public int IdEventCounter { get; set; }
		public string? Name { get; set; }
		public string? DisplayName { get; set; }
		public string? DisplayRateTimeScale { get; set; }
		public double? Increment { get; set; }
		public float? IntervalSec { get; set; }
		public string? Metadata { get; set; }
		public string? Series { get; set; }
		public string? CounterType { get; set; }
		public string? DisplayUnits { get; set; }
		public Dictionary<string, object>? OtherValues { get; set; }

		public event Action<EventCounterData>? OnUpdate;

		double? IEventCounterItem.Value => Increment;

		public IncrementingEventCounterItem(int idEventCounter)
		{
			IdEventCounter = idEventCounter;
		}

		public IncrementingEventCounterItem(int idEventCounter, IDictionary<string, object> payload)
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
					if (kvp.Key.Equals(nameof(DisplayRateTimeScale), StringComparison.Ordinal))
						DisplayRateTimeScale = (string)kvp.Value;
					else if (kvp.Key.Equals(nameof(Increment), StringComparison.Ordinal))
						Increment = (double)kvp.Value;
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
					else if (kvp.Key.Equals(nameof(DisplayRateTimeScale), StringComparison.Ordinal))
						DisplayRateTimeScale = (string)kvp.Value;
					else if (kvp.Key.Equals(nameof(Increment), StringComparison.Ordinal))
						Increment = (double)kvp.Value;
					else if (kvp.Key.Equals(nameof(IntervalSec), StringComparison.Ordinal))
						IntervalSec = (float)kvp.Value;
					else if (kvp.Key.Equals(nameof(Metadata), StringComparison.Ordinal))
						Metadata = (string)kvp.Value;
					else if (kvp.Key.Equals(nameof(Series), StringComparison.Ordinal))
						Series = (string)kvp.Value;
					else if (kvp.Key.Equals(nameof(CounterType), StringComparison.Ordinal))
						CounterType = (string)kvp.Value;
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
				CounterType = Metrics.CounterType.Sum,
				IdEventCounter = IdEventCounter,
				Name = Name,
				Increment = Increment,
				IntervalSec = IntervalSec
			};
	}
}
