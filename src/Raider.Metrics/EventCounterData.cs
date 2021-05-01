using System;
using System.Collections.Generic;

namespace Raider.Metrics
{
	public struct EventCounterData
	{
		public CounterType? CounterType { get; set; }
		public int IdEventCounter { get; set; }
		public string? Name { get; set; }
		public double? Increment { get; set; }
		public double? Mean { get; set; }
		public double? StandardDeviation { get; set; }
		public int? Count { get; set; }
		public double? Min { get; set; }
		public double? Max { get; set; }
		public float? IntervalSec { get; set; }

		//public EventCounterData(
		//	CounterType? counterType,
		//	string? name,
		//	double? increment,
		//	double? mean,
		//	double? standardDeviation,
		//	int? count,
		//	double? min,
		//	double? max,
		//	float? intervalSec)
		//{
		//	CounterType = counterType;
		//	Name = name;
		//	Increment = increment;
		//	Mean = mean;
		//	StandardDeviation = standardDeviation;
		//	Count = count;
		//	Min = min;
		//	Max = max;
		//	IntervalSec = intervalSec;
		//}

		public EventCounterData(int idEventCounter, IDictionary<string, object> payload)
		{
			CounterType = null;
			IdEventCounter = idEventCounter;
			Name = null;
			Increment = null;
			Mean = null;
			StandardDeviation = null;
			Count = null;
			Min = null;
			Max = null;
			IntervalSec = null;

			foreach (var kvp in payload)
			{
				if (kvp.Key.Equals(nameof(Name), StringComparison.Ordinal))
					Name = (string)kvp.Value;
				else if (kvp.Key.Equals(nameof(Increment), StringComparison.Ordinal))
					Increment = (double)kvp.Value;
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
				else if (kvp.Key.Equals(nameof(CounterType), StringComparison.Ordinal))
				{
					var counterType = (string)kvp.Value;
					if (counterType.Equals("Mean", StringComparison.Ordinal))
						CounterType = Metrics.CounterType.Mean;
					else if (counterType.Equals("Sum", StringComparison.Ordinal))
						CounterType = Metrics.CounterType.Sum;
				}
			}
		}
	}
}
