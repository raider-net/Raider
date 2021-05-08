using Raider.Validation.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Raider.Validation
{
	public interface IValidationDescriptor
	{
		Type ObjectType { get; }

		IValidationFrame ValidationFrame { get; }

		ValidatorType ValidatorType { get; }

		string ValidatorTypeInfo { get; }

		bool Conditional { get; }

		IClientConditionDefinition? ClientConditionDefinition { get; }

		IReadOnlyList<IValidationDescriptor> Validators { get; }

		//DefaultOrEmpty, NotDefaultOrEmpty
		object? DefaultValue { get; }

		//Equal, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual
		IComparable? ValueToCompare { get; }

		//MultiEqual, MultiNotEqual
		IEnumerable<IComparable?>? ValuesToCompare { get; }

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

		string Message { get; }

		string MessageWithPropertyName { get; }

		bool IsEqualTo(IValidationDescriptor other);

		string Print();

		internal void PrintInternal(StringBuilder sb, int indent);
	}
}
