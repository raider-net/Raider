using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace Raider.Metrics
{
	public class EventSourceAdapter : EventListener, IEventListener
	{
		private readonly Action<EventCounterItem>? _onEventCounter;
		private readonly Action<IncrementingEventCounterItem>? _onIncrementingEventCounter;

		private readonly Func<string, int>? _idEventCounterGetter;

		private Action<EventCounterData>? _onUpdate;

		public bool Enabled { get; private set; }

		public string EventSourceName { get; }
		public int EventCounterIntervalSec { get; }
		public EventLevel EventLevel { get; }
		public EventKeywords EventKeywords { get; }
		public IDictionary<string, (int IdEventCounter, Action<EventCounterData> OnUpdate)>? AllowedCounters { get; } //IDictionary<name, (idEventCounter, onUpdate)>
		public IDictionary<string, (Func<string, int> IdEventCounterGetter, Action<EventCounterData> OnUpdate)>? AllowedCountersWithFuncId { get; } //IDictionary<name, (Func<name, idEventCounter>, onUpdate)>

		List<string>? IEventListener.AllowedCounters => AllowedCounters?.Keys.ToList();

		public Dictionary<string, Func<double?>> ActualValues => new();

		public ConcurrentDictionary<string, bool> UnhandledPayloads => new();

		private EventSourceAdapter(
			string eventSourceName,
			int eventCounterIntervalSec,
			EventLevel eventLevel = EventLevel.LogAlways,
			EventKeywords eventKeywords = EventKeywords.All,
			bool autoEnable = true)
		{
			EventSourceName = string.IsNullOrWhiteSpace(eventSourceName)
				? throw new ArgumentNullException(nameof(eventSourceName))
				: eventSourceName;

			EventCounterIntervalSec = eventCounterIntervalSec < 1
				? 1
				: eventCounterIntervalSec;

			EventLevel = eventLevel;
			EventKeywords = eventKeywords;

			if (autoEnable)
				Enable();
		}

		public EventSourceAdapter(
			string eventSourceName,
			int eventCounterIntervalSec,
			IDictionary<string, (int IdEventCounter, Action<EventCounterData> OnUpdate)> allowedCounters,
			EventLevel eventLevel = EventLevel.LogAlways,
			EventKeywords eventKeywords = EventKeywords.All,
			bool autoEnable = true)
			: this(eventSourceName, eventCounterIntervalSec, eventLevel, eventKeywords, false)
		{
			if (allowedCounters == null)
				throw new ArgumentNullException(nameof(allowedCounters));

			AllowedCounters = allowedCounters.ToDictionary(x => x.Key, x => x.Value);
			if (AllowedCounters.Count == 0 || AllowedCounters.Any(x => x.Value.OnUpdate == null))
				throw new InvalidOperationException($"Invalid {nameof(allowedCounters)} - missing Action<{nameof(EventCounterData)}>>");

			if (autoEnable)
				Enable();
		}

		public EventSourceAdapter(
			string eventSourceName,
			int eventCounterIntervalSec,
			IDictionary<string, (Func<string, int> IdEventCounterGetter, Action<EventCounterData> OnUpdate)> allowedCounters,
			EventLevel eventLevel = EventLevel.LogAlways,
			EventKeywords eventKeywords = EventKeywords.All,
			bool autoEnable = true)
			: this(eventSourceName, eventCounterIntervalSec, eventLevel, eventKeywords, false)
		{
			if (allowedCounters == null)
				throw new ArgumentNullException(nameof(allowedCounters));

			AllowedCountersWithFuncId = allowedCounters.ToDictionary(x => x.Key, x => x.Value);
			if (AllowedCountersWithFuncId.Count == 0 || AllowedCountersWithFuncId.Any(x => x.Value.OnUpdate == null))
				throw new InvalidOperationException($"Invalid {nameof(allowedCounters)} - missing Action<{nameof(EventCounterData)}>> OnUpdate");
			else if (AllowedCountersWithFuncId.Any(x => x.Value.IdEventCounterGetter == null))
				throw new InvalidOperationException($"Invalid {nameof(allowedCounters)} - missing Func<string, int>> IdEventCounterGetter");

			if (autoEnable)
				Enable();
		}

		public EventSourceAdapter(
			string eventSourceName,
			int eventCounterIntervalSec,
			Action<EventCounterData> onUpdate,
			Func<string, int> idEventCounterGetter,
			EventLevel eventLevel = EventLevel.LogAlways,
			EventKeywords eventKeywords = EventKeywords.All,
			bool autoEnable = true)
			: this(eventSourceName, eventCounterIntervalSec, eventLevel, eventKeywords, false)
		{
			AddOnUpdateEvent(onUpdate);
			_idEventCounterGetter = idEventCounterGetter ?? throw new ArgumentNullException(nameof(idEventCounterGetter));

			if (autoEnable)
				Enable();
		}

		public EventSourceAdapter(
			string eventSourceName,
			int eventCounterIntervalSec,
			Action<EventCounterItem>? onEventCounter,
			Action<IncrementingEventCounterItem>? onIncrementingEventCounter,
			Func<string, int> idEventCounterGetter,
			EventLevel eventLevel = EventLevel.LogAlways,
			EventKeywords eventKeywords = EventKeywords.All,
			bool autoEnable = true)
			: this(eventSourceName, eventCounterIntervalSec, eventLevel, eventKeywords, false)
		{
			_onEventCounter = onEventCounter;
			_onIncrementingEventCounter = onIncrementingEventCounter;

			if (_onEventCounter == null && _onIncrementingEventCounter == null)
				throw new ArgumentNullException($"{nameof(onEventCounter)} && {nameof(onIncrementingEventCounter)}");

			_idEventCounterGetter = idEventCounterGetter ?? throw new ArgumentNullException(nameof(idEventCounterGetter));

			if (autoEnable)
				Enable();
		}

		public EventSourceAdapter(EventSourceOptions options, Action<EventCounterData> onUpdate)
			: this(
				options?.EventSourceName ?? throw new ArgumentNullException(nameof(options)),
				options.EventCounterIntervalSec,
				options.EventLevel,
				options.EventKeywords,
				false)
		{
			if (!EventSourceName.Equals(options.EventSourceName, StringComparison.Ordinal))
				throw new InvalidOperationException($"Invalid {nameof(options)}.{nameof(options.EventSourceName)} Required {nameof(EventSourceName)} is '{EventSourceName}'");

			if (onUpdate == null)
				throw new ArgumentNullException(nameof(onUpdate));

			if (options.EventSourceAdapterAllowedCounters != null)
			{
				AllowedCounters = options.EventSourceAdapterAllowedCounters.ToDictionary(x => x.Key, x => (x.Value, onUpdate));
			}
			else if (options.EventSourceAdapterAllowedCounterGetters != null)
			{
				AllowedCountersWithFuncId = options.EventSourceAdapterAllowedCounterGetters.ToDictionary(x => x.Key, x => (x.Value, onUpdate));
			}
			else
			{
				AddOnUpdateEvent(onUpdate);
				_idEventCounterGetter = options.EventSourceAdapterIdEventCounterGetter ?? throw new ArgumentNullException($"{nameof(options)}.{nameof(options.EventSourceAdapterIdEventCounterGetter)}");
			}

			if (options.AutoEnable)
				Enable();
		}

		public void AddOnUpdateEvent(Action<EventCounterData>? onUpdate)
		{
			_onUpdate = onUpdate ?? throw new ArgumentNullException(nameof(onUpdate));
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

			if (!source.Name.Equals(EventSourceName, StringComparison.Ordinal))
				return;

			EnableEvents(source, EventLevel, EventKeywords, new Dictionary<string, string?> { { "EventCounterIntervalSec", EventCounterIntervalSec.ToString() } });
			_created = true;
		}

		protected override void OnEventWritten(EventWrittenEventArgs eventData)
		{
			if (eventData.EventSource.Name.Equals(EventSourceName, StringComparison.Ordinal))
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
				if (AllowedCounters != null)
				{
					(int IdEventCounter, Action<EventCounterData> OnUpdate) callback = default;
					if (AllowedCounters != null && !AllowedCounters.TryGetValue(name, out callback))
						return;

					callback.OnUpdate?.Invoke(new EventCounterData(callback.IdEventCounter, payload));
				}
				else if (AllowedCountersWithFuncId != null)
				{
					(Func<string, int> IdEventCounterGetter, Action<EventCounterData> OnUpdate) callback = default;
					if (AllowedCountersWithFuncId != null && !AllowedCountersWithFuncId.TryGetValue(name, out callback))
						return;

					callback.OnUpdate?.Invoke(new EventCounterData(callback.IdEventCounterGetter?.Invoke(name) ?? 0, payload));
				}
				else if (_onUpdate != null)
				{
					_onUpdate.Invoke(new EventCounterData(_idEventCounterGetter?.Invoke(name) ?? 0, payload));
				}
				else
				{
					if (payload.TryGetValue("CounterType", out object? counterTypeValue) && counterTypeValue is string counterType)
					{
						if (counterType.Equals("Mean", StringComparison.Ordinal))
							_onEventCounter?.Invoke(new EventCounterItem(_idEventCounterGetter?.Invoke(name) ?? 0, payload));
						else if (counterType.Equals("Sum", StringComparison.Ordinal))
							_onIncrementingEventCounter?.Invoke(new IncrementingEventCounterItem(_idEventCounterGetter?.Invoke(name) ?? 0, payload));
					}
				}
			}
		}
	}
}
