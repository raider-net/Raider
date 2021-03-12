using Raider.Collections;
using Raider.Extensions;
using System.Collections.Generic;
using System.Text;

namespace Raider.Hardware
{
	public class MotherBoard : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public string? Model { get; set; }
		public string? Product { get; set; }
		public string? Name { get; set; }
		public string? Manufacturer { get; set; }
		public string? SerialNumber { get; set; }
		public string? Version { get; set; }

		public IDictionary<string, object?> ToDictionary()
			=> new DictionaryBuilder<string>()
				.AddIfNotWhiteSpace(nameof(Model), Model, out _)
				.AddIfNotWhiteSpace(nameof(Product), Product, out _)
				.AddIfNotWhiteSpace(nameof(Name), Name, out _)
				.AddIfNotWhiteSpace(nameof(Manufacturer), Manufacturer, out _)
				.AddIfNotWhiteSpace(nameof(SerialNumber), SerialNumber, out _)
				.AddIfNotWhiteSpace(nameof(Version), Version, out _)
				.ToObject();

		public override string ToString()
		{
			return $"{Product} | {Version} | {SerialNumber}";
		}

		public void WriteTo(StringBuilder sb, string? before = null, string? after = null)
		{
			sb
				.AppendLineSafe(before)
				.AppendLine($"Motherboard = {this}")
				.AppendLineSafe(after);
		}
	}
}
