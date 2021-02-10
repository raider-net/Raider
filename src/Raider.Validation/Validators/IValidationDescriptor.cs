using Raider.Validation.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Raider.Validation
{
	public interface IValidationDescriptor
	{
		public Type ObjectType { get; }

		IValidationFrame ValidationFrame { get; }

		ValidatorType ValidatorType { get; }

		public string ValidatorTypeInfo { get; }

		bool Conditional { get; }

		IClientConditionDefinition? ClientConditionDefinition { get; }

		IReadOnlyList<IValidationDescriptor> Validators { get; }

		//Equal, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual
		IComparable? ValueToCompare { get; }

		//MultiEqual, MultiNotEqual
		public IEnumerable<IComparable?>? ValuesToCompare { get; }

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

		bool IsEqualTo(IValidationDescriptor other);

		string Print();

		internal void PrintInternal(StringBuilder sb, int indent);
	}
}
