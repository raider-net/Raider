using Raider.Database.PostgreSql;
using Raider.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Metrics.PostgreSql
{
	internal class EventCounterDataWriter : DbBatchWriter<DbEventCounterData>, IEventCounterDataWriter, IDisposable
	{
		private bool _enabledWrite;
		public IReadOnlyDictionary<string, IEventListener> EventListeners { get; }

		public EventCounterDataWriter(
			DBEventCounterDataWriterOptions options,
			List<EventSourceOptions> eventSourceOptions,
			List<IEventListener>? customListeners = null,
			Action<string, object?, object?, object?>? errorLogger = null)
			: base(options ?? new DBEventCounterDataWriterOptions(), errorLogger ?? DefaultErrorLoggerDelegate.Log)
		{
			if (eventSourceOptions == null || eventSourceOptions.Count == 0)
				throw new ArgumentNullException(nameof(eventSourceOptions));

			eventSourceOptions = eventSourceOptions.Where(x => x != null).ToList();
			var eventSourceNames = eventSourceOptions.Select(x => x.EventSourceName).ToList();

			if (customListeners != null)
			{
				customListeners = customListeners.Where(x => x != null).ToList();
				var esNames = customListeners.Select(x => x.EventSourceName);
				eventSourceNames.AddRange(esNames);
			}

			var multipleEventSourceNames = eventSourceNames.GroupBy(x => x).Where(x => 1 < x.Count()).Select(x => x.Key).ToList();
			if (0 < multipleEventSourceNames.Count)
				throw new InvalidOperationException($"Multipne event sources: {string.Join(", ", multipleEventSourceNames)}");

			var eventListeners = new Dictionary<string, IEventListener>();

			_enabledWrite = true;

			void AddEventListener(IEventListener eventListener)
				=> eventListeners.Add(eventListener.EventSourceName, eventListener);

			foreach (var esOptions in eventSourceOptions)
			{
				if (AspNetCoreHttpConnectionsEventSource.EVENT_SOURCE_NAME.Equals(esOptions.EventSourceName, StringComparison.Ordinal))
				{
					AddEventListener(new AspNetCoreHttpConnectionsEventSource(esOptions, OnUpdate));
				}
				else if (AspNetCoreServerKestrelEventSource.EVENT_SOURCE_NAME.Equals(esOptions.EventSourceName, StringComparison.Ordinal))
				{
					AddEventListener(new AspNetCoreServerKestrelEventSource(esOptions, OnUpdate));
				}
				else if (AspNetHostingEventSource.EVENT_SOURCE_NAME.Equals(esOptions.EventSourceName, StringComparison.Ordinal))
				{
					AddEventListener(new AspNetHostingEventSource(esOptions, OnUpdate));
				}
				else if (SystemNetHttpEventSource.EVENT_SOURCE_NAME.Equals(esOptions.EventSourceName, StringComparison.Ordinal))
				{
					AddEventListener(new SystemNetHttpEventSource(esOptions, OnUpdate));
				}
				else if (SystemRuntimeEventSource.EVENT_SOURCE_NAME.Equals(esOptions.EventSourceName, StringComparison.Ordinal))
				{
					AddEventListener(new SystemRuntimeEventSource(esOptions, OnUpdate));
				}
				else
				{
					AddEventListener(new EventSourceAdapter(esOptions, OnUpdate));
				}
			}

			if (customListeners != null)
			{
				foreach (var customListener in customListeners)
				{
					customListener.AddOnUpdateEvent(OnUpdate);
					AddEventListener(customListener);
				}
			}

			EventListeners = eventListeners;
		}

		public override IDictionary<string, object?>? ToDictionary(DbEventCounterData dbEventCounterData)
			=> dbEventCounterData.ToDictionary();

		private void OnUpdate(EventCounterData data)
		{
			if (_enabledWrite)
				Write(new DbEventCounterData(data));
		}

		public void EnableAllEventListeners()
		{
			_enabledWrite = true;

			foreach (var listener in EventListeners.Values)
				listener.Enable();
		}

		public void DisableWriteForAllEventListeners()
		{
			_enabledWrite = false;

			foreach (var listener in EventListeners.Values)
				listener.DisableWrite();
		}
	}
}
