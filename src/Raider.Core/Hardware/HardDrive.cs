using Raider.Collections;
using Raider.Extensions;
using System.Collections.Generic;
using System.Text;

namespace Raider.Hardware
{
	public class HardDrive : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public string? Caption { get; set; }
		public string? Name { get; set; }
		public string? Manufacturer { get; set; }
		public string? Model { get; set; }
		public uint? Signature { get; set; }
		public ulong? Capacity { get; set; }

		public double? CapacityGB =>
			Capacity.HasValue
				? (Capacity.Value / (double)(1024 * 1024 * 1024))
				: (double?)null;

		public string? SerialNumber { get; set; }

		public IReadOnlyDictionary<string, object?> ToDictionary()
			=> new DictionaryBuilder<string>()
				.AddIfNotWhiteSpace(nameof(Caption), Caption, out _)
				.AddIfNotWhiteSpace(nameof(Name), Name, out _)
				.AddIfNotWhiteSpace(nameof(Manufacturer), Manufacturer, out _)
				.AddIfNotWhiteSpace(nameof(Model), Model, out _)
				.AddIfHasValue(nameof(Signature), Signature, out _)
				.AddIfHasValue(nameof(Capacity), Capacity, out _)
				.AddIfNotWhiteSpace(nameof(SerialNumber), SerialNumber, out _)
				.ToObject();

		public override string ToString()
		{
			return $"{Caption} | {CapacityGB} GB | {SerialNumber}";
		}

		public void WriteTo(StringBuilder sb, string? before = null, string? after = null)
		{
			sb
				.AppendLineSafe(before)
				.AppendLine($"HDD = {this}")
				.AppendLineSafe(after);
		}
	}
}
