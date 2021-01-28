using Raider.Collections;
using Raider.Extensions;
using System.Collections.Generic;
using System.Text;

namespace Raider.Hardware
{
	public class Bios : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public string? Name { get; set; }
		public string? Manufacturer { get; set; }
		public string? SerialNumber { get; set; }
		public string? Version { get; set; }

		public IReadOnlyDictionary<string, object?> ToDictionary()
			=> new DictionaryBuilder<string>()
				.AddIfNotWhiteSpace(nameof(Name), Name, out _)
				.AddIfNotWhiteSpace(nameof(Manufacturer), Manufacturer, out _)
				.AddIfNotWhiteSpace(nameof(SerialNumber), SerialNumber, out _)
				.AddIfNotWhiteSpace(nameof(Version), Version, out _)
				.ToObject();

		public override string ToString()
		{
			return $"{Manufacturer} {Name} | {SerialNumber}";
		}

		public void WriteTo(StringBuilder sb, string? before = null, string? after = null)
		{
			sb
				.AppendLineSafe(before)
				.AppendLine($"BIOS = {this}")
				.AppendLineSafe(after);
		}
	}
}
