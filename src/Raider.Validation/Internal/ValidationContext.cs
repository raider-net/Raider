using System;
using System.Collections.Generic;

namespace Raider.Validation
{
	internal class ValidationContext
	{
		private IValidationFrame? _validationFrame;

		public object? InstanceToValidate { get; }
		public Dictionary<int, int> Indexes { get; } //Dictionary<Depth, Index>

		public ValidationContext(object? instanceToValidate, ValidationContext? parent)
		{
			InstanceToValidate = instanceToValidate;
			Indexes = parent?.Indexes ?? new Dictionary<int, int>();
		}

		public ValidationContext SetValidationFrame(IValidationFrame validationFrame)
		{
			_validationFrame = validationFrame ?? throw new ArgumentNullException(nameof(validationFrame));
			
			if (string.IsNullOrWhiteSpace(_validationFrame.PropertyName))
				throw new InvalidOperationException($"{nameof(_validationFrame)}.{nameof(_validationFrame.PropertyName)} == null");

			return this;
		}

		public IValidationFrame ToReadOnlyValidationFrame()
			=> ToReadOnlyValidationFrame(_validationFrame);

		public IValidationFrame ToReadOnlyValidationFrame(IValidationFrame? validationFrame)
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
