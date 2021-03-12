using Raider.Collections;
using Raider.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raider.Hardware
{
	public class Memory : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public List<RamStick> RamSticks { get; set; }

		public ulong? TotalMemoryCapacity { get; set; }

		public double? TotalMemoryCapacityGB =>
			TotalMemoryCapacity.HasValue
				? Math.Round(TotalMemoryCapacity.Value / (double)(1024 * 1024 * 1024), 2)
				: (double?)null;

		public ulong? AvailableBytes { get; set; }

		public double? PercentUsed => !TotalMemoryCapacity.HasValue || !AvailableBytes.HasValue
			? (double?)null
			: Math.Round((double)100 - (((double)(AvailableBytes.Value * 100)) / TotalMemoryCapacity.Value), 2);

		public Memory()
		{
			RamSticks = new List<RamStick>();
		}

		public IDictionary<string, object?> ToDictionary()
			=> new DictionaryBuilder<string>()
				.AddIfHasValue(nameof(TotalMemoryCapacity), TotalMemoryCapacity, out _)
				.AddIfHasValue(nameof(TotalMemoryCapacityGB), TotalMemoryCapacityGB, out _)
				.AddIfHasValue(nameof(AvailableBytes), AvailableBytes, out _)
				.AddIfHasValue(nameof(PercentUsed), PercentUsed, out _)
				.ToObject();

		public override string ToString()
		{
			return $"{TotalMemoryCapacityGB} GB";
		}

		public void WriteTo(StringBuilder sb, string? before = null, string? after = null)
		{
			sb
				.AppendLineSafe(before)
				.AppendLine($"Installed RAM = {TotalMemoryCapacityGB} GB")
				.AppendLineSafe(PercentUsed.HasValue, () => $"Used memory = {PercentUsed} %");

			foreach (var ram in RamSticks)
				ram.WriteTo(sb);

			sb.AppendLineSafe(after);
		}
	}
}
