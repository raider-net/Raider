using System;
using System.Collections.Generic;

namespace Raider.Validation
{
	public class ValidationResult
	{
		private readonly List<IValidationFailure> _errors = new List<IValidationFailure>();

		public IReadOnlyList<IValidationFailure> Errors => _errors;

		public bool Interrupted { get; set; }

		public ValidationResult()
		{
		}

		public ValidationResult(IValidationFailure failure)
		{
			if (failure == null)
				throw new ArgumentNullException(nameof(failure));

			_errors.Add(failure);
		}

		internal ValidationResult AddFailure(IValidationFailure? failure)
		{
			if (failure != null)
				_errors.Add(failure);

			return this;
		}

		internal void Merge(ValidationResult result)
		{
			if (result == null)
				return;

			foreach (var error in result.Errors)
				AddFailure(error);
		}
	}
}
