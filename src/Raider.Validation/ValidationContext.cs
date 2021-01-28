using System;
using System.Collections.Generic;

namespace Raider.Validation
{
	public class ValidationContext
	{
		public object? InstanceToValidate { get; }
		public ValidationContext? Parent { get; private set; }
		public Dictionary<int, int> Indexes { get; } //Dictionary<Depth, Index>

		public ValidationContext(object? instanceToValidate, ValidationContext? parent)
		{
			InstanceToValidate = instanceToValidate;
			Parent = parent;
			Indexes = Parent?.Indexes ?? new Dictionary<int, int>();
		}

		public IValidationFrame ToReadOnlyValidationFrame(IValidationFrame validationFrame)
		{
			if (validationFrame == null)
				throw new ArgumentNullException(nameof(validationFrame));

			var result = new ValidationFrameFailure
			{
				Depth = validationFrame.Depth,
				ObjectType = validationFrame.ObjectType,
				PropertyName = validationFrame.PropertyName
			};

			if (validationFrame is ValidationFrameEnumeration enumeration)
				result.Index = Indexes[enumeration.Depth];

			if (validationFrame.Parent != null)
			{
				result.Parent = ToReadOnlyValidationFrame(validationFrame.Parent);
			}

			return result;
		}
	}
}
