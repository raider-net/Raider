using Raider.Collections;
using Raider.Extensions;
using System.Collections.Generic;
using System.Text;

namespace Raider.Hardware
{
	public class GraphicsCard : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public uint? MemoryCapacity { get; set; }
		public double? MemoryCapacityMB =>
			MemoryCapacity.HasValue
				? (MemoryCapacity.Value / (double)(1024 * 1024))
				: (double?)null;

		public string? Caption { get; set; }
		public string? Description { get; set; }
		public string? Name { get; set; }
		public string? DeviceID { get; set; }

		public IDictionary<string, object?> ToDictionary(Serializer.ISerializer? serializer = null)
			=> new DictionaryBuilder<string>()
					.AddIfHasValue(nameof(MemoryCapacity), MemoryCapacity, out _)
					.AddIfHasValue(nameof(MemoryCapacityMB), MemoryCapacityMB, out _)
					.AddIfNotWhiteSpace(nameof(Caption), Caption, out _)
					.AddIfNotWhiteSpace(nameof(Description), Description, out _)
					.AddIfNotWhiteSpace(nameof(Name), Name, out _)
					.AddIfNotWhiteSpace(nameof(DeviceID), DeviceID, out _)
					.ToObject();

		public override string ToString()
		{
			return $"{Caption} | {MemoryCapacityMB} MB";
		}

		public void WriteTo(StringBuilder sb, string? before = null, string? after = null)
		{
			sb
				.AppendLineSafe(before)
				.AppendLine($"GPU = {this}")
				.AppendLineSafe(after);
		}
	}
}
