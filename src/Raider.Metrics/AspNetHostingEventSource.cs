using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace Raider.Metrics
{
	public class AspNetHostingEventSource : EventListener, IEventListener
	{
		public const string EVENT_SOURCE_NAME = "Microsoft.AspNetCore.Hosting";

		private const string _currentRequests = "current-requests";
		private const string _failedRequests = "failed-requests";
		private const string _requestsPerSecond = "requests-per-second";
		private const string _totalRequests = "total-requests";

		private readonly Dictionary<string, string> _countersMap;

		public bool Enabled { get; private set; }

		public string EventSourceName => EVENT_SOURCE_NAME;
		public int EventCounterIntervalSec { get; }
		public EventLevel EventLevel { get; }
		public EventKeywords EventKeywords { get; }
		public List<string>? AllowedCounters { get; }

		public EventCounterItem CurrentRequests { get; }
		public EventCounterItem FailedRequests { get; }
		public IncrementingEventCounterItem RequestsPerSecond { get; }
		public EventCounterItem TotalRequests { get; }

		public Dictionary<string, Func<double?>> ActualValues { get; }
		public ConcurrentDictionary<string, bool> UnhandledPayloads { get; }

		public AspNetHostingEventSource(
			int eventCounterIntervalSec,
			EventLevel eventLevel = EventLevel.LogAlways,
			EventKeywords eventKeywords = EventKeywords.All,
			bool autoEnable = true)
		{
			EventCounterIntervalSec = eventCounterIntervalSec < 1
				? 1
				: eventCounterIntervalSec;
			EventLevel = eventLevel;
			EventKeywords = eventKeywords;

			_countersMap = new Dictionary<string, string>
			{
				[_currentRequests] = nameof(CurrentRequests),
				[_failedRequests] = nameof(FailedRequests),
				[_requestsPerSecond] = nameof(RequestsPerSecond),
				[_totalRequests] = nameof(TotalRequests)
			};

			CurrentRequests = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreHostingCurrentRequests);
			FailedRequests = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreHostingFailedRequests);
			RequestsPerSecond = new IncrementingEventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreHostingRequestsPerSecond);
			TotalRequests = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreHostingTotalRequests);

			ActualValues = new Dictionary<string, Func<double?>>
			{
				[nameof(CurrentRequests)] = () => CurrentRequests.Mean,
				[nameof(FailedRequests)] = () => FailedRequests.Mean,
				[nameof(RequestsPerSecond)] = () => RequestsPerSecond.Increment,
				[nameof(TotalRequests)] = () => TotalRequests.Mean
			};

			UnhandledPayloads = new ConcurrentDictionary<string, bool>();

			if (autoEnable)
				Enable();
		}

		public AspNetHostingEventSource(
			int eventCounterIntervalSec,
			IDictionary<string, Action<EventCounterData>?>? allowedCounters,
			EventLevel eventLevel = EventLevel.LogAlways,
			EventKeywords eventKeywords = EventKeywords.All,
			bool autoEnable = true)
			: this(eventCounterIntervalSec, eventLevel, eventKeywords, false)
		{
			if (allowedCounters != null)
			{
				AllowedCounters = allowedCounters.Keys.ToList();

				foreach (var kvp in allowedCounters)
				{
					if (kvp.Key == nameof(CurrentRequests))
						CurrentRequests.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(FailedRequests))
						FailedRequests.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(RequestsPerSecond))
						RequestsPerSecond.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(TotalRequests))
						TotalRequests.OnUpdate += kvp.Value;
				}
			}

			if (autoEnable)
				Enable();
		}

		public AspNetHostingEventSource(
			int eventCounterIntervalSec,
			Action<EventCounterData>? onUpdate,
			EventLevel eventLevel = EventLevel.LogAlways,
			EventKeywords eventKeywords = EventKeywords.All,
			bool autoEnable = true)
			: this(eventCounterIntervalSec, eventLevel, eventKeywords, false)
		{
			AddOnUpdateEvent(onUpdate);

			if (autoEnable)
				Enable();
		}

		public AspNetHostingEventSource(EventSourceOptions options, Action<EventCounterData> onUpdate)
			: this(
				options?.EventCounterIntervalSec ?? throw new ArgumentNullException(nameof(options)),
				options.EventLevel,
				options.EventKeywords,
				false)
		{
			if (!EVENT_SOURCE_NAME.Equals(options.EventSourceName, StringComparison.Ordinal))
				throw new InvalidOperationException($"Invalid {nameof(options)}.{nameof(options.EventSourceName)} Required {nameof(EVENT_SOURCE_NAME)} is '{EVENT_SOURCE_NAME}'");

			if (onUpdate == null)
				throw new ArgumentNullException(nameof(onUpdate));

			if (options.AllowedCounters != null)
			{
				AllowedCounters = options.AllowedCounters.ToList();

				foreach (var allowedCounter in AllowedCounters)
				{
					if (allowedCounter == nameof(CurrentRequests))
						CurrentRequests.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(FailedRequests))
						FailedRequests.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(RequestsPerSecond))
						RequestsPerSecond.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(TotalRequests))
						TotalRequests.OnUpdate += onUpdate;
				}
			}
			else
			{
				AddOnUpdateEvent(onUpdate);
			}

			if (options.AutoEnable)
				Enable();
		}

		public void AddOnUpdateEvent(Action<EventCounterData>? onUpdate)
		{
			if (onUpdate != null)
			{
				CurrentRequests.OnUpdate += onUpdate;
				FailedRequests.OnUpdate += onUpdate;
				RequestsPerSecond.OnUpdate += onUpdate;
				TotalRequests.OnUpdate += onUpdate;
			}
		}

		public void Enable()
		{
			Enabled = true;

			if (_created)
				return;

			foreach (var eventSource in EventSource.GetSources())
				OnEventSourceCreated(eventSource);
		}

		public void DisableWrite()
			=> Enabled = false;

		private bool _created;
		protected override void OnEventSourceCreated(EventSource source)
		{
			if (!Enabled || _created)
				return;

			if (!source.Name.Equals(EVENT_SOURCE_NAME, StringComparison.Ordinal))
				return;

			EnableEvents(source, EventLevel, EventKeywords, new Dictionary<string, string?> { { "EventCounterIntervalSec", EventCounterIntervalSec.ToString() } });
			_created = true;
		}

		protected override void OnEventWritten(EventWrittenEventArgs eventData)
		{
			if (!Enabled)
				return;

			if (eventData.EventSource.Name.Equals(EVENT_SOURCE_NAME, StringComparison.Ordinal))
			{
				if (eventData.Payload != null && 0 < eventData.Payload.Count && eventData.Payload[0] is IDictionary<string, object> payload)
					Update(payload);
			}
		}

		public void Update(IDictionary<string, object> payload)
		{
			if (payload == null)
				return;

			if (payload.TryGetValue("Name", out object? nameValue) && nameValue is string name)
			{
				if (AllowedCounters != null && !AllowedCounters.Contains(_countersMap[name], StringComparer.Ordinal))
					return;

				if (name.Equals(_currentRequests, StringComparison.Ordinal))
				{
					CurrentRequests.Update(payload);
				}
				else if (name.Equals(_failedRequests, StringComparison.Ordinal))
				{
					FailedRequests.Update(payload);
				}
				else if (name.Equals(_requestsPerSecond, StringComparison.Ordinal))
				{
					RequestsPerSecond.Update(payload);
				}
				else if (name.Equals(_totalRequests, StringComparison.Ordinal))
				{
					TotalRequests.Update(payload);
				}
				else
				{
					UnhandledPayloads.TryAdd(name, true);
				}
			}
		}
	}
}
