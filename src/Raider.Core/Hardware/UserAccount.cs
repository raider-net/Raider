using Raider.Collections;
using System.Collections.Generic;
using System.Text;

namespace Raider.Hardware
{
	public class UserAccount : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public string? Name { get; set; }
		public string? FullName { get; set; }
		public bool? IsDisabled { get; set; }

		public IReadOnlyDictionary<string, object?> ToDictionary()
			=> new DictionaryBuilder<string>()
				.AddIfNotWhiteSpace(nameof(Name), Name, out _)
				.AddIfNotWhiteSpace(nameof(FullName), FullName, out _)
				.AddIfHasValue(nameof(IsDisabled), IsDisabled, out _)
				.ToObject();

		public override string ToString()
		{
			return $"{FullName} | {Name}";
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
