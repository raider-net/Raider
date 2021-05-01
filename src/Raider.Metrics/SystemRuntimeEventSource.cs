using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace Raider.Metrics
{
	public class SystemRuntimeEventSource : EventListener, IEventListener
	{
		public const string EVENT_SOURCE_NAME = "System.Runtime";

		private const string _cpuUsage = "cpu-usage";
		private const string _workingSet = "working-set";
		private const string _gcHeapSize = "gc-heap-size";
		private const string _gen0GcCount = "gen-0-gc-count";
		private const string _gen1GcCount = "gen-1-gc-count";
		private const string _gen2GcCount = "gen-2-gc-count";
		private const string _threadpoolThreadCount = "threadpool-thread-count";
		private const string _monitorLockContentionCount = "monitor-lock-contention-count";
		private const string _threadpoolQueueLength = "threadpool-queue-length";
		private const string _threadpoolCompletedItemsCount = "threadpool-completed-items-count";
		private const string _allocRate = "alloc-rate";
		private const string _activeTimerCount = "active-timer-count";
		private const string _gcFragmentation = "gc-fragmentation";
		private const string _exceptionCount = "exception-count";
		private const string _timeInGc = "time-in-gc";
		private const string _gen0Size = "gen-0-size";
		private const string _gen1Size = "gen-1-size";
		private const string _gen2Size = "gen-2-size";
		private const string _lohSize = "loh-size";
		private const string _pohSize = "poh-size";
		private const string _assemblyCount = "assembly-count";
		private const string _ilBytesJitted = "il-bytes-jitted";
		private const string _methodsJittedCount = "methods-jitted-count";

		private readonly Dictionary<string, string> _countersMap;

		public bool Enabled { get; private set; }

		public string EventSourceName => EVENT_SOURCE_NAME;
		public int EventCounterIntervalSec { get; }
		public EventLevel EventLevel { get; }
		public EventKeywords EventKeywords { get; }
		public List<string>? AllowedCounters { get; }

		public EventCounterItem CpuUsage { get; }
		public EventCounterItem WorkingSet { get; }
		public EventCounterItem GcHeapSize { get; }
		public IncrementingEventCounterItem Gen0GcCount { get; }
		public IncrementingEventCounterItem Gen1GcCount { get; }
		public IncrementingEventCounterItem Gen2GcCount { get; }
		public EventCounterItem ThreadpoolThreadCount { get; }
		public IncrementingEventCounterItem MonitorLockContentionCount { get; }
		public EventCounterItem ThreadpoolQueueLength { get; }
		public IncrementingEventCounterItem ThreadpoolCompletedItemsCount { get; }
		public IncrementingEventCounterItem AllocRate { get; }
		public EventCounterItem ActiveTimerCount { get; }
		public EventCounterItem GcFragmentation { get; }
		public IncrementingEventCounterItem ExceptionCount { get; }
		public EventCounterItem TimeInGc { get; }
		public EventCounterItem Gen0Size { get; }
		public EventCounterItem Gen1Size { get; }
		public EventCounterItem Gen2Size { get; }
		public EventCounterItem LohSize { get; }
		public EventCounterItem PohSize { get; }
		public EventCounterItem AssemblyCount { get; }
		public EventCounterItem IlBytesJitted { get; }
		public EventCounterItem MethodsJittedCount { get; }

		public Dictionary<string, Func<double?>> ActualValues { get; }
		public ConcurrentDictionary<string, bool> UnhandledPayloads { get; }

		public SystemRuntimeEventSource(
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
				[_cpuUsage] = nameof(CpuUsage),
				[_workingSet] = nameof(WorkingSet),
				[_gcHeapSize] = nameof(GcHeapSize),
				[_gen0GcCount] = nameof(Gen0GcCount),
				[_gen1GcCount] = nameof(Gen1GcCount),
				[_gen2GcCount] = nameof(Gen2GcCount),
				[_threadpoolThreadCount] = nameof(ThreadpoolThreadCount),
				[_monitorLockContentionCount] = nameof(MonitorLockContentionCount),
				[_threadpoolQueueLength] = nameof(ThreadpoolQueueLength),
				[_threadpoolCompletedItemsCount] = nameof(ThreadpoolCompletedItemsCount),
				[_allocRate] = nameof(AllocRate),
				[_activeTimerCount] = nameof(ActiveTimerCount),
				[_gcFragmentation] = nameof(GcFragmentation),
				[_exceptionCount] = nameof(ExceptionCount),
				[_timeInGc] = nameof(TimeInGc),
				[_gen0Size] = nameof(Gen0Size),
				[_gen1Size] = nameof(Gen1Size),
				[_gen2Size] = nameof(Gen2Size),
				[_lohSize] = nameof(LohSize),
				[_pohSize] = nameof(PohSize),
				[_assemblyCount] = nameof(AssemblyCount),
				[_ilBytesJitted] = nameof(IlBytesJitted),
				[_methodsJittedCount] = nameof(MethodsJittedCount),
			};

			CpuUsage = new EventCounterItem((int)EventCounterEnum.SystemRuntimeCpuUsage);
			WorkingSet = new EventCounterItem((int)EventCounterEnum.SystemRuntimeWorkingSet);
			GcHeapSize = new EventCounterItem((int)EventCounterEnum.SystemRuntimeGcHeapSize);
			Gen0GcCount = new IncrementingEventCounterItem((int)EventCounterEnum.SystemRuntimeGen0GcCount);
			Gen1GcCount = new IncrementingEventCounterItem((int)EventCounterEnum.SystemRuntimeGen1GcCount);
			Gen2GcCount = new IncrementingEventCounterItem((int)EventCounterEnum.SystemRuntimeGen2GcCount);
			ThreadpoolThreadCount = new EventCounterItem((int)EventCounterEnum.SystemRuntimeThreadpoolThreadCount);
			MonitorLockContentionCount = new IncrementingEventCounterItem((int)EventCounterEnum.SystemRuntimeMonitorLockContentionCount);
			ThreadpoolQueueLength = new EventCounterItem((int)EventCounterEnum.SystemRuntimeThreadpoolQueueLength);
			ThreadpoolCompletedItemsCount = new IncrementingEventCounterItem((int)EventCounterEnum.SystemRuntimeThreadpoolCompletedItemsCount);
			AllocRate = new IncrementingEventCounterItem((int)EventCounterEnum.SystemRuntimeAllocRate);
			ActiveTimerCount = new EventCounterItem((int)EventCounterEnum.SystemRuntimeActiveTimerCount);
			GcFragmentation = new EventCounterItem((int)EventCounterEnum.SystemRuntimeGcFragmentation);
			ExceptionCount = new IncrementingEventCounterItem((int)EventCounterEnum.SystemRuntimeExceptionCount);
			TimeInGc = new EventCounterItem((int)EventCounterEnum.SystemRuntimeTimeInGc);
			Gen0Size = new EventCounterItem((int)EventCounterEnum.SystemRuntimeGen0Size);
			Gen1Size = new EventCounterItem((int)EventCounterEnum.SystemRuntimeGen1Size);
			Gen2Size = new EventCounterItem((int)EventCounterEnum.SystemRuntimeGen2Size);
			LohSize = new EventCounterItem((int)EventCounterEnum.SystemRuntimeLohSize);
			PohSize = new EventCounterItem((int)EventCounterEnum.SystemRuntimePohSize);
			AssemblyCount = new EventCounterItem((int)EventCounterEnum.SystemRuntimeAssemblyCount);
			IlBytesJitted = new EventCounterItem((int)EventCounterEnum.SystemRuntimeIlBytesJitted);
			MethodsJittedCount = new EventCounterItem((int)EventCounterEnum.SystemRuntimeMethodsJittedCount);

			ActualValues = new Dictionary<string, Func<double?>>
			{
				[nameof(CpuUsage)] = () => CpuUsage.Mean,
				[nameof(WorkingSet)] = () => WorkingSet.Mean,
				[nameof(GcHeapSize)] = () => GcHeapSize.Mean,
				[nameof(Gen0GcCount)] = () => Gen0GcCount.Increment,
				[nameof(Gen1GcCount)] = () => Gen1GcCount.Increment,
				[nameof(Gen2GcCount)] = () => Gen2GcCount.Increment,
				[nameof(ThreadpoolThreadCount)] = () => ThreadpoolThreadCount.Mean,
				[nameof(MonitorLockContentionCount)] = () => MonitorLockContentionCount.Increment,
				[nameof(ThreadpoolQueueLength)] = () => ThreadpoolQueueLength.Mean,
				[nameof(ThreadpoolCompletedItemsCount)] = () => ThreadpoolCompletedItemsCount.Increment,
				[nameof(AllocRate)] = () => AllocRate.Increment,
				[nameof(ActiveTimerCount)] = () => ActiveTimerCount.Mean,
				[nameof(GcFragmentation)] = () => GcFragmentation.Mean,
				[nameof(ExceptionCount)] = () => ExceptionCount.Increment,
				[nameof(TimeInGc)] = () => TimeInGc.Mean,
				[nameof(Gen0Size)] = () => Gen0Size.Mean,
				[nameof(Gen1Size)] = () => Gen1Size.Mean,
				[nameof(Gen2Size)] = () => Gen2Size.Mean,
				[nameof(LohSize)] = () => LohSize.Mean,
				[nameof(PohSize)] = () => PohSize.Mean,
				[nameof(AssemblyCount)] = () => AssemblyCount.Mean,
				[nameof(IlBytesJitted)] = () => IlBytesJitted.Mean,
				[nameof(MethodsJittedCount)] = () => MethodsJittedCount.Mean
			};

			UnhandledPayloads = new ConcurrentDictionary<string, bool>();

			if (autoEnable)
				Enable();
		}

		public SystemRuntimeEventSource(
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
					if (kvp.Key == nameof(CpuUsage))
						CpuUsage.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(WorkingSet))
						WorkingSet.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(GcHeapSize))
						GcHeapSize.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(Gen0GcCount))
						Gen0GcCount.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(Gen1GcCount))
						Gen1GcCount.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(Gen2GcCount))
						Gen2GcCount.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(ThreadpoolThreadCount))
						ThreadpoolThreadCount.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(MonitorLockContentionCount))
						MonitorLockContentionCount.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(ThreadpoolQueueLength))
						ThreadpoolQueueLength.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(ThreadpoolCompletedItemsCount))
						ThreadpoolCompletedItemsCount.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(AllocRate))
						AllocRate.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(ActiveTimerCount))
						ActiveTimerCount.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(GcFragmentation))
						GcFragmentation.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(ExceptionCount))
						ExceptionCount.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(TimeInGc))
						TimeInGc.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(Gen0Size))
						Gen0Size.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(Gen1Size))
						Gen1Size.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(Gen2Size))
						Gen2Size.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(LohSize))
						LohSize.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(PohSize))
						PohSize.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(AssemblyCount))
						AssemblyCount.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(IlBytesJitted))
						IlBytesJitted.OnUpdate += kvp.Value;
					else if (kvp.Key == nameof(MethodsJittedCount))
						MethodsJittedCount.OnUpdate += kvp.Value;
				}
			}

			if (autoEnable)
				Enable();
		}

		public SystemRuntimeEventSource(
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

		public SystemRuntimeEventSource(EventSourceOptions options, Action<EventCounterData> onUpdate)
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
					if (allowedCounter == nameof(CpuUsage))
						CpuUsage.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(WorkingSet))
						WorkingSet.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(GcHeapSize))
						GcHeapSize.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(Gen0GcCount))
						Gen0GcCount.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(Gen1GcCount))
						Gen1GcCount.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(Gen2GcCount))
						Gen2GcCount.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(ThreadpoolThreadCount))
						ThreadpoolThreadCount.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(MonitorLockContentionCount))
						MonitorLockContentionCount.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(ThreadpoolQueueLength))
						ThreadpoolQueueLength.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(ThreadpoolCompletedItemsCount))
						ThreadpoolCompletedItemsCount.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(AllocRate))
						AllocRate.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(ActiveTimerCount))
						ActiveTimerCount.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(GcFragmentation))
						GcFragmentation.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(ExceptionCount))
						ExceptionCount.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(TimeInGc))
						TimeInGc.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(Gen0Size))
						Gen0Size.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(Gen1Size))
						Gen1Size.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(Gen2Size))
						Gen2Size.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(LohSize))
						LohSize.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(PohSize))
						PohSize.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(AssemblyCount))
						AssemblyCount.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(IlBytesJitted))
						IlBytesJitted.OnUpdate += onUpdate;
					else if (allowedCounter == nameof(MethodsJittedCount))
						MethodsJittedCount.OnUpdate += onUpdate;
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
				CpuUsage.OnUpdate += onUpdate;
				WorkingSet.OnUpdate += onUpdate;
				GcHeapSize.OnUpdate += onUpdate;
				Gen0GcCount.OnUpdate += onUpdate;
				Gen1GcCount.OnUpdate += onUpdate;
				Gen2GcCount.OnUpdate += onUpdate;
				ThreadpoolThreadCount.OnUpdate += onUpdate;
				MonitorLockContentionCount.OnUpdate += onUpdate;
				ThreadpoolQueueLength.OnUpdate += onUpdate;
				ThreadpoolCompletedItemsCount.OnUpdate += onUpdate;
				AllocRate.OnUpdate += onUpdate;
				ActiveTimerCount.OnUpdate += onUpdate;
				GcFragmentation.OnUpdate += onUpdate;
				ExceptionCount.OnUpdate += onUpdate;
				TimeInGc.OnUpdate += onUpdate;
				Gen0Size.OnUpdate += onUpdate;
				Gen1Size.OnUpdate += onUpdate;
				Gen2Size.OnUpdate += onUpdate;
				LohSize.OnUpdate += onUpdate;
				PohSize.OnUpdate += onUpdate;
				AssemblyCount.OnUpdate += onUpdate;
				IlBytesJitted.OnUpdate += onUpdate;
				MethodsJittedCount.OnUpdate += onUpdate;
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

				if (name.Equals(_cpuUsage, StringComparison.Ordinal))
				{
					CpuUsage.Update(payload);
				}
				else if (name.Equals(_workingSet, StringComparison.Ordinal))
				{
					WorkingSet.Update(payload);
				}
				else if (name.Equals(_gcHeapSize, StringComparison.Ordinal))
				{
					GcHeapSize.Update(payload);
				}
				else if (name.Equals(_gen0GcCount, StringComparison.Ordinal))
				{
					Gen0GcCount.Update(payload);
				}
				else if (name.Equals(_gen1GcCount, StringComparison.Ordinal))
				{
					Gen1GcCount.Update(payload);
				}
				else if (name.Equals(_gen2GcCount, StringComparison.Ordinal))
				{
					Gen2GcCount.Update(payload);
				}
				else if (name.Equals(_threadpoolThreadCount, StringComparison.Ordinal))
				{
					ThreadpoolThreadCount.Update(payload);
				}
				else if (name.Equals(_monitorLockContentionCount, StringComparison.Ordinal))
				{
					MonitorLockContentionCount.Update(payload);
				}
				else if (name.Equals(_threadpoolQueueLength, StringComparison.Ordinal))
				{
					ThreadpoolQueueLength.Update(payload);
				}
				else if (name.Equals(_threadpoolCompletedItemsCount, StringComparison.Ordinal))
				{
					ThreadpoolCompletedItemsCount.Update(payload);
				}
				else if (name.Equals(_allocRate, StringComparison.Ordinal))
				{
					AllocRate.Update(payload);
				}
				else if (name.Equals(_activeTimerCount, StringComparison.Ordinal))
				{
					ActiveTimerCount.Update(payload);
				}
				else if (name.Equals(_gcFragmentation, StringComparison.Ordinal))
				{
					GcFragmentation.Update(payload);
				}
				else if (name.Equals(_exceptionCount, StringComparison.Ordinal))
				{
					ExceptionCount.Update(payload);
				}
				else if (name.Equals(_timeInGc, StringComparison.Ordinal))
				{
					TimeInGc.Update(payload);
				}
				else if (name.Equals(_gen0Size, StringComparison.Ordinal))
				{
					Gen0Size.Update(payload);
				}
				else if (name.Equals(_gen1Size, StringComparison.Ordinal))
				{
					Gen1Size.Update(payload);
				}
				else if (name.Equals(_gen2Size, StringComparison.Ordinal))
				{
					Gen2Size.Update(payload);
				}
				else if (name.Equals(_lohSize, StringComparison.Ordinal))
				{
					LohSize.Update(payload);
				}
				else if (name.Equals(_pohSize, StringComparison.Ordinal))
				{
					PohSize.Update(payload);
				}
				else if (name.Equals(_assemblyCount, StringComparison.Ordinal))
				{
					AssemblyCount.Update(payload);
				}
				else if (name.Equals(_ilBytesJitted, StringComparison.Ordinal))
				{
					IlBytesJitted.Update(payload);
				}
				else if (name.Equals(_methodsJittedCount, StringComparison.Ordinal))
				{
					MethodsJittedCount.Update(payload);
				}
				else
				{
					UnhandledPayloads.TryAdd(name, true);
				}
			}
		}
	}
}
