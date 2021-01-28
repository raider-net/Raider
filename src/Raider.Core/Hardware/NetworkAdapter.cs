using Raider.Collections;
using Raider.Extensions;
using System.Collections.Generic;
using System.Text;

namespace Raider.Hardware
{
	public class NetworkAdapter : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public string? Caption { get; set; }
		public string? Description { get; set; }
		public bool? IsIpEnabled { get; set; }
		public string? MacAddress { get; set; }
		public string? DNSDomain { get; set; }
		public string? DNSHostName { get; set; }

		public IReadOnlyDictionary<string, object?> ToDictionary()
			=> new DictionaryBuilder<string>()
				.AddIfNotWhiteSpace(nameof(Caption), Caption, out _)
				.AddIfNotWhiteSpace(nameof(Description), Description, out _)
				.AddIfHasValue(nameof(IsIpEnabled), IsIpEnabled, out _)
				.AddIfNotWhiteSpace(nameof(MacAddress), MacAddress, out _)
				.AddIfNotWhiteSpace(nameof(DNSDomain), DNSDomain, out _)
				.AddIfNotWhiteSpace(nameof(DNSHostName), DNSHostName, out _)
				.ToObject();

		public override string ToString()
		{
			return $"{Description} | {MacAddress}";
		}

		public void WriteTo(StringBuilder sb, string? before = null, string? after = null)
		{
			sb
				.AppendLineSafe(before)
				.AppendLine($"NetworkAdapter = {this}")
				.AppendLineSafe(after);
		}
	}
}
