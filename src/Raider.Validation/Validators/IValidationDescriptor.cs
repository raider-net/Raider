using System;
using System.Collections;
using System.Collections.Generic;

namespace Raider.Validation
{
	public interface IValidationDescriptor
	{
		IValidationFrame ValidationFrame { get; }

		ValidatorType ValidatorType { get; }

		bool Conditional { get; }

		IReadOnlyList<IValidationDescriptor> Validators { get; }

		//Equal, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual
		IComparable? ValueToCompare { get; }

		//Equal, NotEqual
		IEqualityComparer? Comparer { get; }

		//ExclusiveBetween, InclusiveBetween
		IComparable? From { get; }

		//ExclusiveBetween, InclusiveBetween
		IComparable? To { get; }

		//Length
		int MinLength { get; }

		//Length
		int MaxLength { get; }

		//PrecisionScaleDecimal
		int Scale { get; }

		//PrecisionScaleDecimal
		int Precision { get; }

		//PrecisionScaleDecimal
		bool IgnoreTrailingZeros { get; }

		//RegEx
		string? Pattern { get; }
	}
}
