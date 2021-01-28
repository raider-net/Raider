using System;
using System.Collections;
using System.Collections.Generic;

namespace Raider.Validation.Internal
{
	internal class ValidationDescriptor : IValidationDescriptor
	{
		public IValidationFrame ValidationFrame {get; }

		public ValidatorType ValidatorType {get; }

		public bool Conditional {get; }

		public List<IValidationDescriptor> Validators {get; }

		IReadOnlyList<IValidationDescriptor> IValidationDescriptor.Validators => Validators;

		//Equal, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual
		public IComparable? ValueToCompare {get; set; }

		//Equal, NotEqual
		public IEqualityComparer? Comparer {get; set; }

		//ExclusiveBetween, InclusiveBetween
		public IComparable? From {get; set; }

		//ExclusiveBetween, InclusiveBetween
		public IComparable? To {get; set; }

		//Length
		public int MinLength {get; set; }

		//Length
		public int MaxLength {get; set; }

		//PrecisionScaleDecimal
		public int Scale {get; set; }

		//PrecisionScaleDecimal
		public int Precision {get; set; }

		//PrecisionScaleDecimal
		public bool IgnoreTrailingZeros {get; set; }

		//RegEx
		public string? Pattern {get; set; }

		public ValidationDescriptor(IValidationFrame validationFrame, ValidatorType validatorType, bool conditional)
		{
			ValidationFrame = validationFrame;
			ValidatorType = validatorType;
			Conditional = conditional;
			Validators = new List<IValidationDescriptor>();
		}

		public ValidationDescriptor AddValidators(IEnumerable<IValidator>? validators)
		{
			if (validators == null)
				return this;

			foreach (var validator in validators)
			{
				var desc = validator.ToDescriptor();
				if (desc != null)
					Validators.Add(desc);
			}

			return this;
		}
	}
}
