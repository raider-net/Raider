using Raider.Extensions;
using Raider.Validation.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raider.Validation.Internal
{
	internal class ValidationDescriptor : IValidationDescriptor
	{
		public Type ObjectType { get; }

		public IValidationFrame ValidationFrame {get; }

		public ValidatorType ValidatorType {get; }

		public string ValidatorTypeInfo { get; }

		public bool Conditional {get; }

		public IClientConditionDefinition? ClientConditionDefinition { get; }

		public List<IValidationDescriptor> Validators {get; }

		IReadOnlyList<IValidationDescriptor> IValidationDescriptor.Validators => Validators;

		//Equal, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual
		public IComparable? ValueToCompare {get; set; }

		//MultiEqual, MultiNotEqual
		public IEnumerable<IComparable?>? ValuesToCompare { get; set; }

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

		public ValidationDescriptor(Type objectType, IValidationFrame validationFrame, ValidatorType validatorType, string validatorTypeInfo, bool conditional, IClientConditionDefinition? clientConditionDefinition)
		{
			ObjectType = objectType ?? throw new ArgumentNullException(nameof(objectType));
			ValidationFrame = validationFrame;
			ValidatorType = validatorType;
			ValidatorTypeInfo = validatorTypeInfo;
			Conditional = conditional;
			ClientConditionDefinition = clientConditionDefinition;
			Validators = new List<IValidationDescriptor>();
		}

		public ValidationDescriptor AddValidators(IEnumerable<ValidatorBase>? validators)
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

		public bool IsEqualTo(IValidationDescriptor other)
		{
			if (other == null)
				return false;

			if (ObjectType != other.ObjectType
				|| ValidationFrame.ToString() != other.ValidationFrame.ToString()
				|| ValidatorType != other.ValidatorType
				|| Conditional != other.Conditional
				|| !Equals(ValueToCompare, other.ValueToCompare)
				|| !Equals(ValuesToCompare, other.ValuesToCompare) //TODO uprav
				|| !Equals(Comparer, other.Comparer)
				|| !Equals(From, other.From)
				|| !Equals(To, other.To)
				|| MinLength != other.MinLength
				|| MaxLength != other.MaxLength
				|| Scale != other.Scale
				|| Precision != other.Precision
				|| IgnoreTrailingZeros != other.IgnoreTrailingZeros
				|| Pattern != other.Pattern
				|| Validators.Count != other.Validators.Count)
				return false;

			for (int i = 0; i < Validators.Count; i++)
				if (!Validators[i].IsEqualTo(other.Validators[i]))
					return false;

			return true;
		}

		public string Print()
		{
			var sb = new StringBuilder();
			
			sb.AppendLine("_____________________________________________");
			PrintInternal(sb, 0);
			sb.AppendLine("_____________________________________________");

			return sb.ToString();
		}

		private void PrintInternal(StringBuilder sb, int indent)
		{
			if (0 < indent)
				sb.Append(string.Join("", Enumerable.Range(1, indent).Select(x => "    ")));

			sb.Append($"{ValidatorType}<{ObjectType?.FullName?.GetLastSplitSubstring(".")}> | {ValidationFrame} | Conditional={Conditional} | Validators={Validators.Count}");

			sb.AppendLine();

			foreach (var validator in Validators)
				validator.PrintInternal(sb, indent + 1);
		}

		void IValidationDescriptor.PrintInternal(StringBuilder sb, int indent)
			=> PrintInternal(sb, indent);

		public override string ToString()
		{
			return $"{ValidatorType}<{ObjectType?.FullName?.GetLastSplitSubstring(".")}> | {ValidationFrame} | Conditional={Conditional} | Validators={Validators.Count}";
		}
	}
}
