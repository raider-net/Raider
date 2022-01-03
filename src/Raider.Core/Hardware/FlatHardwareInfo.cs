using Raider.Collections;
using Raider.Extensions;
using Raider.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raider.Hardware
{
	public class FlatHardwareInfo : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public Guid RuntimeUniqueKey => EnvironmentInfo.RUNTIME_UNIQUE_KEY;
		public string? HWThumbprint { get; set; }
		public double? TotalMemoryCapacityGB { get; set; }
		public double? MemoryAvailableGB { get; set; }
		public double? MemoryPercentUsed { get; set; }
		public double? PercentProcessorIdleTime { get; set; }
		public double? PercentProcessorTime { get; set; }
		public string? OS { get; set; }
		public string? HWJson { get; set; }

		public IDictionary<string, object?> ToDictionary(Serializer.ISerializer? serializer = null)
			=> new DictionaryBuilder<string>()
				.TryAdd(nameof(RuntimeUniqueKey), RuntimeUniqueKey, out _)
				.AddIfNotWhiteSpace(nameof(HWThumbprint), HWThumbprint, out _)
				.AddIfHasValue(nameof(TotalMemoryCapacityGB), TotalMemoryCapacityGB, out _)
				.AddIfHasValue(nameof(MemoryAvailableGB), MemoryAvailableGB, out _)
				.AddIfHasValue(nameof(MemoryPercentUsed), MemoryPercentUsed, out _)
				.AddIfHasValue(nameof(PercentProcessorIdleTime), PercentProcessorIdleTime, out _)
				.AddIfHasValue(nameof(PercentProcessorTime), PercentProcessorTime, out _)
				.AddIfNotWhiteSpace(nameof(OS), OS, out _)
				.AddIfNotWhiteSpace(nameof(HWJson), HWJson, out _)
				.ToObject();

		public override string ToString()
		{
			return $"{HWThumbprint} | {OS}";
		}

		public void WriteTo(StringBuilder sb, string? before = null, string? after = null)
		{
			sb
				.AppendLineSafe(before)
				.AppendLine($"HW = {this}")
				.AppendLineSafe(after);
		}
	}
}
