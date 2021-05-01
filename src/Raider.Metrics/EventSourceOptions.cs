using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Raider.Metrics
{
	public class EventSourceOptions
	{
		public string EventSourceName { get; set; }
		public int EventCounterIntervalSec { get; set; } = 1;
		public EventLevel EventLevel { get; set; } = EventLevel.LogAlways;
		public EventKeywords EventKeywords { get; set; } = EventKeywords.All;
		public List<string>? AllowedCounters { get; set; }
		public IDictionary<string, int>? EventSourceAdapterAllowedCounters { get; set; }
		public IDictionary<string, Func<string, int>>? EventSourceAdapterAllowedCounterGetters { get; set; }
		public Func<string, int> EventSourceAdapterIdEventCounterGetter { get; set; }
		public bool AutoEnable { get; set; } = true;

		public EventSourceOptions(string eventSourceName)
		{
			EventSourceName = string.IsNullOrWhiteSpace(eventSourceName)
				? throw new ArgumentNullException(nameof(eventSourceName))
				: eventSourceName;
		}
	}
}
