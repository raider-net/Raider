using Raider.Validation.Client;
using System;

namespace Raider.Validation
{
	internal class ValidationFailure : IValidationFailure
	{
		public IValidationFrame ValidationFrame { get; }
		public ValidatorType Type { get; }
		public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
		public string Message { get; internal set; }
		public string MessageWithPropertyName { get; internal set; }
		public bool Conditional { get; }
		public IClientConditionDefinition? ClientConditionDefinition { get; }

		public ValidationFailure(
			IValidationFrame validationFrame,
			ValidatorType type,
			bool conditional,
			IClientConditionDefinition? clientConditionDefinition,
			string message,
			string messageWithPropertyName)
		{
			ValidationFrame = validationFrame ?? throw new ArgumentNullException(nameof(validationFrame));

			Type = type;
			Conditional = conditional;
			ClientConditionDefinition = clientConditionDefinition;

			if (string.IsNullOrWhiteSpace(message))
				throw new ArgumentNullException(nameof(message));

			Message = message;
			MessageWithPropertyName = messageWithPropertyName;
		}

		public override string ToString()
			=> $"{ValidationFrame}: {Type}: {MessageWithPropertyName}";
	}
}
