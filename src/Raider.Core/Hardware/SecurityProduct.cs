using Raider.Collections;
using System.Collections.Generic;
using System.Text;

namespace Raider.Hardware
{
	public class SecurityProduct : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public string? ProductType { get; set; }
		public string? Name { get; set; }
		public string? PathToExe { get; set; }

		public IDictionary<string, object?> ToDictionary()
			=> new DictionaryBuilder<string>()
				.AddIfNotWhiteSpace(nameof(ProductType), ProductType, out _)
				.AddIfNotWhiteSpace(nameof(Name), Name, out _)
				.AddIfNotWhiteSpace(nameof(PathToExe), PathToExe, out _)
				.ToObject();

		public override string ToString()
		{
			return $"{ProductType}: {Name}";
		}

		public void WriteTo(StringBuilder sb, string? before = null, string? after = null)
		{
			if (before != null)
				sb.AppendLine(before);

			sb.AppendLine($"Security = {this}");

			if (after != null)
				sb.AppendLine(after);
		}
	}
}
