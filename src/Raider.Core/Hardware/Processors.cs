using System.Collections.Generic;
using System.Text;

namespace Raider.Hardware
{
	public class Processors : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public List<Processor> CPUs { get; set; }

		public Processors()
		{
			CPUs = new List<Processor>();
		}

		public IReadOnlyDictionary<string, object> ToDictionary()
		{
			var dict = new Dictionary<string, object>();

			if (CPUs != null)
				for (int i = 0; i < CPUs.Count; i++)
					dict.Add($"{nameof(CPUs)}[{i}]", CPUs[i]?.ToDictionary());

			return dict;
		}

		public void WriteTo(StringBuilder sb, string before = null, string after = null)
		{
			if (before != null)
				sb.AppendLine(before);

			foreach (var cpu in CPUs)
				cpu.WriteTo(sb);

			if (after != null)
				sb.AppendLine(after);
		}
	}
}
