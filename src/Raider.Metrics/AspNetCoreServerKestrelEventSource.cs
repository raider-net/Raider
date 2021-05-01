using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace Raider.Metrics
{
	public class AspNetCoreServerKestrelEventSource : EventListener, IEventListener
	{
		public const string EVENT_SOURCE_NAME = "Microsoft-AspNetCore-Server-Kestrel";

		private const string _connectionsPerSecond = "connections-per-second";
		private const string _tlsHandshakesPerSecond = "tls-handshakes-per-second";
		private const string _totalConnections = "total-connections";
		private const string _totalTlsHandshakes = "total-tls-handshakes";
		private const string _currentTlsHandshakes = "current-tls-handshakes";
		private const string _failedTlsHandshakes = "failed-tls-handshakes";
		private const string _currentConnections = "current-connections";
		private const string _connectionQueueLength = "connection-queue-length";
		private const string _requestQueueLength = "request-queue-length";
		private const string _currentUpgradedRequests = "current-upgraded-requests";

		private readonly Dictionary<string, string> _countersMap;

		public bool Enabled { get; private set; }

		public string EventSourceName => EVENT_SOURCE_NAME;
		public int EventCounterIntervalSec { get; }
		public EventLevel EventLevel { get; }
		public EventKeywords EventKeywords { get; }
		public List<string>? AllowedCounters { get; }

		public IncrementingEventCounterItem ConnectionsPerSecond { get; }
		public IncrementingEventCounterItem TlsHandshakesPerSecond { get; }
		public EventCounterItem TotalConnections { get; }
		public EventCounterItem TotalTlsHandshakes { get; }
		public EventCounterItem CurrentTlsHandshakes { get; }
		public EventCounterItem FailedTlsHandshakes { get; }
		public EventCounterItem CurrentConnections { get; }
		public EventCounterItem ConnectionQueueLength { get; }
		public EventCounterItem RequestQueueLength { get; }
		public EventCounterItem CurrentUpgradedRequests { get; }

		public Dictionary<string, Func<double?>> ActualValues { get; }
		public ConcurrentDictionary<string, bool> UnhandledPayloads { get; }

		public AspNetCoreServerKestrelEventSource(
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
				[_connectionsPerSecond] = nameof(ConnectionsPerSecond),
				[_tlsHandshakesPerSecond] = nameof(TlsHandshakesPerSecond),
				[_totalConnections] = nameof(TotalConnections),
				[_totalTlsHandshakes] = nameof(TotalTlsHandshakes),
				[_currentTlsHandshakes] = nameof(CurrentTlsHandshakes),
				[_failedTlsHandshakes] = nameof(FailedTlsHandshakes),
				[_currentConnections] = nameof(CurrentConnections),
				[_connectionQueueLength] = nameof(ConnectionQueueLength),
				[_requestQueueLength] = nameof(RequestQueueLength),
				[_currentUpgradedRequests] = nameof(CurrentUpgradedRequests),
			};

			ConnectionsPerSecond = new IncrementingEventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreServerKestrelConnectionsPerSecond);
			TlsHandshakesPerSecond = new IncrementingEventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreServerKestrelTlsHandshakesPerSecond);
			TotalConnections = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreServerKestrelTotalConnections);
			TotalTlsHandshakes = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreServerKestrelTotalTlsHandshakes);
			CurrentTlsHandshakes = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreServerKestrelCurrentTlsHandshakes);
			FailedTlsHandshakes = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreServerKestrelFailedTlsHandshakes);
			CurrentConnections = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreServerKestrelCurrentConnections);
			ConnectionQueueLength = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreServerKestrelConnectionQueueLength);
			RequestQueueLength = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreServerKestrelRequestQueueLength);
			CurrentUpgradedRequests = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreServerKestrelCurrentUpgradedRequests);

			ActualValues = new Dictionary<string, Func<double?>>
			{
				[nameof(ConnectionsPerSecond)] = () => ConnectionsPerSecond.Increment,
				[nameof(TlsHandshakesPerSecond)] = () => TlsHandshakesPerSecond.Increment,
				[nameof(TotalConnections)] = () => TotalConnections.Mean,
				[nameof(TotalTlsHandshakes)] = () => TotalTlsHandshakes.Mean,
				[nameof(CurrentTlsHandshakes)] = () => CurrentTlsHandshakes.Mean,
				[nameof(FailedTlsHandshakes)] = () => FailedTlsHandshakes.Mean,
				[nameof(CurrentConnections)] = () => CurrentConnections.Mean,
				[nameof(ConnectionQueueLength)] = () => ConnectionQueueLength.Mean,
				[nameof(RequestQueueLength)] = () => RequestQueueLength.Mean,
				[nameof(CurrentUpgradedRequests)] = () => CurrentUpgradedRequests.Mean,
			};

			UnhandledPayloads = new ConcurrentDictionary<string, bool>();

			if (autoEnable)
				Enable();
		}

		public AspNetCoreServerKestrelEventSource(
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
					if (kvp.Key == nameof(ConnectionsPerSecond))
						ConnectionsPerSecond.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(TlsHandshakesPerSecond))
						TlsHandshakesPerSecond.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(TotalConnections))
						TotalConnections.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(TotalTlsHandshakes))
						TotalTlsHandshakes.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(CurrentTlsHandshakes))
						CurrentTlsHandshakes.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(FailedTlsHandshakes))
						FailedTlsHandshakes.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(CurrentConnections))
						CurrentConnections.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(ConnectionQueueLength))
						ConnectionQueueLength.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(RequestQueueLength))
						RequestQueueLength.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(CurrentUpgradedRequests))
						CurrentUpgradedRequests.OnUpdate += kvp.Value;
				}
			}

			if (autoEnable)
				Enable();
		}

		public AspNetCoreServerKestrelEventSource(
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

		public AspNetCoreServerKestrelEventSource(EventSourceOptions options, Action<EventCounterData> onUpdate)
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
					if (allowedCounter == nameof(ConnectionsPerSecond))
						ConnectionsPerSecond.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(TlsHandshakesPerSecond))
						TlsHandshakesPerSecond.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(TotalConnections))
						TotalConnections.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(TotalTlsHandshakes))
						TotalTlsHandshakes.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(CurrentTlsHandshakes))
						CurrentTlsHandshakes.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(FailedTlsHandshakes))
						FailedTlsHandshakes.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(CurrentConnections))
						CurrentConnections.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(ConnectionQueueLength))
						ConnectionQueueLength.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(RequestQueueLength))
						RequestQueueLength.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(CurrentUpgradedRequests))
						CurrentUpgradedRequests.OnUpdate += onUpdate;
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
				ConnectionsPerSecond.OnUpdate += onUpdate;
				TlsHandshakesPerSecond.OnUpdate += onUpdate;
				TotalConnections.OnUpdate += onUpdate;
				TotalTlsHandshakes.OnUpdate += onUpdate;
				CurrentTlsHandshakes.OnUpdate += onUpdate;
				FailedTlsHandshakes.OnUpdate += onUpdate;
				CurrentConnections.OnUpdate += onUpdate;
				ConnectionQueueLength.OnUpdate += onUpdate;
				RequestQueueLength.OnUpdate += onUpdate;
				CurrentUpgradedRequests.OnUpdate += onUpdate;
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

				if (name.Equals(_connectionsPerSecond, StringComparison.Ordinal))
				{
					ConnectionsPerSecond.Update(payload);
				}
				else if (name.Equals(_tlsHandshakesPerSecond, StringComparison.Ordinal))
				{
					TlsHandshakesPerSecond.Update(payload);
				}
				else if (name.Equals(_totalConnections, StringComparison.Ordinal))
				{
					TotalConnections.Update(payload);
				}
				else if (name.Equals(_totalTlsHandshakes, StringComparison.Ordinal))
				{
					TotalTlsHandshakes.Update(payload);
				}
				else if (name.Equals(_currentTlsHandshakes, StringComparison.Ordinal))
				{
					CurrentTlsHandshakes.Update(payload);
				}
				else if (name.Equals(_failedTlsHandshakes, StringComparison.Ordinal))
				{
					FailedTlsHandshakes.Update(payload);
				}
				else if (name.Equals(_currentConnections, StringComparison.Ordinal))
				{
					CurrentConnections.Update(payload);
				}
				else if (name.Equals(_connectionQueueLength, StringComparison.Ordinal))
				{
					ConnectionQueueLength.Update(payload);
				}
				else if (name.Equals(_requestQueueLength, StringComparison.Ordinal))
				{
					RequestQueueLength.Update(payload);
				}
				else if (name.Equals(_currentUpgradedRequests, StringComparison.Ordinal))
				{
					CurrentUpgradedRequests.Update(payload);
				}
				else
				{
					UnhandledPayloads.TryAdd(name, true);
				}
			}
		}
	}
}
