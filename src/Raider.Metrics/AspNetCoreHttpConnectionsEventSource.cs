using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace Raider.Metrics
{
	public class AspNetCoreHttpConnectionsEventSource : EventListener, IEventListener
	{
		public const string EVENT_SOURCE_NAME = "Microsoft.AspNetCore.Http.Connections";

		private const string _connectionsDuration = "connections-duration";
		private const string _currentConnections = "current-connections";
		private const string _connectionsStarted = "connections-started";
		private const string _connectionsStopped = "connections-stopped";
		private const string _connectionsTimedOut = "connections-timed-out";

		private readonly Dictionary<string, string> _countersMap;

		public bool Enabled { get; private set; }

		public string EventSourceName => EVENT_SOURCE_NAME;
		public int EventCounterIntervalSec { get; }
		public EventLevel EventLevel { get; }
		public EventKeywords EventKeywords { get; }
		public List<string>? AllowedCounters { get; }

		public EventCounterItem ConnectionsDuration { get; }
		public EventCounterItem CurrentConnections { get; }
		public EventCounterItem ConnectionsStarted { get; }
		public EventCounterItem ConnectionsStopped { get; }
		public EventCounterItem ConnectionsTimedOut { get; }

		public Dictionary<string, Func<double?>> ActualValues { get; }
		public ConcurrentDictionary<string, bool> UnhandledPayloads { get; }

		private AspNetCoreHttpConnectionsEventSource(
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
				[_connectionsDuration] = nameof(ConnectionsDuration),
				[_currentConnections] = nameof(CurrentConnections),
				[_connectionsStarted] = nameof(ConnectionsStarted),
				[_connectionsStopped] = nameof(ConnectionsStopped),
				[_connectionsTimedOut] = nameof(ConnectionsTimedOut)
			};

			ConnectionsDuration = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreHttpConnectionsConnectionsDuration);
			CurrentConnections = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreHttpConnectionsCurrentConnections);
			ConnectionsStarted = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreHttpConnectionsConnectionsStarted);
			ConnectionsStopped = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreHttpConnectionsConnectionsStopped);
			ConnectionsTimedOut = new EventCounterItem((int)EventCounterEnum.MicrosoftAspNetCoreHttpConnectionsConnectionsTimedOut);

			ActualValues = new Dictionary<string, Func<double?>>
			{
				[nameof(ConnectionsDuration)] = () => ConnectionsDuration.Mean,
				[nameof(CurrentConnections)] = () => CurrentConnections.Mean,
				[nameof(ConnectionsStarted)] = () => ConnectionsStarted.Mean,
				[nameof(ConnectionsStopped)] = () => ConnectionsStopped.Mean,
				[nameof(ConnectionsTimedOut)] = () => ConnectionsTimedOut.Mean
			};

			UnhandledPayloads = new ConcurrentDictionary<string, bool>();

			if (autoEnable)
				Enable();
		}

		public AspNetCoreHttpConnectionsEventSource(
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
					if (kvp.Key == nameof(ConnectionsDuration))
						ConnectionsDuration.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(CurrentConnections))
						CurrentConnections.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(ConnectionsStarted))
						ConnectionsStarted.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(ConnectionsStopped))
						ConnectionsStopped.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(ConnectionsTimedOut))
						ConnectionsTimedOut.OnUpdate += kvp.Value;
				}
			}

			if (autoEnable)
				Enable();
		}

		public AspNetCoreHttpConnectionsEventSource(
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

		public AspNetCoreHttpConnectionsEventSource(EventSourceOptions options, Action<EventCounterData> onUpdate)
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
					if (allowedCounter == nameof(ConnectionsDuration))
						ConnectionsDuration.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(CurrentConnections))
						CurrentConnections.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(ConnectionsStarted))
						ConnectionsStarted.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(ConnectionsStopped))
						ConnectionsStopped.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(ConnectionsTimedOut))
						ConnectionsTimedOut.OnUpdate += onUpdate;
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
				ConnectionsDuration.OnUpdate += onUpdate;
				CurrentConnections.OnUpdate += onUpdate;
				ConnectionsStarted.OnUpdate += onUpdate;
				ConnectionsStopped.OnUpdate += onUpdate;
				ConnectionsTimedOut.OnUpdate += onUpdate;
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

				if (name.Equals(_connectionsDuration, StringComparison.Ordinal))
				{
					ConnectionsDuration.Update(payload);
				}
				else if (name.Equals(_currentConnections, StringComparison.Ordinal))
				{
					CurrentConnections.Update(payload);
				}
				else if (name.Equals(_connectionsStarted, StringComparison.Ordinal))
				{
					ConnectionsStarted.Update(payload);
				}
				else if (name.Equals(_connectionsStopped, StringComparison.Ordinal))
				{
					ConnectionsStopped.Update(payload);
				}
				else if (name.Equals(_connectionsTimedOut, StringComparison.Ordinal))
				{
					ConnectionsTimedOut.Update(payload);
				}
				else
				{
					UnhandledPayloads.TryAdd(name, true);
				}
			}
		}
	}
}
