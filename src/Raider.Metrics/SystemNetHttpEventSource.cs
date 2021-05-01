using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace Raider.Metrics
{
	public class SystemNetHttpEventSource : EventListener, IEventListener
	{
		public const string EVENT_SOURCE_NAME = "System.Net.Http";

		private const string _requestsStartedRate = "requests-started-rate";
		private const string _requestsFailedRate = "requests-failed-rate";
		private const string _requestsStarted = "requests-started";
		private const string _requestsFailed = "requests-failed";
		private const string _currentRequests = "current-requests";
		private const string _http11ConnectionsCurrentTotal = "http11-connections-current-total";
		private const string _http20ConnectionsCurrentTotal = "http20-connections-current-total";
		private const string _http11RequestsQueueDuration = "http11-requests-queue-duration";
		private const string _http20RequestsQueueDuration = "http20-requests-queue-duration";

		private readonly Dictionary<string, string> _countersMap;

		public bool Enabled { get; private set; }

		public string EventSourceName => EVENT_SOURCE_NAME;
		public int EventCounterIntervalSec { get; }
		public EventLevel EventLevel { get; }
		public EventKeywords EventKeywords { get; }
		public List<string>? AllowedCounters { get; }

		public IncrementingEventCounterItem RequestsStartedRate { get; }
		public IncrementingEventCounterItem RequestsFailedRate { get; }
		public EventCounterItem RequestsStarted { get; }
		public EventCounterItem RequestsFailed { get; }
		public EventCounterItem CurrentRequests { get; }
		public EventCounterItem Http11ConnectionsCurrentTotal { get; }
		public EventCounterItem Http20ConnectionsCurrentTotal { get; }
		public EventCounterItem Http11RequestsQueueDuration { get; }
		public EventCounterItem Http20RequestsQueueDuration { get; }

		public Dictionary<string, Func<double?>> ActualValues { get; }
		public ConcurrentDictionary<string, bool> UnhandledPayloads { get; }

		public SystemNetHttpEventSource(
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
				[_requestsStartedRate] = nameof(RequestsStartedRate),
				[_requestsFailedRate] = nameof(RequestsFailedRate),
				[_requestsStarted] = nameof(RequestsStarted),
				[_requestsFailed] = nameof(RequestsFailed),
				[_currentRequests] = nameof(CurrentRequests),
				[_http11ConnectionsCurrentTotal] = nameof(Http11ConnectionsCurrentTotal),
				[_http20ConnectionsCurrentTotal] = nameof(Http20ConnectionsCurrentTotal),
				[_http11RequestsQueueDuration] = nameof(Http11RequestsQueueDuration),
				[_http20RequestsQueueDuration] = nameof(Http20RequestsQueueDuration),
			};

			RequestsStartedRate = new IncrementingEventCounterItem((int)EventCounterEnum.SystemNetHttpRequestsStartedRate);
			RequestsFailedRate = new IncrementingEventCounterItem((int)EventCounterEnum.SystemNetHttpRequestsFailedRate);
			RequestsStarted = new EventCounterItem((int)EventCounterEnum.SystemNetHttpRequestsStarted);
			RequestsFailed = new EventCounterItem((int)EventCounterEnum.SystemNetHttpRequestsFailed);
			CurrentRequests = new EventCounterItem((int)EventCounterEnum.SystemNetHttpCurrentRequests);
			Http11ConnectionsCurrentTotal = new EventCounterItem((int)EventCounterEnum.SystemNetHttpHttp11ConnectionsCurrentTotal);
			Http20ConnectionsCurrentTotal = new EventCounterItem((int)EventCounterEnum.SystemNetHttpHttp20ConnectionsCurrentTotal);
			Http11RequestsQueueDuration = new EventCounterItem((int)EventCounterEnum.SystemNetHttpHttp11RequestsQueueDuration);
			Http20RequestsQueueDuration = new EventCounterItem((int)EventCounterEnum.SystemNetHttpHttp20RequestsQueueDuration);

			ActualValues = new Dictionary<string, Func<double?>>
			{
				[nameof(RequestsStartedRate)] = () => RequestsStartedRate.Increment,
				[nameof(RequestsFailedRate)] = () => RequestsFailedRate.Increment,
				[nameof(RequestsStarted)] = () => RequestsStarted.Mean,
				[nameof(RequestsFailed)] = () => RequestsFailed.Mean,
				[nameof(CurrentRequests)] = () => CurrentRequests.Mean,
				[nameof(Http11ConnectionsCurrentTotal)] = () => Http11ConnectionsCurrentTotal.Mean,
				[nameof(Http20ConnectionsCurrentTotal)] = () => Http20ConnectionsCurrentTotal.Mean,
				[nameof(Http11RequestsQueueDuration)] = () => Http11RequestsQueueDuration.Mean,
				[nameof(Http20RequestsQueueDuration)] = () => Http20RequestsQueueDuration.Mean,
			};

			UnhandledPayloads = new ConcurrentDictionary<string, bool>();

			if (autoEnable)
				Enable();
		}

		public SystemNetHttpEventSource(
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
					if (kvp.Key == nameof(RequestsStartedRate))
						RequestsStartedRate.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(RequestsFailedRate))
						RequestsFailedRate.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(RequestsStarted))
						RequestsStarted.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(RequestsFailed))
						RequestsFailed.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(CurrentRequests))
						CurrentRequests.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(Http11ConnectionsCurrentTotal))
						Http11ConnectionsCurrentTotal.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(Http20ConnectionsCurrentTotal))
						Http20ConnectionsCurrentTotal.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(Http11RequestsQueueDuration))
						Http11RequestsQueueDuration.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(Http20RequestsQueueDuration))
						Http20RequestsQueueDuration.OnUpdate += kvp.Value;
				}
			}

			if (autoEnable)
				Enable();
		}

		public SystemNetHttpEventSource(
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

		public SystemNetHttpEventSource(EventSourceOptions options, Action<EventCounterData> onUpdate)
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
					if (allowedCounter == nameof(RequestsStartedRate))
						RequestsStartedRate.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(RequestsFailedRate))
						RequestsFailedRate.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(RequestsStarted))
						RequestsStarted.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(RequestsFailed))
						RequestsFailed.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(CurrentRequests))
						CurrentRequests.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(Http11ConnectionsCurrentTotal))
						Http11ConnectionsCurrentTotal.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(Http20ConnectionsCurrentTotal))
						Http20ConnectionsCurrentTotal.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(Http11RequestsQueueDuration))
						Http11RequestsQueueDuration.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(Http20RequestsQueueDuration))
						Http20RequestsQueueDuration.OnUpdate += onUpdate;
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
				RequestsStartedRate.OnUpdate += onUpdate;
				RequestsFailedRate.OnUpdate += onUpdate;
				RequestsStarted.OnUpdate += onUpdate;
				RequestsFailed.OnUpdate += onUpdate;
				CurrentRequests.OnUpdate += onUpdate;
				Http11ConnectionsCurrentTotal.OnUpdate += onUpdate;
				Http20ConnectionsCurrentTotal.OnUpdate += onUpdate;
				Http11RequestsQueueDuration.OnUpdate += onUpdate;
				Http20RequestsQueueDuration.OnUpdate += onUpdate;
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

				if (name.Equals(_requestsStartedRate, StringComparison.Ordinal))
				{
					RequestsStartedRate.Update(payload);
				}
				else if (name.Equals(_requestsFailedRate, StringComparison.Ordinal))
				{
					RequestsFailedRate.Update(payload);
				}
				else if (name.Equals(_requestsStarted, StringComparison.Ordinal))
				{
					RequestsStarted.Update(payload);
				}
				else if (name.Equals(_requestsFailed, StringComparison.Ordinal))
				{
					RequestsFailed.Update(payload);
				}
				else if (name.Equals(_currentRequests, StringComparison.Ordinal))
				{
					CurrentRequests.Update(payload);
				}
				else if (name.Equals(_http11ConnectionsCurrentTotal, StringComparison.Ordinal))
				{
					Http11ConnectionsCurrentTotal.Update(payload);
				}
				else if (name.Equals(_http20ConnectionsCurrentTotal, StringComparison.Ordinal))
				{
					Http20ConnectionsCurrentTotal.Update(payload);
				}
				else if (name.Equals(_http11RequestsQueueDuration, StringComparison.Ordinal))
				{
					Http11RequestsQueueDuration.Update(payload);
				}
				else if (name.Equals(_http20RequestsQueueDuration, StringComparison.Ordinal))
				{
					Http20RequestsQueueDuration.Update(payload);
				}
				else
				{
					UnhandledPayloads.TryAdd(name, true);
				}
			}
		}
	}
}
