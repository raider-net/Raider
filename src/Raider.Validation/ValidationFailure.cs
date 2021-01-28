using System;

namespace Raider.Validation
{
	public interface IValidationFailure
	{
		IValidationFrame ValidationFrame { get; }
		ValidatorType Type { get; }
		ValidationSeverity Severity { get; }
		string Message { get; }
		string MessageWithPropertyName { get; }
		bool Conditional { get; }
	}

	public class ValidationFailure : IValidationFailure
	{
		public IValidationFrame ValidationFrame { get; }
		public ValidatorType Type { get; }
		public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
		public string Message { get; internal set; }
		public string MessageWithPropertyName { get; internal set; }
		public bool Conditional { get; }

		public ValidationFailure(IValidationFrame validationFrame, IValidator validator, string message, string messageWithPropertyName)
		{
			ValidationFrame = validationFrame ?? throw new ArgumentNullException(nameof(validationFrame));

			if (validator == null)
				throw new ArgumentNullException(nameof(validator));

			Type = validator.ValidatorType;
			Conditional = validator.Conditional;

			if (string.IsNullOrWhiteSpace(message))
				throw new ArgumentNullException(nameof(message));

			Message = message;
			MessageWithPropertyName = messageWithPropertyName;
		}

		public override string ToString()
			=> $"{ValidationFrame}: {Type}: {MessageWithPropertyName}";
	}
}
