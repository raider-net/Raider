using Raider.Extensions;
using System.Collections.Generic;
using System.Text;

namespace Raider.Hardware
{
	public class GraphicsCards : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public List<GraphicsCard> GPUs { get; set; }

		public GraphicsCards()
		{
			GPUs = new List<GraphicsCard>();
		}

		public IDictionary<string, object?> ToDictionary()
		{
			var dict = new Dictionary<string, object?>();

			if (GPUs != null)
				for (int i = 0; i < GPUs.Count; i++)
					dict.Add($"{nameof(GPUs)}[{i}]", GPUs[i]?.ToDictionary());

			return dict;
		}

		public void WriteTo(StringBuilder sb, string? before = null, string? after = null)
		{
			sb.AppendLineSafe(before);

			foreach (var gpu in GPUs)
				gpu.WriteTo(sb);

			sb.AppendLineSafe(after);
		}
	}
}
