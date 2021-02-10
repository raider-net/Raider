using System;
using System.Collections.Generic;

namespace Raider.Validation
{
	internal class ValidationFrameFailure : IValidationFrame
	{
		public string? ObjectType { get; set; }
		public string? PropertyName { get; set; }
		public IValidationFrame? Parent { get; set; }
		public int? Index { get; set; }
		public int Depth { get; set; }
		public string? PropertyNameWithIndex => GetPropertyNameWithIndex();

		private string? GetPropertyNameWithIndex()
		{
			if (Index.HasValue)
			{
				if (string.IsNullOrWhiteSpace(PropertyName))
					throw new InvalidOperationException($"PropertyName is null, but {nameof(Index)} = {Index}");

				return $"{PropertyName}[{Index}]";
			}
			return PropertyName;
		}

		public override string ToString()
		{
			var path = new List<string> { GetPropertyNameWithIndex() ?? "_" };
			var parent = Parent;
			while (parent != null)
			{
				path.Add(parent.PropertyNameWithIndex ?? "_");
				parent = parent.Parent;
			}

			path.Reverse();
			return string.Join(".", path);
		}
	}
}
