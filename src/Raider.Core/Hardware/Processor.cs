using Raider.Collections;
using Raider.Extensions;
using System.Collections.Generic;
using System.Text;

namespace Raider.Hardware
{
	public class Processor : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public uint? ClockSpeed { get; set; }
		public uint? NumberOfLogicalProcessors { get; set; }
		public string? Name { get; set; }
		public string? Manufacturer { get; set; }
		public uint? NumberOfCores { get; set; }
		public string? Id { get; set; }
		public string? UniqueId { get; set; }
		public ulong? PercentIdleTime { get; set; }
		public ulong? PercentProcessorTime { get; set; }

		public IDictionary<string, object?> ToDictionary(Serializer.ISerializer? serializer = null)
			=> new DictionaryBuilder<string>()
				.AddIfHasValue(nameof(ClockSpeed), ClockSpeed, out _)
				.AddIfHasValue(nameof(NumberOfLogicalProcessors), NumberOfLogicalProcessors, out _)
				.AddIfNotWhiteSpace(nameof(Name), Name, out _)
				.AddIfNotWhiteSpace(nameof(Manufacturer), Manufacturer, out _)
				.AddIfHasValue(nameof(NumberOfCores), NumberOfCores, out _)
				.AddIfNotWhiteSpace(nameof(Id), Id, out _)
				.AddIfNotWhiteSpace(nameof(UniqueId), UniqueId, out _)
				.AddIfHasValue(nameof(PercentIdleTime), PercentIdleTime, out _)
				.AddIfHasValue(nameof(PercentProcessorTime), PercentProcessorTime, out _)
				.ToObject();

		public override string ToString()
		{
			return $"{Name} | Cores = {NumberOfCores} | Logical processors = {NumberOfLogicalProcessors}";
		}

		public void WriteTo(StringBuilder sb, string? before = null, string? after = null)
		{
			sb
				.AppendLineSafe(before)
				.AppendLine($"Processor = {this}")
				.AppendLineSafe(PercentIdleTime.HasValue, () => $"PercentIdleTime = {PercentIdleTime}")
				.AppendLineSafe(PercentProcessorTime.HasValue, () => $"PercentProcessorTime = {PercentProcessorTime}")
				.AppendLineSafe(after);
		}
	}
}
