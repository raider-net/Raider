using Raider.Validation.Client;

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
		IClientConditionDefinition? ClientConditionDefinition { get; }
	}
}
